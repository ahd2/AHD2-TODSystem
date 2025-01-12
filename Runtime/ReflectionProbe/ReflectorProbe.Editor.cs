using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AHD2TimeOfDay
{
    /// <summary>
    /// 编辑器部分逻辑
    /// </summary>
    public partial class ReflectorProbe : MonoBehaviour
    {
        private static HashSet<ReflectorProbe> dynamics = new HashSet<ReflectorProbe>(); 

        private static Dictionary<int, RenderPair> targets = new Dictionary<int, RenderPair>();
#if UNITY_EDITOR
        public Camera Camera
        {
            get
            {
                if (externalCamera != null)
                    return externalCamera;

                if (customCameraInstance != null)
                    return customCameraInstance;

                if (customCamera == null)
                {
                    if (renderCamera == null)
                        renderCamera = new GameObject("ReflectionCamera").AddComponent<Camera>();

                    return renderCamera;
                }
                else
                {
                    customCameraInstance = Instantiate(customCamera.gameObject).GetComponent<Camera>();
                    return customCameraInstance;
                }
            }
            set
            {
                externalCamera = value;
            }
        }
        private void OnDrawGizmos()
        {
            ReflectionProbe probe = GetComponent<ReflectionProbe>();
            Gizmos.color = new Color(1, 0.4f, 0, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;

            Vector3 center = probe.center;

            Gizmos.DrawWireCube(center, probe.size);
            Gizmos.matrix = Matrix4x4.identity;
        }

        private void OnDrawGizmosSelected()
        {
            ReflectionProbe probe = GetComponent<ReflectionProbe>();
            Gizmos.color = new Color(1, 0.4f, 0, 0.1f);
            Gizmos.matrix = transform.localToWorldMatrix;

            Vector3 center = probe.center;

            Gizmos.DrawCube(center, probe.size);
            Gizmos.matrix = Matrix4x4.identity;
        }

        public Texture2DArray BakeReflection()
        {
            if (Application.isPlaying || !bakeable)
                return null;

            CreateData();

            int resolution = GetComponent<ReflectionProbe>().resolution;

            RenderPair pair;
            if (!targets.TryGetValue(resolution, out pair))
                return null;

            Texture2DArray bakedDiffuseArray = new Texture2DArray(resolution, resolution,6, TextureFormat.RGBA32, false);
            bakedDiffuseArray.anisoLevel = 0;
            
            Camera camera = Camera;
            camera.gameObject.SetActive(true);
            camera.transform.position = transform.position;

            SetCameraSettings(camera);
            for (int face = 0; face < 6; face++)
            {
                camera.transform.rotation = orientations[face];

                Shader.EnableKeyword("NO_REFLECTION");
                int bakeRendererIndex = -1;
                BindingFlags bindings = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                ScriptableRendererData[] m_rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", bindings).GetValue(UniversalRenderPipeline.asset);
                for (int i = 0; i < m_rendererDataList.Length; i++)
                {
                    if (m_rendererDataList[i].name == "ReflectionBakedRenderer")
                    {
                        bakeRendererIndex = i;
                        //Debug.Log("找到了烘焙renderer");
                        //通过反射获取renderfeature
                        var renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer(i);
                        var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", bindings);
                        List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;
                        foreach (var feature in features)
                        {
                            //先试试只用一个feature
                            if (feature.name == "Diffuse")
                            {
                                camera.targetTexture = pair.Render;
                                feature.SetActive(true);
                                camera.GetUniversalAdditionalCameraData().SetRenderer(bakeRendererIndex);
                                camera.clearFlags = CameraClearFlags.SolidColor;
                                camera.cameraType = CameraType.Game;
                                camera.Render();
                                feature.SetActive(false);
                            }
                            //先烘焙遮罩到mirror贴图上
                            if (feature.name == "Mask")
                            {
                                camera.targetTexture = pair.Mask;
                                feature.SetActive(true);
                                camera.Render();
                                feature.SetActive(false);
                            }
                        }
                    }
                    //如果没找到，说明没添加renderer，报错
                }
                
                Shader.DisableKeyword("NO_REFLECTION");
                //合并同时镜像
                mirror.SetTexture("_MaskTex", pair.Mask);
                Graphics.Blit(pair.Render, pair.Mirror, mirror);
                //投影到数组
                Texture2D reader = new Texture2D(resolution, resolution, TextureFormat.RGBA32, true, true);
                reader.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                bakedDiffuseArray.SetPixels(reader.GetPixels(), face);

                Clear(pair);
                RenderTexture.active = null;
            }

            bakedDiffuseArray.Apply();
            baked = bakedDiffuseArray;

            DestroyImmediate(camera.gameObject);

            return bakedDiffuseArray;
        }
        
        public Texture2DArray BakeNormalReflection()
        {
            if (Application.isPlaying || !bakeable)
                return null;

            CreateData();

            int resolution = GetComponent<ReflectionProbe>().resolution;

            RenderPair pair;
            if (!targets.TryGetValue(resolution, out pair))
                return null;

            Texture2DArray bakedNormalArray = new Texture2DArray(resolution, resolution,6, TextureFormat.RGBA32, false, true);
            bakedNormalArray.anisoLevel = 0;
            
            Camera camera = Camera;
            camera.gameObject.SetActive(true);
            camera.transform.position = transform.position;
            camera.targetTexture = pair.Render;

            SetCameraSettings(camera);
            for (int face = 0; face < 6; face++)
            {
                camera.transform.rotation = orientations[face];

                Shader.EnableKeyword("NO_REFLECTION");
                int bakeRendererIndex = -1;
                BindingFlags bindings = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                ScriptableRendererData[] m_rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", bindings).GetValue(UniversalRenderPipeline.asset);
                for (int i = 0; i < m_rendererDataList.Length; i++)
                {
                    if (m_rendererDataList[i].name == "ReflectionBakedRenderer")
                    {
                        bakeRendererIndex = i;
                        //Debug.Log("找到了烘焙renderer");
                        //通过反射获取renderfeature
                        var renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer(i);
                        var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", bindings);
                        List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;
                        foreach (var feature in features)
                        {
                            //先试试只用一个feature
                            if (feature.name == "Normal")
                            {
                                feature.SetActive(true);
                                camera.GetUniversalAdditionalCameraData().SetRenderer(bakeRendererIndex);
                                camera.clearFlags = CameraClearFlags.SolidColor;
                                camera.cameraType = CameraType.Game;
                                camera.Render();
                                feature.SetActive(false);
                            }
                        }
                    }
                    //如果没找到，说明没添加renderer，报错
                }
                
                Shader.DisableKeyword("NO_REFLECTION");
                //镜像投影到面上
                Graphics.Blit(pair.Render, pair.Mirror, mirror);
                //投影到Tex数组上
                Texture2D reader = new Texture2D(resolution, resolution, TextureFormat.RGBA32, true, true);
                reader.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                bakedNormalArray.SetPixels(reader.GetPixels(), face);
                Clear(pair);
            }

            bakedNormalArray.Apply();
            //GetComponent<ReflectionProbe>().customBakedTexture = cubemap; //赋值给探针，暂时不用
            bakedNormal = bakedNormalArray;
            
            DestroyImmediate(camera.gameObject);

            //ResetReflection();

            return bakedNormalArray;
        }

        public void ClearBaking()
        {
            baked = null;
            GetComponent<ReflectionProbe>().customBakedTexture = null;
        }

        private void Clear(RenderPair pair)
        {
            RenderTexture rt = RenderTexture.active;

            RenderTexture.active = pair.Render;
            GL.Clear(true, true, Color.red);

            RenderTexture.active = pair.Mirror;
            GL.Clear(true, true, Color.red);
            
            RenderTexture.active = pair.Mask;
            GL.Clear(true, true, Color.red);

            RenderTexture.active = rt;
        }

        private void CreateData()
        {
            if (Application.isPlaying)
            {
                if (dynamics.Contains(this))
                    return;

                dynamics.Add(this);
            }

            if (mirror == null)
                mirror = new Material(Shader.Find("Hidden/ReflectorProbe/Mirror"));

            int resolution = GetComponent<ReflectionProbe>().resolution;

            RenderPair pair;
            if (targets.TryGetValue(resolution, out pair))
            {
                pair.Reflections.Add(this);
            }
            else
            {
                RenderTexture render = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                render.useMipMap = false;
                render.Create();

                RenderTexture mirror = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                mirror.useMipMap = false;
                mirror.Create();
                
                RenderTexture mask = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                mask.useMipMap = false;
                mask.Create();

                pair = new RenderPair(render, mirror, mask);
                pair.Reflections.Add(this);
                targets.Add(resolution, pair);
            }
        }
        private void SetCameraSettings(Camera camera)
        {
            ReflectionProbe probe = GetComponent<ReflectionProbe>();

            camera.hideFlags = HideFlags.HideAndDontSave;
            camera.enabled = false;
            camera.gameObject.SetActive(true);
            camera.fieldOfView = 90;

            if (customCamera == null)
            {
                camera.farClipPlane = probe.farClipPlane;
                camera.nearClipPlane = probe.nearClipPlane;
                camera.cullingMask = probe.cullingMask;
                camera.clearFlags = (CameraClearFlags)probe.clearFlags;
                camera.backgroundColor = probe.backgroundColor;
                camera.allowHDR = probe.hdr;
            }
        }

        private struct RenderPair
        {
            private RenderTexture render;

            public RenderTexture Render
            {
                get { return render; }
            }

            private RenderTexture mirror;

            public RenderTexture Mirror
            {
                get { return mirror; }
            }
            
            private RenderTexture mask;

            public RenderTexture Mask
            {
                get { return mask; }
            }

            private HashSet<ReflectorProbe> reflections;

            public HashSet<ReflectorProbe> Reflections
            {
                get { return reflections; }
            }

            public RenderPair(RenderTexture render, RenderTexture mirror, RenderTexture mask)
            {
                this.render = render;
                this.mirror = mirror;
                this.mask = mask;
                reflections = new HashSet<ReflectorProbe>();
            }

            public void Release()
            {
                render.Release();
                mirror.Release();
                mask.Release();
            }
        }
#endif
    }
}