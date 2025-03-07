using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AHD2TimeOfDay
{
    public class DensityAndLightingPass : ScriptableRenderPass
    {
        #region Variables

        private static readonly int _DensityBuffer = Shader.PropertyToID("_DensityBuffer");
        private static readonly int _DownBuffer = Shader.PropertyToID("_DownBuffer");
        private static readonly int _HistoryScatterBuffer = Shader.PropertyToID("_HistoryScatterBuffer");
        private static readonly int _ScatterBuffer = Shader.PropertyToID("_ScatterBuffer");
        //分为三个pass，计算每个体积纹素的密度和光照（吸收+内散射），计算总的透射率和内散射，降采样
        private ProfilingSampler _volumetricFogSampler = new ProfilingSampler("VolumetricFog Pass");
        //private RenderTexture _densityTexture;
        private RenderTargetIdentifier _DensityBufferID;
        private RenderTargetIdentifier _ScatterBufferID;
        private RenderTextureDescriptor densityTexDesc;
        private ComputeShader _densityAndLightingComputeShader;
        //3D纹理分辨率
        private int _texScreenWidth = 1920;
        private int _texScreenHeight = 1080;
        //双缓冲
        private RenderTexture _scatterBuffer;
        private RenderTexture _historyScatterBuffer;
        //
        private bool _fogEnable;
        private GlobalKeyword _fogKeyword;
        
        //全局雾密度
        private float _fogDensity;
        //体积雾视锥体远平面（体积雾最远距离）
        private float _fogFarPlaneDistance;
        //体积雾深度切片线性程度
        //体积雾介质吸收系数
        private float _fogExtinctonCoeffient;
        //雾起始高度
        private float _fogStartHeight;
        private static readonly int Ahd2HgCoefficient = Shader.PropertyToID("AHD2_HGCoefficient");

        #endregion
        

        public DensityAndLightingPass(VolumetricFogRendererFeature.VolumetricFogSettings volumetricFogSettings)
        {
            this.renderPassEvent = volumetricFogSettings.renderPassEvent;
            _densityAndLightingComputeShader = volumetricFogSettings.densityAndLightingComputeShader;
            _fogEnable = volumetricFogSettings.enable;
            _fogKeyword = GlobalKeyword.Create("VOLUMETRICFOG_ON");

            //雾参数
            _fogDensity = volumetricFogSettings.fogDensity;
            _fogFarPlaneDistance = volumetricFogSettings.fogFarPlaneDistance;
            _fogExtinctonCoeffient = volumetricFogSettings.fogExtinctonCoeffient;
            _fogStartHeight = volumetricFogSettings.fogStartHeight;
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var targetCamera = renderingData.cameraData.camera;
            // 1. 获取垂直视场角（弧度）
            float verticalFoV = targetCamera.GetGateFittedFieldOfView() * Mathf.Deg2Rad;
            // 2. 获取镜头偏移（物理相机）
            Vector2 lensShift = targetCamera.GetGateFittedLensShift();
            // 3. 获取世界到视图矩阵
            Matrix4x4 worldToViewMatrix = targetCamera.worldToCameraMatrix;
            // 4. 判断是否是正交投影
            bool isOrthographic = targetCamera.orthographic;
            // 渲染目标的分辨率
            Vector4 screenSize = new Vector4(
                _texScreenWidth,          // 宽度
                _texScreenHeight,         // 高度
                1.0f / _texScreenWidth,   // 1/宽度
                1.0f / _texScreenHeight   // 1/高度
            );
            Matrix4x4 vBufferCoordToWorldDir = ComputePixelCoordToWorldSpaceViewDirectionMatrix(
                verticalFoV,
                lensShift,
                screenSize,
                worldToViewMatrix,
                false,
                aspectRatio: -1,         // 自动计算宽高比
                isOrthographic: isOrthographic
            );
            Shader.SetGlobalMatrix("_VBufferCoordToViewDirWS", vBufferCoordToWorldDir);
            
            // //深度编码信息
            // float sliceDistributionUniformity = 0.8f;//用户控制，范围0-1
            // float c = 2 - 2 * sliceDistributionUniformity;
            // c = Mathf.Max(c, 0.001f); // 防止除零或无效值
            var depthDecodingParams = ComputeLogarithmicDepthDecodingParams(targetCamera.nearClipPlane, _fogFarPlaneDistance, 2f);
            Shader.SetGlobalVector("_VBufferDistanceDecodingParams", depthDecodingParams);
            var depthEncodingParams = ComputeLogarithmicDepthEncodingParams(targetCamera.nearClipPlane, _fogFarPlaneDistance, 2f);
            Shader.SetGlobalVector("_VBufferDistanceEncodingParams", depthEncodingParams);
            Shader.SetGlobalFloat("_FogFarPlaneDistance", _fogFarPlaneDistance);
            Shader.SetGlobalFloat("_FogStartHeight", _fogStartHeight);
            Shader.SetGlobalFloat("_FogDensity", _fogDensity);
            CommandBuffer cmd = CommandBufferPool.Get();
            //cmd.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);
            // 计算线程组大小
            int threadGroupsX = Mathf.Max(1, Mathf.CeilToInt(_texScreenWidth / 8.0f)); // 确保至少为 1
            int threadGroupsY = Mathf.Max(1, Mathf.CeilToInt(_texScreenHeight / 8.0f)); // 确保至少为 1
            int blurthreadGroupsX = Mathf.Max(1, Mathf.CeilToInt(_texScreenWidth / 64.0f));
            int blurthreadGroupsY = Mathf.Max(1, Mathf.CeilToInt(_texScreenHeight / 2.0f));
            int blurthreadGroupsZ = Mathf.Max(1, Mathf.CeilToInt(128 / 2.0f));
            using (new ProfilingScope(cmd, _volumetricFogSampler))
            {
                //int threadGroupsZ = 128; // 3D 纹理的深度
                int densityAndLightingCSKernel = _densityAndLightingComputeShader.FindKernel("DensityAndLighting");
                cmd.SetComputeFloatParam(_densityAndLightingComputeShader, Ahd2HgCoefficient, Shader.GetGlobalFloat(Ahd2HgCoefficient));
                cmd.SetComputeFloatParam(_densityAndLightingComputeShader, "_FogStartHeight", _fogStartHeight);
                cmd.SetComputeFloatParam(_densityAndLightingComputeShader, "_FogDensity", _fogDensity);
                cmd.SetComputeTextureParam(_densityAndLightingComputeShader, densityAndLightingCSKernel, _DensityBuffer, _DensityBufferID);
                cmd.SetComputeMatrixParam(_densityAndLightingComputeShader,  "unity_MatrixV", renderingData.cameraData.GetViewMatrix());
                cmd.SetComputeVectorParam(_densityAndLightingComputeShader, "_InvTextureSize", new Vector4(1.0f/_texScreenWidth, 1.0f/_texScreenHeight, 1.0f/128, 0));
                cmd.DispatchCompute(_densityAndLightingComputeShader, densityAndLightingCSKernel, threadGroupsX, threadGroupsY, 1);
                
                //计算散射
                int scatterCSKernel = _densityAndLightingComputeShader.FindKernel("Scatter");
                cmd.SetComputeFloatParam(_densityAndLightingComputeShader, "_FogExtinctonCoeffient", _fogExtinctonCoeffient);
                cmd.SetComputeTextureParam(_densityAndLightingComputeShader, scatterCSKernel, _DensityBuffer, _DensityBufferID);
                cmd.SetComputeTextureParam(_densityAndLightingComputeShader, scatterCSKernel, _ScatterBuffer, _scatterBuffer);
                cmd.DispatchCompute(_densityAndLightingComputeShader, scatterCSKernel, threadGroupsX, threadGroupsY, 1);
                
                //滤波
                //先降采样
                // int downSampleCSKernel = _densityAndLightingComputeShader.FindKernel("DownSample");
                // cmd.SetComputeTextureParam(_densityAndLightingComputeShader, downSampleCSKernel, "_ScatterBuffer1", _ScatterBuffer);
                // cmd.SetComputeTextureParam(_densityAndLightingComputeShader, downSampleCSKernel, _DownBuffer, _downBuffer);
                // cmd.SetComputeVectorParam(_densityAndLightingComputeShader, "_InvTextureSize", new Vector4(2.0f/_texScreenWidth, 2.0f/_texScreenHeight, 1.0f/64, 0));
                // cmd.DispatchCompute(_densityAndLightingComputeShader, downSampleCSKernel, halfthreadGroupsX, halfthreadGroupsY, 8);
                
                //再高斯模糊+历史帧混合
                //X方向模糊
                int gaussianXCSKernel = _densityAndLightingComputeShader.FindKernel("GaussianX");
                cmd.SetComputeTextureParam(_densityAndLightingComputeShader, gaussianXCSKernel, _DownBuffer, _scatterBuffer);
                cmd.SetComputeIntParam(_densityAndLightingComputeShader, "_XEdge", densityTexDesc.width);
                cmd.DispatchCompute(_densityAndLightingComputeShader, gaussianXCSKernel, Mathf.CeilToInt(_texScreenWidth / 128.0f), 
                    Mathf.CeilToInt(_texScreenHeight), 
                    Mathf.CeilToInt(128));
                //Y方向模糊
                int gaussianYCSKernel = _densityAndLightingComputeShader.FindKernel("GaussianY");
                cmd.SetComputeTextureParam(_densityAndLightingComputeShader, gaussianYCSKernel, _DownBuffer, _scatterBuffer);
                cmd.SetComputeIntParam(_densityAndLightingComputeShader, "_YEdge", densityTexDesc.height);
                cmd.DispatchCompute(_densityAndLightingComputeShader, gaussianYCSKernel, Mathf.CeilToInt(_texScreenWidth), 
                    Mathf.CeilToInt(_texScreenHeight / 128.0f), 
                    Mathf.CeilToInt(128 ));
                
                cmd.SetGlobalTexture(_ScatterBuffer, _scatterBuffer);
            }
            //context.ExecuteCommandBufferAsync(cmd, ComputeQueueType.Default);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //Debug.Log("测试OnCameraSetup是否每帧调用");
            _texScreenWidth = renderingData.cameraData.scaledWidth;
            _texScreenWidth /= 8;
            _texScreenHeight = renderingData.cameraData.scaledHeight;
            _texScreenHeight /= 8 ;

            _DensityBufferID = new RenderTargetIdentifier(_DensityBuffer);
            _ScatterBufferID = new RenderTargetIdentifier(_ScatterBuffer);
            
            densityTexDesc =
                new RenderTextureDescriptor(_texScreenWidth , _texScreenHeight, RenderTextureFormat.ARGB64);
            densityTexDesc.dimension = TextureDimension.Tex3D;
            densityTexDesc.volumeDepth = 128;
            densityTexDesc.enableRandomWrite = true;
            
            //降采样
            RenderTextureDescriptor downSampleTexDesc =
                new RenderTextureDescriptor(_texScreenWidth/2 , _texScreenHeight/2, RenderTextureFormat.ARGB64);
            downSampleTexDesc.dimension = TextureDimension.Tex3D;
            downSampleTexDesc.volumeDepth = 64;
            downSampleTexDesc.enableRandomWrite = true;
            //如果双缓冲buffer没有初始化，就初始化
            if (!_scatterBuffer)
            {
                _scatterBuffer = new RenderTexture(densityTexDesc);
                _scatterBuffer.Create();
            }else if (densityTexDesc.width != _scatterBuffer.width || densityTexDesc.height != _scatterBuffer.height)
            {
                //如果rt分辨率改变，rt也改变（这个不会经常触发）
                _scatterBuffer.Release();
                _scatterBuffer = new RenderTexture(densityTexDesc);
                _scatterBuffer.Create();
            }

            if (!_historyScatterBuffer)
            {
                _historyScatterBuffer = new RenderTexture(densityTexDesc);
                _historyScatterBuffer.Create();
            }else if (densityTexDesc.width != _historyScatterBuffer.width || densityTexDesc.height != _historyScatterBuffer.height)
            {
                //如果rt分辨率改变，rt也改变（这个不会经常触发）
                _historyScatterBuffer.Release();
                _historyScatterBuffer = new RenderTexture(densityTexDesc);
                _historyScatterBuffer.Create();
            }
            
            cmd.GetTemporaryRT(_DensityBuffer, densityTexDesc);//相当于同时设置成了全局纹理（除了ComputeShader还要自己设置）
            
            Shader.SetKeyword(_fogKeyword, _fogEnable);
        }
        
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_DensityBuffer);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            //Debug.Log("测试FrameCleanup是否每帧调用");
            //交换两个buffer的指针
            (_historyScatterBuffer, _scatterBuffer) = (_scatterBuffer, _historyScatterBuffer);
        }

        public void Dispose()
        {
            if (_scatterBuffer)
            {
                _scatterBuffer.Release();
            }

            if (_historyScatterBuffer)
            {
                _historyScatterBuffer.Release();
            }
            Shader.DisableKeyword("VOLUMETRICFOG_ON");
        }

        //辅助函数
        #region UtilsFun
        internal static Matrix4x4 ComputePixelCoordToWorldSpaceViewDirectionMatrix(float verticalFoV, Vector2 lensShift, Vector4 screenSize, Matrix4x4 worldToViewMatrix, bool renderToCubemap, float aspectRatio = -1, bool isOrthographic = false)
        {
            Matrix4x4 viewSpaceRasterTransform;

            if (isOrthographic)
            {
                // For ortho cameras, project the skybox with no perspective
                // the same way as builtin does (case 1264647)
                viewSpaceRasterTransform = new Matrix4x4(
                    new Vector4(-2.0f * screenSize.z, 0.0f, 0.0f, 0.0f),
                    new Vector4(0.0f, -2.0f * screenSize.w, 0.0f, 0.0f),
                    new Vector4(1.0f, 1.0f, -1.0f, 0.0f),
                    new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
            }
            else
            {
                // Compose the view space version first.
                // V = -(X, Y, Z), s.t. Z = 1,
                // X = (2x / resX - 1) * tan(vFoV / 2) * ar = x * [(2 / resX) * tan(vFoV / 2) * ar] + [-tan(vFoV / 2) * ar] = x * [-m00] + [-m20]
                // Y = (2y / resY - 1) * tan(vFoV / 2)      = y * [(2 / resY) * tan(vFoV / 2)]      + [-tan(vFoV / 2)]      = y * [-m11] + [-m21]

                aspectRatio = aspectRatio < 0 ? screenSize.x * screenSize.w : aspectRatio;
                float tanHalfVertFoV = Mathf.Tan(0.5f * verticalFoV);

                // Compose the matrix.
                float m21 = (1.0f - 2.0f * lensShift.y) * tanHalfVertFoV;
                float m11 = -2.0f * screenSize.w * tanHalfVertFoV;

                float m20 = (1.0f - 2.0f * lensShift.x) * tanHalfVertFoV * aspectRatio;
                float m00 = -2.0f * screenSize.z * tanHalfVertFoV * aspectRatio;

                if (renderToCubemap)
                {
                    // Flip Y.
                    m11 = -m11;
                    m21 = -m21;
                }

                viewSpaceRasterTransform = new Matrix4x4(new Vector4(m00, 0.0f, 0.0f, 0.0f),
                    new Vector4(0.0f, m11, 0.0f, 0.0f),
                    new Vector4(m20, m21, -1.0f, 0.0f),
                    new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            }

            // Remove the translation component.
            var homogeneousZero = new Vector4(0, 0, 0, 1);
            worldToViewMatrix.SetColumn(3, homogeneousZero);

            // Flip the Z to make the coordinate system left-handed.
            worldToViewMatrix.SetRow(2, -worldToViewMatrix.GetRow(2));

            // Transpose for HLSL.
            return Matrix4x4.Transpose(worldToViewMatrix.transpose * viewSpaceRasterTransform);
        }
        
        // See DecodeLogarithmicDepthGeneralized().
        static Vector4 ComputeLogarithmicDepthDecodingParams(float nearPlane, float farPlane, float c)
        {
            Vector4 depthParams = new Vector4();

            float n = nearPlane;
            float f = farPlane;

            depthParams.x = 1.0f / c;
            depthParams.y = Mathf.Log(c * (f - n) + 1, 2);
            depthParams.z = n - 1.0f / c; // Same
            depthParams.w = 0.0f;

            return depthParams;
        }
        // See EncodeLogarithmicDepthGeneralized().
        static Vector4 ComputeLogarithmicDepthEncodingParams(float nearPlane, float farPlane, float c)
        {
            Vector4 depthParams = new Vector4();

            float n = nearPlane;
            float f = farPlane;

            depthParams.y = 1.0f / Mathf.Log(c * (f - n) + 1, 2);
            depthParams.x = Mathf.Log(c, 2) * depthParams.y;
            depthParams.z = n - 1.0f / c; // Same
            depthParams.w = 0.0f;

            return depthParams;
        }
        
        #endregion
        
    }
}