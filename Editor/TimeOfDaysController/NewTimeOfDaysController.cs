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
                defaultTODGlobalParameters = Instantiate(AssetDatabase.LoadAssetAtPath<TODGlobalParameters>("Packages/com.ahd2.tod-system/TODSystem/TODGlobalParameters.asset"));//复制出全局参数SO
                AssetDatabase.CreateAsset(defaultTODGlobalParameters, "Assets/TODSystem/DefaultTODGlobalParameters.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            scriptComponent.todGlobalParameters = defaultTODGlobalParameters;
            
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
            
            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.ahd2.tod-system/TODSystem/Material/HDRSkyBox_BaseSky.mat");
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
            FieldInfo renderersField = urpAssetType.GetField("m_Renderers", BindingFlags.NonPublic | BindingFlags.Instance);
            if (renderersField == null)
            {
                Debug.LogError("无法获取m_Renderers字段");
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
            // 查找并设置ComputeShader
            ComputeShader fogComputeShader =
                AssetDatabase.LoadAssetAtPath<ComputeShader>(
                    "Packages/com.ahd2.tod-system/TODSystem/Shaders/VolumetricFog/DensityAndLighting.compute");
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
            rendererData.rendererFeatures.Add(newFeature);

            // 标记资源需要保存
            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();
        }
    }
}
