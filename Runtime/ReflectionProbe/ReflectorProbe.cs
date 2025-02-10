using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AHD2TimeOfDay
{
    [RequireComponent(typeof(ReflectionProbe)), ExecuteInEditMode]
    public partial class ReflectorProbe : MonoBehaviour//, ISectorizable
    {
        private static HashSet<ReflectorProbe> instances = new HashSet<ReflectorProbe>();
        public static ReflectorProbe[] Instances
        {
            get { return instances.ToArray(); }
        }

        private static Camera renderCamera;

        private static Material mirror = null;

        private static Quaternion[] orientations = new Quaternion[]
        {
            Quaternion.LookRotation(Vector3.right, Vector3.down),
            Quaternion.LookRotation(Vector3.left, Vector3.down),
            Quaternion.LookRotation(Vector3.up, Vector3.forward),
            Quaternion.LookRotation(Vector3.down, Vector3.back),
            Quaternion.LookRotation(Vector3.forward, Vector3.down),
            Quaternion.LookRotation(Vector3.back, Vector3.down)
        };

        private RenderTexture cubemap;

        public GameObject GameObject
        {
            get { return gameObject; }
        }

        [SerializeField]
        public bool bakeable = false;

        [SerializeField]
        private Texture baked;

        public Texture Baked
        {
            get { return baked; }
        }
        [SerializeField]
        private Texture bakedNormal;

        public Texture BakedNormal
        {
            get { return bakedNormal; }
        }
        //渲染天空盒相机vp矩阵SO
        public BakeCameraData BakeCameraData;
        public TODGlobalParameters TodGlobalParameters;

        [SerializeField]
        private Camera customCamera;

        private Camera customCameraInstance;
        private Camera externalCamera;
        
        private ReflectionProbe probe;
        private ComputeShader RelightCS;
        public class Coefficience : ScriptableObject
        {
            public float3[] coefficiencesArray;
        }
        private void OnEnable()
        {
            instances.Add(this);
            
            probe = GetComponent<ReflectionProbe>();
            probe.hideFlags = HideFlags.None;
            probe.mode = ReflectionProbeMode.Custom;
            probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            
            CreateCubemap();
            
            coefficience = ScriptableObject.CreateInstance<Coefficience>();
            int resolution = probe.resolution;
            sphericalHarmonicsComputeShader = TodGlobalParameters.TimeOfDayData.shaders.sphericalHarmonicsCS;
            RelightCS = TodGlobalParameters.TimeOfDayData.shaders.relightCS;
            skyboxmesh = TodGlobalParameters.TimeOfDayData.meshes.skyboxmesh;
            //初始化skyboxRT（不懂要不要释放）
            if (skyboxmap == null)
            {
                skyboxmap = new RenderTexture(resolution, resolution, 0,RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                skyboxmap.useMipMap = false;
                skyboxmap.Create();
            }
            if (skyboxmapmirror == null)
            {
                skyboxmapmirror = new RenderTexture(resolution, resolution, 0,RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                skyboxmapmirror.useMipMap = true;
                skyboxmapmirror.autoGenerateMips = false;
                skyboxmapmirror.enableRandomWrite = true;
                skyboxmapmirror.Create();
            }
            if (mirror == null)
            {
                mirror = new Material(Shader.Find("Hidden/ReflectorProbe/Mirror"));
            }

#if UNITY_EDITOR
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(probe, false);
#endif
        }
        ProfilingSampler m_ProfilingSampler = new ProfilingSampler("RenderIBL");
        private RenderTexture skyboxmap;
        private RenderTexture skyboxmapmirror;
        private Mesh skyboxmesh;
        private void Update()
        {
            var cmd = CommandBufferPool.Get();
            for (int i = 0; i < 6; i++)
            {
                using (new ProfilingScope(cmd, m_ProfilingSampler)){
                    cmd.SetRenderTarget(skyboxmap);
                    cmd.SetViewProjectionMatrices(BakeCameraData.faceMatrices[i], BakeCameraData.PMatrix4X4);
                    LocalKeyword renderingReflectorProbe = new LocalKeyword(RenderSettings.skybox.shader, "_REFLECTOR_RENDERING");
                    cmd.EnableKeyword(RenderSettings.skybox, renderingReflectorProbe);//必须用cmd不然不会和drawmesh同步
                    cmd.DrawMesh(skyboxmesh, Matrix4x4.identity, RenderSettings.skybox);
                    cmd.DisableKeyword(RenderSettings.skybox, renderingReflectorProbe);
                }
                Graphics.ExecuteCommandBuffer(cmd);
                Graphics.Blit(skyboxmap,skyboxmapmirror,mirror);
                
                //先找核
                int index = RelightCS.FindKernel("CSMain");
                int resolution = probe.resolution;
                RelightCS.SetTexture(index, ShaderConstants.DiffuseTexture, baked);
                RelightCS.SetTexture(index, ShaderConstants.NormalTexture, bakedNormal);
                RelightCS.SetTexture(index, ShaderConstants.SkyboxTexture, skyboxmapmirror);
                RelightCS.SetInt(ShaderConstants.FaceIndex, i);
                RelightCS.SetInt(ShaderConstants.Resolution, resolution);
                RelightCS.Dispatch(index, resolution / 8,resolution / 8,1);
                skyboxmapmirror.GenerateMips();
                Graphics.CopyTexture(skyboxmapmirror, 0, cubemap, i);
                
                cmd.Clear();
            }
#if UNITY_EDITOR
            //防止framedebug因为异步操作频繁抖动
            // 检测编辑器是否暂停
            if (FrameDebugger.enabled)
            {
                // 暂停时停止异步操作
                AsyncGPUReadback.WaitAllRequests(); // 等待所有异步操作完成
            }
            else
            {
                Calculate();
            }
#else
            Calculate();
#endif
            Shader.SetGlobalTexture(ShaderConstants.AHD2_SpecCube0, cubemap);
        }
        
        struct CoeffsPack3
        {
            public float3 coeff0;
            public float3 coeff1;
            public float3 coeff2;

            public CoeffsPack3(float x)
            {
                coeff0 = new float3(x);
                coeff1 = new float3(x);
                coeff2 = new float3(x);
            }
        }

        private CoeffsPack3 SumCoeffs(NativeArray<CoeffsPack3> coeffsArray)
        {
            CoeffsPack3 tempCoeffs = new CoeffsPack3(0);
            for (int i = 0; i < coeffsArray.Length; i++)
            {
                tempCoeffs.coeff0 += coeffsArray[i].coeff0;
                tempCoeffs.coeff1 += coeffsArray[i].coeff1;
                tempCoeffs.coeff2 += coeffsArray[i].coeff2;
            }
            return tempCoeffs;
        }

        private ComputeShader sphericalHarmonicsComputeShader;
        private Coefficience coefficience;
        private ComputeBuffer pack3Buffer;
        private CoeffsPack3[] pack3Array;
        private int shPackIndex = 0; // 用于跟踪当前计算的球谐参数索引
        private bool isCalculating = false; // 用于标记是否正在计算
        private int kernel;
        private Vector3Int dispatchCounts;
        private void Calculate()
        {
            if (isCalculating)
                return; // 如果正在计算，则直接返回
            isCalculating = true;
            shPackIndex = 0;
            
            coefficience.coefficiencesArray = new float3[9];

            kernel = sphericalHarmonicsComputeShader.FindKernel("CalculateSHMain");
            sphericalHarmonicsComputeShader.GetKernelThreadGroupSizes(kernel, out uint x, out uint y, out uint z);
            dispatchCounts = new Vector3Int(Mathf.CeilToInt((float)cubemap.width / (float)x),
                                                        Mathf.CeilToInt((float)cubemap.height / (float)y),
                                                        Mathf.CeilToInt((float)6 / (float)z));
            
            if (pack3Buffer != null)
            {
                pack3Buffer.Release();
            }
            pack3Buffer = new ComputeBuffer(dispatchCounts.x * dispatchCounts.y * dispatchCounts.z, 3 * 3 * 4, ComputeBufferType.Structured);
            pack3Array = new CoeffsPack3[dispatchCounts.x * dispatchCounts.y * dispatchCounts.z];
            for (int i = 0; i < dispatchCounts.x * dispatchCounts.y * dispatchCounts.z; i++)
            {
                pack3Array[i] = new CoeffsPack3(0);
            }
            pack3Buffer.SetData(pack3Array);

            sphericalHarmonicsComputeShader.SetTexture(kernel, ShaderConstants.CubeMapTexture, cubemap);
            sphericalHarmonicsComputeShader.SetBuffer(kernel, ShaderConstants.GroupCoefficients, pack3Buffer);
            sphericalHarmonicsComputeShader.SetVector(ShaderConstants.DispatchCount, (Vector3)dispatchCounts);
            sphericalHarmonicsComputeShader.SetVector(ShaderConstants.TextureSize, new Vector3(cubemap.width, cubemap.height, 6));
            CalculateNextSH();
        }
        
        private void CalculateNextSH()
        {
            if (shPackIndex >= 3)
            {
                SetBuffer();//计算完成再setbuffer，避免闪烁
                // 计算完成，释放资源
                pack3Buffer.Release();
                isCalculating = false;
                return;
            }
            sphericalHarmonicsComputeShader.SetFloat(ShaderConstants.SHPackIndex, shPackIndex);
            sphericalHarmonicsComputeShader.Dispatch(kernel, dispatchCounts.x, dispatchCounts.y, dispatchCounts.z);
            AsyncGPUReadback.Request(pack3Buffer, OnReadbackComplete);
        }
        private void OnReadbackComplete(AsyncGPUReadbackRequest request)
        {
            if (request.hasError)
            {
                Debug.LogError("GPU readback error detected.");
                return;
            }
            //Debug.Log("计算出了三个球谐系数");
            // 获取读取的数据
            NativeArray<CoeffsPack3> array = request.GetData<CoeffsPack3>();
            CoeffsPack3 tempCoeffs = SumCoeffs(array);//cpu求和
            
            float inv4PI = 1.0f / Mathf.PI;
            coefficience.coefficiencesArray[shPackIndex * 3 + 0] = tempCoeffs.coeff0 * inv4PI;
            coefficience.coefficiencesArray[shPackIndex * 3 + 1] = tempCoeffs.coeff1 * inv4PI;
            coefficience.coefficiencesArray[shPackIndex * 3 + 2] = tempCoeffs.coeff2 * inv4PI;
            
            // 计算下一个球谐参数
            shPackIndex++;
            CalculateNextSH();
        }
        private Vector4[] shArray = new Vector4[7];
        
        private const float C1 = 0.429043f;
        private const float C2 = 0.511664f;
        private const float C3 = 0.743125f;
        private const float C4 = 0.886227f;
        private const float C5 = 0.247708f;

        private void SetBuffer()
        {
            //0是00 1是1-1 2是10 3是11 4是2-2 5是2-1 6是20 7是21 8是22 
            shArray[0] = new Vector4(2 * C2 * coefficience.coefficiencesArray[3].x,
                2 * C2 * coefficience.coefficiencesArray[1].x,
                2 * C2 * coefficience.coefficiencesArray[2].x, 
                C4 * coefficience.coefficiencesArray[0].x - C5 * coefficience.coefficiencesArray[6].x);
            shArray[1] = new Vector4(2 * C2 * coefficience.coefficiencesArray[3].y,
                2 * C2 * coefficience.coefficiencesArray[1].y,
                2 * C2 * coefficience.coefficiencesArray[2].y, 
                C4 * coefficience.coefficiencesArray[0].y - C5 * coefficience.coefficiencesArray[6].y);
            shArray[2] = new Vector4(2 * C2 * coefficience.coefficiencesArray[3].z,
                2 * C2 * coefficience.coefficiencesArray[1].z,
                2 * C2 * coefficience.coefficiencesArray[2].z, 
                C4 * coefficience.coefficiencesArray[0].z - C5 * coefficience.coefficiencesArray[6].x);
            shArray[3] = new Vector4(2 * C1 * coefficience.coefficiencesArray[4].x,
                2 * C1 * coefficience.coefficiencesArray[5].x,
                C3 * coefficience.coefficiencesArray[6].x, 
                2 * C1 * coefficience.coefficiencesArray[7].x);
            shArray[4] = new Vector4(2 * C1 * coefficience.coefficiencesArray[4].y,
                2 * C1 * coefficience.coefficiencesArray[5].y,
                C3 * coefficience.coefficiencesArray[6].y, 
                2 * C1 * coefficience.coefficiencesArray[7].y);
            shArray[5] = new Vector4(2 * C1 * coefficience.coefficiencesArray[4].z,
                2 * C1 * coefficience.coefficiencesArray[5].z,
                C3 * coefficience.coefficiencesArray[6].z, 
                2 * C1 * coefficience.coefficiencesArray[7].z);
            shArray[6] = new Vector4(C1 * coefficience.coefficiencesArray[8].x,
                C1 * coefficience.coefficiencesArray[8].y,
                C1 * coefficience.coefficiencesArray[8].z, 
                1);
            Shader.SetGlobalVectorArray(ShaderConstants.AHD2_SHArray, shArray);
        }

        private void OnDisable()
        {
            instances.Remove(this);
            if (customCameraInstance != null)
                Destroy(customCameraInstance.gameObject);
            // 检查异步操作是否仍在运行
            if (isCalculating)
            {
                // 中止异步操作
                AsyncGPUReadback.WaitAllRequests(); // 等待所有异步操作完成
                isCalculating = false;
            }
            if(pack3Buffer!=null)
                pack3Buffer.Release();
        }

        private void CreateCubemap()
        {
            if (cubemap != null)
                return;

            int resolution = GetComponent<ReflectionProbe>().resolution;

            cubemap = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            cubemap.dimension = TextureDimension.Cube;
            cubemap.useMipMap = true;
            cubemap.Create();

            GetComponent<ReflectionProbe>().customBakedTexture = cubemap;
        }
        
        void OnDestroy()
        {
            // 检查异步操作是否仍在运行
            if (isCalculating)
            {
                // 中止异步操作
                AsyncGPUReadback.WaitAllRequests(); // 等待所有异步操作完成
                isCalculating = false;
            }
            if(pack3Buffer!=null)
                pack3Buffer.Release();
        }

        static class ShaderConstants
        {
            //SH系数
            public static readonly int AHD2_SHArray = Shader.PropertyToID("AHD2_SHArray");

            // Compute Shader 变量
            public static readonly int DiffuseTexture = Shader.PropertyToID("_Diffuse");
            public static readonly int NormalTexture = Shader.PropertyToID("_Normal");
            public static readonly int SkyboxTexture = Shader.PropertyToID("SkyboxTex");
            public static readonly int FaceIndex = Shader.PropertyToID("FaceIndex");
            public static readonly int Resolution = Shader.PropertyToID("Resolution");
            public static readonly int CubeMapTexture = Shader.PropertyToID("_CubeMapTexture");
            public static readonly int GroupCoefficients = Shader.PropertyToID("_GroupCoefficients");
            public static readonly int DispatchCount = Shader.PropertyToID("_DispatchCount");
            public static readonly int TextureSize = Shader.PropertyToID("_TextureSize");
            public static readonly int SHPackIndex = Shader.PropertyToID("_SHPackIndex");
            
            //反射贴图（虽然从反射探针也能获取，但是要有一个全局主探针来设置，让某些拿不到specmap0的物体也能拿信息。）（好吧，好像还是不用了，毛发似乎用diffuse就好了）
            public static readonly int AHD2_SpecCube0 = Shader.PropertyToID("_AHD2_SpecCube0");
        }
    }
}