using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace AHD2TimeOfDay
{
    public class NewTimeOfDaysController : MonoBehaviour
    {
        [MenuItem("GameObject/AHD2TODSystem/CreateTimeOfDaysController", false, 2000)]
        public static void CreateNewTimeOfDaysController(MenuCommand menuCommand)
        {
            // 自动添加fogRenderFeature并挂载ComputeShader
            AddFogRenderFeature();
            
            // 创建一个新的游戏对象
            GameObject timeOfDaysController = new GameObject("TimeOfDaysController");

            // 确保撤销操作能够撤销这个创建动作
            Undo.RegisterCreatedObjectUndo(timeOfDaysController, "Create TimeOfDaysController");

            // 将新创建的游戏对象设置为当前选中的对象的子对象
            GameObjectUtility.SetParentAndAlign(timeOfDaysController, menuCommand.context as GameObject);
            
            // 在新创建的游戏对象上添加指定的脚本组件
            TODController scriptComponent = timeOfDaysController.AddComponent<TODController>(); 
            //初始化
            TimeOfDaysEditorUtility.CreateDirectory("Assets/TODSystem");//尝试创建路径
            TODGlobalParameters defaultTODGlobalParameters =
                AssetDatabase.LoadAssetAtPath<TODGlobalParameters>("Assets/TODSystem/DefaultTODGlobalParameters.asset");//尝试拿到复制出来的全局参数SO
            if (!defaultTODGlobalParameters)//如果没有,拿不到，那么就新建一个
            {
                //复制所有文件
                TimeOfDaysEditorUtility.CopyDirectory("Packages/com.ahd2.tod-system/TODSystem", "Assets/TODSystem");
                //更改TOD的引用
                TimeOfDay DawnTOD = AssetDatabase.LoadAssetAtPath<TimeOfDay>("Assets/TODSystem/TOD/Dawn2AfterDawn.asset");
                TimeOfDay AfterDawnTOD = AssetDatabase.LoadAssetAtPath<TimeOfDay>("Assets/TODSystem/TOD/AfterDawn2Noon1.asset");
                TimeOfDay Noon1TOD = AssetDatabase.LoadAssetAtPath<TimeOfDay>("Assets/TODSystem/TOD/Noon12Noon2.asset");
                TimeOfDay Noon2TOD = AssetDatabase.LoadAssetAtPath<TimeOfDay>("Assets/TODSystem/TOD/Noon22PreDusk.asset");
                TimeOfDay PreDuskTOD = AssetDatabase.LoadAssetAtPath<TimeOfDay>("Assets/TODSystem/TOD/PreDusk2Dusk.asset");
                TimeOfDay DuskTOD = AssetDatabase.LoadAssetAtPath<TimeOfDay>("Assets/TODSystem/TOD/Dusk2AfterDusk.asset");
                TimeOfDay AfterDuskTOD = AssetDatabase.LoadAssetAtPath<TimeOfDay>("Assets/TODSystem/TOD/AfterDusk2Night1.asset");
                TimeOfDay Night1TOD = AssetDatabase.LoadAssetAtPath<TimeOfDay>("Assets/TODSystem/TOD/Night12Night2.asset");
                TimeOfDay Night2TOD = AssetDatabase.LoadAssetAtPath<TimeOfDay>("Assets/TODSystem/TOD/Night22PreDawn.asset");
                TimeOfDay PreDawnTOD = AssetDatabase.LoadAssetAtPath<TimeOfDay>("Assets/TODSystem/TOD/PreDawn2Dawn.asset");
                
                Material DawnSky = AssetDatabase.LoadAssetAtPath<Material>("Assets/TODSystem/Material/HDRSkyBox_BaseSky_TOD/Dawn_HDRSkyBox_BaseSky.mat");
                Material AfterDawnSky = AssetDatabase.LoadAssetAtPath<Material>("Assets/TODSystem/Material/HDRSkyBox_BaseSky_TOD/AfterDawn_HDRSkyBox_BaseSky.mat");
                Material Noon1Sky = AssetDatabase.LoadAssetAtPath<Material>("Assets/TODSystem/Material/HDRSkyBox_BaseSky_TOD/Noon1_HDRSkyBox_BaseSky.mat");
                Material Noon2Sky = AssetDatabase.LoadAssetAtPath<Material>("Assets/TODSystem/Material/HDRSkyBox_BaseSky_TOD/Noon2_HDRSkyBox_BaseSky.mat");
                Material PreDuskSky = AssetDatabase.LoadAssetAtPath<Material>("Assets/TODSystem/Material/HDRSkyBox_BaseSky_TOD/PreDusk_HDRSkyBox_BaseSky.mat");
                Material DuskSky = AssetDatabase.LoadAssetAtPath<Material>("Assets/TODSystem/Material/HDRSkyBox_BaseSky_TOD/Dusk_HDRSkyBox_BaseSky.mat");
                Material AfterDuskSky = AssetDatabase.LoadAssetAtPath<Material>("Assets/TODSystem/Material/HDRSkyBox_BaseSky_TOD/AfterDusk_HDRSkyBox_BaseSky.mat");
                Material Night1Sky = AssetDatabase.LoadAssetAtPath<Material>("Assets/TODSystem/Material/HDRSkyBox_BaseSky_TOD/Night1_HDRSkyBox_BaseSky.mat");
                Material Night2Sky = AssetDatabase.LoadAssetAtPath<Material>("Assets/TODSystem/Material/HDRSkyBox_BaseSky_TOD/Night2_HDRSkyBox_BaseSky.mat");
                Material PreDawnSky = AssetDatabase.LoadAssetAtPath<Material>("Assets/TODSystem/Material/HDRSkyBox_BaseSky_TOD/PreDawn_HDRSkyBox_BaseSky.mat");

                DawnTOD.materials[0] = DawnSky;
                DawnTOD.nextTOD = AfterDawnTOD;
                
                AfterDawnTOD.materials[0] = AfterDawnSky;
                AfterDawnTOD.nextTOD = Noon1TOD;
                
                Noon1TOD.materials[0] = Noon1Sky;
                Noon1TOD.nextTOD = Noon2TOD;
                
                Noon2TOD.materials[0] = Noon2Sky;
                Noon2TOD.nextTOD = PreDuskTOD;

                PreDuskTOD.materials[0] = PreDuskSky;
                PreDuskTOD.nextTOD = DuskTOD;

                DuskTOD.materials[0] = DuskSky;
                DuskTOD.nextTOD = AfterDuskTOD;

                AfterDuskTOD.materials[0] = AfterDuskSky;
                AfterDuskTOD.nextTOD = Night1TOD;

                Night1TOD.materials[0] = Night1Sky;
                Night1TOD.nextTOD = Night2TOD;

                Night2TOD.materials[0] = Night2Sky;
                Night2TOD.nextTOD = PreDawnTOD;

                PreDawnTOD.materials[0] = PreDawnSky;
                PreDawnTOD.nextTOD = DawnTOD;
                //更改全局参数的引用
                defaultTODGlobalParameters = AssetDatabase.LoadAssetAtPath<TODGlobalParameters>("Assets/TODSystem/TODGlobalParameters.asset");
                defaultTODGlobalParameters.materials[0] = AssetDatabase.LoadAssetAtPath<Material>("Assets/TODSystem/Material/HDRSkyBox_BaseSky.mat");

                defaultTODGlobalParameters.timeOfDays[0] = DawnTOD;
                defaultTODGlobalParameters.timeOfDays[1] = AfterDawnTOD;
                defaultTODGlobalParameters.timeOfDays[2] = Noon1TOD;
                defaultTODGlobalParameters.timeOfDays[3] = Noon2TOD;
                defaultTODGlobalParameters.timeOfDays[4] = PreDuskTOD;
                defaultTODGlobalParameters.timeOfDays[5] = DuskTOD;
                defaultTODGlobalParameters.timeOfDays[6] = AfterDuskTOD;
                defaultTODGlobalParameters.timeOfDays[7] = Night1TOD;
                defaultTODGlobalParameters.timeOfDays[8] = Night2TOD;
                defaultTODGlobalParameters.timeOfDays[9] = PreDawnTOD;
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            scriptComponent.todGlobalParameters = defaultTODGlobalParameters;
            
            //复制反射探针烘焙renderdata到Asset目录
            ScriptableRendererData bakedRenderData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>("Assets/TODSystem/ReflectionBakedRenderer.asset");//尝试拿到复制出来的全局参数SO
            if (!bakedRenderData)
            {
                bakedRenderData = Instantiate(AssetDatabase.LoadAssetAtPath<ScriptableRendererData>("Packages/com.ahd2.tod-system/Runtime/ReflectionProbe/ReflectionBakedRenderer.asset"));//复制出全局参数SO
                AssetDatabase.CreateAsset(bakedRenderData, "Assets/TODSystem/ReflectionBakedRenderer.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            //所有方向光
            Light[] allLights = FindObjectsOfType<Light>();
            List<Light> directionalLights = new List<Light>();
            foreach (Light light in allLights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLights.Add(light);
                }
            }
            Light[] lights = directionalLights.ToArray();//
            
            scriptComponent.MainLight = lights[0];
            if (!scriptComponent.MainLight)
            {
                Debug.LogWarning("请放入主方向光！！Please add the main directional light!");
            }
            
            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/TODSystem/Material/HDRSkyBox_BaseSky.mat");
            // 选中新创建的游戏对象
            Selection.activeObject = timeOfDaysController;
        }

        private static void AddFogRenderFeature()
        {
            // 获取当前渲染管线资产
            var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                Debug.LogError("未找到Universal Render Pipeline资产。请确保项目使用URP。");
                return;
            }
            
            // 通过反射获取当前渲染器数据列表
            Type urpAssetType = urpAsset.GetType();
            FieldInfo renderersField = urpAssetType.GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);
            if (renderersField == null)
            {
                Debug.LogError("无法获取m_RendererDataList字段");
                return;
            }
            
            // 获取渲染器数据数组
            var rendererDataArray = renderersField.GetValue(urpAsset) as ScriptableRendererData[];
            if (rendererDataArray == null || rendererDataArray.Length == 0)
            {
                Debug.LogError("未找到渲染器数据");
                return;
            }
            
            // 默认使用第一个渲染器（通常是前向渲染器）
            var rendererData = rendererDataArray[0] as UniversalRendererData;
            if (rendererData == null)
            {
                Debug.LogError("未找到UniversalRendererData");
                return;
            }
            
            // 检查是否已经存在VolumetricFogRendererFeature
            bool fogFeatureExists = false;
            foreach (var feature in rendererData.rendererFeatures)
            {
                // 假设你的雾效类型是VolumetricFogRendererFeature
                if (feature is VolumetricFogRendererFeature)
                {
                    fogFeatureExists = true;
                    Debug.Log("已经存在VolumetricFogRendererFeature");
                    break;
                }
            }
            
            // 创建新Feature实例
            var newFeature = ScriptableObject.CreateInstance<VolumetricFogRendererFeature>();
            newFeature.name = "VolumetricFogRendererFeature";
            // 查找并设置ComputeShader
            ComputeShader fogComputeShader =
                AssetDatabase.LoadAssetAtPath<ComputeShader>(
                    "Packages/com.ahd2.tod-system/Shaders/VolumetricFog/DensityAndLighting.compute");
            if (fogComputeShader == null)
            {
                Debug.LogWarning("未找到雾效ComputeShader，请手动设置");
            }
            else
            {
                // 通过反射设置ComputeShader
                FieldInfo settingsField = typeof(VolumetricFogRendererFeature).GetField("volumetricFogSettings",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (settingsField != null)
                {
                    var settings = settingsField.GetValue(newFeature);
                    if (settings != null)
                    {
                        // 假设settings有一个computeShader字段
                        FieldInfo computeShaderField = settings.GetType().GetField("densityAndLightingComputeShader",
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (computeShaderField != null)
                        {
                            computeShaderField.SetValue(settings, fogComputeShader);
                            Debug.Log("已设置雾效ComputeShader");
                        }
                    }
                }
            }

            // 添加到Renderer Data
            addRenderFeature(rendererData, newFeature);

            // 标记资源需要保存
            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();
        }
        
        //来自https://discussions.unity.com/t/urp-adding-a-renderfeature-from-script/842637
        /// <summary>
        /// Based on Unity add feature code.
        /// See: AddComponent() in https://github.com/Unity-Technologies/Graphics/blob/d0473769091ff202422ad13b7b764c7b6a7ef0be/com.unity.render-pipelines.universal/Editor/ScriptableRendererDataEditor.cs#180
        /// </summary>
        /// <param name="data"></param>
        /// <param name="feature"></param>
        static void addRenderFeature(ScriptableRendererData data, ScriptableRendererFeature feature)
        {
            // Let's mirror what Unity does.
            var serializedObject = new SerializedObject(data);

            var renderFeaturesProp = serializedObject.FindProperty("m_RendererFeatures"); // Let's hope they don't change these.
            var renderFeaturesMapProp = serializedObject.FindProperty("m_RendererFeatureMap");

            serializedObject.Update();

            // Store this new effect as a sub-asset so we can reference it safely afterwards.
            // Only when we're not dealing with an instantiated asset
            if (EditorUtility.IsPersistent(data))
                AssetDatabase.AddObjectToAsset(feature, data);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out var guid, out long localId);

            // Grow the list first, then add - that's how serialized lists work in Unity
            renderFeaturesProp.arraySize++;
            var componentProp = renderFeaturesProp.GetArrayElementAtIndex(renderFeaturesProp.arraySize - 1);
            componentProp.objectReferenceValue = feature;

            // Update GUID Map
            renderFeaturesMapProp.arraySize++;
            var guidProp = renderFeaturesMapProp.GetArrayElementAtIndex(renderFeaturesMapProp.arraySize - 1);
            guidProp.longValue = localId;

            // Force save / refresh
            if (EditorUtility.IsPersistent(data))
            {
                AssetDatabase.SaveAssetIfDirty(data);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
