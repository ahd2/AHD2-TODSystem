using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AHD2TimeOfDay
{
    /// <summary>
    /// 烘焙环境反射贴图，暂时只考虑生成
    /// </summary>
    public partial class TimeOfDaysGenerator : EditorWindow
    {
        private void DrawWindow3()
        {
            GUILayout.BeginHorizontal();
            _todGlobalParameters = (TODGlobalParameters)EditorGUILayout.ObjectField(
                _todGlobalParameters, // 当前选中的对象。
                typeof(TODGlobalParameters), // 允许选择的对象类型。
                false
            );
            CheckGlobalParameters();
            GUILayout.EndHorizontal();
        }
    }
}
