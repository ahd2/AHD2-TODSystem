using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
public partial class TimeOfDaysGenerator : EditorWindow
{
    private List<Material> extraMats = new List<Material>();
    private void DrawWindow2()
    {
        GUILayout.BeginHorizontal();
        _todGlobalParameters = (TODGlobalParameters)EditorGUILayout.ObjectField(
            _todGlobalParameters,    // 当前选中的对象。
            typeof(TODGlobalParameters), // 允许选择的对象类型。
            false
        );
        CheckGlobalParameters();
        GUILayout.FlexibleSpace();
        // 定义一个字符串数组作为下拉框的选项
        string[] options = new string[] {"按材质分文件夹", "按关键帧分文件夹"};
        _selectedIndex = EditorGUILayout.Popup( _selectedIndex, options);
        GUILayout.EndHorizontal();
        //要新增的材质列表
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true));
        EditorGUILayout.BeginVertical(GUI.skin.box);
        
        for (int i = 0; i < extraMats.Count; i++)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            extraMats[i] = (Material)EditorGUILayout.ObjectField(
                extraMats[i],    // 当前选中的对象。
                typeof(Material), // 允许选择的对象类型。
                false
            );
            if (GUILayout.Button("Remove"))
            {
                extraMats.RemoveAt(i);
                GUIUtility.ExitGUI();//提前结束绘制，不加这个报错不匹配
                return; // 避免在遍历过程中修改列表
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        
        if (GUILayout.Button("添加材质",GUILayout.Height(50)))
        {
            extraMats.Add(null);
        }
        
        if (GUILayout.Button("生成材质文件",GUILayout.Height(50)))
        {
            CheckMat();
            CreateExtraMat();
            message = "生成成功";
            messageType = MessageType.Info;
        }
        //消息盒子
        if (!string.IsNullOrEmpty(message))
        {
            EditorGUILayout.HelpBox(message, messageType);
        }
    }
    /// <summary>
    /// 生成新增材质文件的函数
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void CreateExtraMat()
    {
        //打开文件选择
        // 弹出文件选择窗口  
        string assetPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);//打开当前Project窗口打开的文件夹
        if (string.IsNullOrEmpty(assetPath)) 
        {
            assetPath = "Assets";
        }

        //string soPath;
        currentPath = EditorUtility.OpenFolderPanel("选择路径", assetPath, "");
        //做一个判空，检测用户关闭弹窗
        if (currentPath == "")
        {
            GUIUtility.ExitGUI();//提前结束绘制，不加这个报错不匹配
            return;
        }
        currentPath = currentPath.Replace(Application.dataPath, "Assets");
        //赋值材质
        string matPath;
        if (_selectedIndex == 0)//如果按材质划分文件夹
        {
            for (int i = 0; i < extraMats.Count; i++)//逐个额外材质
            {
                //每个mat，创建关键帧个数的实例
                CreateDirectory(currentPath + "/" + extraMats[i].name + "_TOD");//尝试创建路径
                for (int j = 0; j < todList.Count; j++)//每个材质逐个TOD赋值
                {
                    //Debug.Log(j);
                    matPath = currentPath + "/" + extraMats[i].name + "_TOD" + "/" + todList[j].name + "_" + extraMats[i].name + ".mat";//命名规则为时刻+材质名，如Noon_Cloud
                    // 创建一个新的材质，复制原始材质的属性
                    Material newMaterial = new Material(extraMats[i]);
                    List<Material> tempMatList = new List<Material>(_todGlobalParameters.timeOfDays[j].materials);//把数组换为list
                    tempMatList.Add(newMaterial);//list新增额外材质
                    _todGlobalParameters.timeOfDays[j].materials = tempMatList.ToArray(); //list再转为数组
                    // 保存新的材质到Assets文件夹
                    AssetDatabase.CreateAsset(newMaterial, matPath);
                    AssetDatabase.SaveAssets();
                }
            }
        }
        else//按关键帧划分文件夹，一个文件夹存一个关键帧下所有材质
        {
            for (int i = 0; i < todList.Count; i++)//逐个关键帧
            {
                //每个关键帧创建一个文件夹
                CreateDirectory(currentPath + "/" + todList[i].name);//尝试创建路径
                for (int j = 0; j < extraMats.Count; j++)
                {
                    matPath = currentPath + "/" + todList[i].name+ "/" + todList[i].name + "_" + extraMats[j].name + ".mat";//命名规则为时刻+材质名，如Noon_Cloud
                    // 创建一个新的材质，复制原始材质的属性
                    Material newMaterial = new Material(extraMats[j]);
                    List<Material> tempMatList = new List<Material>(_todGlobalParameters.timeOfDays[i].materials);//把数组换为list
                    tempMatList.Add(newMaterial);//list新增额外材质
                    _todGlobalParameters.timeOfDays[i].materials = tempMatList.ToArray(); //list再转为数组
                    // 保存新的材质到Assets文件夹
                    AssetDatabase.CreateAsset(newMaterial, matPath);
                    AssetDatabase.SaveAssets();
                }
            }
        }
        
        //全局参数类的材质列表也增加材质
        List<Material> tempGlobalMatList = new List<Material>(_todGlobalParameters.materials);//把数组换为list
        tempGlobalMatList.AddRange(extraMats);//添加额外材质
        _todGlobalParameters.materials = tempGlobalMatList.ToArray(); //list再转为数组
    }
}
