using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace AHD2TimeOfDay
{
    public class NewTimeOfDaysController : MonoBehaviour
    {
        [MenuItem("GameObject/AHD2TODSystem/CreateTimeOfDaysController", false, 2000)]
        public static void CreateNewTimeOfDaysController(MenuCommand menuCommand)
        {
            GlobalKeyword fogKeyword = GlobalKeyword.Create("VOLUMETRICFOG_ON");
            Shader.SetKeyword(fogKeyword, false);//
            
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
    }
}
