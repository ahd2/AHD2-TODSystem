using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AHD2TimeOfDay
{
    /// <summary>
    /// 删除材质
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
            
            //要删除的材质列表
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical(GUI.skin.box);

            for (int i = 0; i < _todGlobalParameters.materials.Length; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                _todGlobalParameters.materials[i] = (Material)EditorGUILayout.ObjectField(
                    _todGlobalParameters.materials[i], // 当前选中的对象。
                    typeof(Material), // 允许选择的对象类型。
                    false
                );
                if (GUILayout.Button("Remove"))
                {
                    DeleteMat(i);
                    GUIUtility.ExitGUI(); //提前结束绘制，不加这个报错不匹配
                    return; // 避免在遍历过程中修改列表
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
        
        private void DeleteMat(int index)
        {
            string path;
            List<Material> tempMatList;
            //根据索引删除材质，所以一定要保证tod和全局参数索引相同
            foreach (var timeOfDay in _todGlobalParameters.timeOfDays)
            {
                tempMatList = new List<Material>(timeOfDay.materials);//数组转换为list
                path = AssetDatabase.GetAssetPath(timeOfDay.materials[index]);//拿到这个tod的这个材质的路径
                tempMatList.RemoveAt(index);//list删除索引处材质
                timeOfDay.materials = tempMatList.ToArray();//list再转为数组，这时候tod上材质就已经移除了。
                AssetDatabase.DeleteAsset(path);//删除材质文件
            }
            //最后从全局参数本身移除这个材质（不删文件
            tempMatList = new List<Material>(_todGlobalParameters.materials);
            tempMatList.RemoveAt(index);
            _todGlobalParameters.materials = tempMatList.ToArray();
        }
    }
}
