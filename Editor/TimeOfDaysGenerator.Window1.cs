using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AHD2TimeOfDay
{
    public partial class TimeOfDaysGenerator : EditorWindow
    {
        private string currentPath;

        private void DrawWindow1()
        {
            GUILayout.BeginHorizontal();
            _todGlobalParameters = (TODGlobalParameters)EditorGUILayout.ObjectField(
                _todGlobalParameters, // 当前选中的对象。
                typeof(TODGlobalParameters), // 允许选择的对象类型。
                false
            );
            CheckGlobalParameters();
            GUILayout.FlexibleSpace();
            // 定义一个字符串数组作为下拉框的选项
            string[] options = new string[] { "按材质分文件夹", "按关键帧分文件夹" };
            _selectedIndex = EditorGUILayout.Popup(_selectedIndex, options);
            GUILayout.EndHorizontal();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical(GUI.skin.box);

            for (int i = 0; i < todList.Count; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                todList[i].name = EditorGUILayout.TextField("Name", todList[i].name);
                todList[i].Time = EditorGUILayout.FloatField("Time", todList[i].Time);
                if (GUILayout.Button("Remove"))
                {
                    todList.RemoveAt(i);
                    GUIUtility.ExitGUI(); //提前结束绘制，不加这个报错不匹配
                    return; // 避免在遍历过程中修改列表
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("添加TOD关键帧", GUILayout.Height(50)))
            {
                todList.Add(new TempTOD());
            }

            if (GUILayout.Button("生成TOD文件", GUILayout.Height(50)))
            {
                CheckName();
                CheckTime();
                GenerateTOD();
            }

            //消息盒子
            if (!string.IsNullOrEmpty(message))
            {
                EditorGUILayout.HelpBox(message, messageType);
            }
        }

        private void GenerateTOD()
        {
            //打开文件选择
            // 弹出文件选择窗口  
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID); //打开当前Project窗口打开的文件夹
            if (string.IsNullOrEmpty(assetPath))
            {
                assetPath = "Assets";
            }

            string soPath;
            currentPath = EditorUtility.OpenFolderPanel("选择路径", assetPath, "");
            //做一个判空，检测用户关闭弹窗
            if (currentPath == "")
            {
                GUIUtility.ExitGUI(); //提前结束绘制，不加这个报错不匹配
                return;
            }

            currentPath = currentPath.Replace(Application.dataPath, "Assets");
            for (int i = 0; i < todList.Count; i++)
            {
                int nextID = (i + 1 == todList.Count) ? 0 : i + 1; //下一个时刻的索引
                soPath = currentPath + "/" + todList[i].name + "2" + todList[nextID].name + ".asset";
                TimeOfDay newTOD = ScriptableObject.CreateInstance<TimeOfDay>();
                newTOD.startTime = todList[i].Time;
                newTOD.endTime = todList[nextID].Time;
                timeOfDays.Add(newTOD);
                AssetDatabase.CreateAsset(newTOD, soPath);
                AssetDatabase.SaveAssets();
            }

            _todGlobalParameters.timeOfDays = new TimeOfDay[timeOfDays.Count]; //重新初始化tod数组
            //赋值nextTOD
            for (int i = 0; i < timeOfDays.Count; i++)
            {
                int nextID = (i + 1 == timeOfDays.Count) ? 0 : i + 1; //下一个时刻的索引
                timeOfDays[i].nextTOD = timeOfDays[nextID];
                timeOfDays[i].materials = new Material[_todGlobalParameters.materials.Length]; //初始化材质数组，大小为全局参数材质数组大小
                _todGlobalParameters.timeOfDays[i] = timeOfDays[i]; //把tod赋值到全局参数中。
            }

            //赋值材质
            string matPath;
            if (_selectedIndex == 0) //如果按材质划分文件夹
            {
                for (int i = 0; i < _todGlobalParameters.materials.Length; i++) //逐个材质
                {
                    //每个mat，创建关键帧个数的实例
                    TimeOfDaysEditorUtility.CreateDirectory(currentPath + "/" + _todGlobalParameters.materials[i].name + "_TOD"); //尝试创建路径
                    for (int j = 0; j < todList.Count; j++) //每个材质逐个TOD赋值
                    {
                        //Debug.Log(j);
                        matPath = currentPath + "/" + _todGlobalParameters.materials[i].name + "_TOD" + "/" +
                                  todList[j].name + "_" + _todGlobalParameters.materials[i].name +
                                  ".mat"; //命名规则为时刻+材质名，如Noon_Cloud
                        // 创建一个新的材质，复制原始材质的属性
                        Material newMaterial = new Material(_todGlobalParameters.materials[i]);
                        timeOfDays[j].materials[i] = newMaterial; //把材质赋予TOD
                        EditorUtility.SetDirty(timeOfDays[j]); //要用这个标记so为已修改。unity才能把改动保存
                        // 保存新的材质到Assets文件夹
                        AssetDatabase.CreateAsset(newMaterial, matPath);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
            else //按关键帧划分文件夹，一个文件夹存一个关键帧下所有材质
            {
                for (int i = 0; i < todList.Count; i++) //逐个关键帧
                {
                    //每个关键帧创建一个文件夹
                    TimeOfDaysEditorUtility.CreateDirectory(currentPath + "/" + todList[i].name); //尝试创建路径
                    for (int j = 0; j < _todGlobalParameters.materials.Length; j++)
                    {
                        matPath = currentPath + "/" + todList[i].name + "/" + todList[i].name + "_" +
                                  _todGlobalParameters.materials[j].name + ".mat"; //命名规则为时刻+材质名，如Noon_Cloud
                        // 创建一个新的材质，复制原始材质的属性
                        Material newMaterial = new Material(_todGlobalParameters.materials[j]);
                        timeOfDays[i].materials[j] = newMaterial; //把材质赋予TOD
                        // 保存新的材质到Assets文件夹
                        AssetDatabase.CreateAsset(newMaterial, matPath);
                        AssetDatabase.SaveAssets();
                    }

                    EditorUtility.SetDirty(timeOfDays[i]); //要用这个标记so为已修改。unity才能把改动保存
                }
            }

            message = "生成成功！";
            messageType = MessageType.Info;
            //清空timeOfDays,todList不清空是因为它也不会在代码中生成
            timeOfDays.Clear();
        }
    }
}