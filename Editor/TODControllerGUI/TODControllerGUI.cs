using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AHD2TimeOfDay
{
    [CustomEditor(typeof(TODController))]
    public class TODControllerGUI : Editor
    {
        private Editor _soEditor; // 用于存储目标 ScriptableObject 的 Editor 实例

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            TODController todController = (TODController)target;
            //只有时间不流动的时候，才绘制SO面板
            if (todController.todGlobalParameters != null)
            {
                if (!todController.isTimeFlow)
                {
                    todController.todGlobalParameters.isTimeFlow = false;
                    // 如果目标 SO 的 Editor 实例还未创建，则创建它
                    if (_soEditor == null)
                    {
                        _soEditor = CreateEditor(todController.todGlobalParameters);
                    }

                    // 绘制目标 SO 的 Inspector
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("全局参数面板", EditorStyles.boldLabel);
                    _soEditor.OnInspectorGUI();
                }
                else
                {
                    if (!todController.todGlobalParameters.isTimeFlow)
                    {
                        todController.todGlobalParameters.isTimeFlow = true;
                    }
                }
            }
        }
    }
}