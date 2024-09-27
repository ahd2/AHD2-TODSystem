using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AHD2TimeOfDay
{
    public class NewTimeOfDaysController : MonoBehaviour
    {
        [MenuItem("GameObject/AHD2TODSystem/CreateTimeOfDaysController", false, 2000)]
        public static void CreateNewTimeOfDaysController(MenuCommand menuCommand)
        {
            // 创建一个新的游戏对象
            GameObject timeOfDaysController = new GameObject("TimeOfDaysController");

            // 确保撤销操作能够撤销这个创建动作
            Undo.RegisterCreatedObjectUndo(timeOfDaysController, "Create TimeOfDaysController");

            // 将新创建的游戏对象设置为当前选中的对象的子对象
            GameObjectUtility.SetParentAndAlign(timeOfDaysController, menuCommand.context as GameObject);

            // 在新创建的游戏对象上添加指定的脚本组件
            TODController scriptComponent = timeOfDaysController.AddComponent<TODController>(); 
            //初始化
            scriptComponent.todGlobalParameters = AssetDatabase.LoadAssetAtPath<TODGlobalParameters>("Packages/com.ahd2.tod-system/TODSystem/TODGlobalParameters.asset");
            Debug.LogWarning("请放入主方向光！！Please add the main directional light!");
            // 选中新创建的游戏对象
            Selection.activeObject = timeOfDaysController;
        }
    }
}
