using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
public partial class TimeOfDaysGenerator : EditorWindow
{
    private TODGlobalParameters _todGlobalParameters;
    private List<TempTOD> todList = new List<TempTOD>();
    public List<TimeOfDay> timeOfDays;//赋值nextTOD的列表
    
    //消息提示框提示的消息
    string message = "";
    MessageType messageType = MessageType.None;
    
    Vector2 scrollPos;//滚动条
    
    [MenuItem("Tools/TOD生成工具")]
    public static void ShowWindow()
    {
        GetWindow<TimeOfDaysGenerator>("ImageReconstructor");
    }
    
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        _todGlobalParameters = (TODGlobalParameters)EditorGUILayout.ObjectField(
            _todGlobalParameters,    // 当前选中的对象。
            typeof(TODGlobalParameters), // 允许选择的对象类型。
            false
        );
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
                GUIUtility.ExitGUI();//提前结束绘制，不加这个报错不匹配
                return; // 避免在遍历过程中修改列表
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("添加TOD关键帧",GUILayout.Height(50)))
        {
            todList.Add(new TempTOD());
        }
        if (GUILayout.Button("生成TOD文件",GUILayout.Height(50)))
        {
            CheckName();
            CheckTime();
            GenerateTOD();
            message = "生成成功！";
            messageType = MessageType.Info;
        }
        //消息盒子
        if (!string.IsNullOrEmpty(message))
        {
            EditorGUILayout.HelpBox(message, messageType);
        }
    }

    private string currentPath;

    private void GenerateTOD()
    {
        //打开文件选择
        // 弹出文件选择窗口  
        string assetPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);//打开当前Project窗口打开的文件夹
        if (string.IsNullOrEmpty(assetPath)) 
        {
            assetPath = "Assets";
        }

        string soPath;
        currentPath = EditorUtility.OpenFolderPanel("选择路径", assetPath, "");
        currentPath = currentPath.Replace(Application.dataPath, "Assets");
        for (int i = 0; i < todList.Count; i++)
        {
            int nextID = (i + 1 == todList.Count) ? 0 : i + 1;//下一个时刻的索引
            soPath = currentPath + "/" + todList[i].name + "2" + todList[nextID].name + ".asset";
            TimeOfDay newTOD = ScriptableObject.CreateInstance<TimeOfDay>();
            newTOD.startTime = todList[i].Time;
            newTOD.endTime = todList[nextID].Time;
            timeOfDays.Add(newTOD);
            AssetDatabase.CreateAsset(newTOD, soPath);
            AssetDatabase.SaveAssets();
        }
        //赋值nextTOD
        for (int i = 0; i < timeOfDays.Count; i++)
        {
            int nextID = (i + 1 == todList.Count) ? 0 : i + 1;//下一个时刻的索引
            timeOfDays[i].nextTOD = timeOfDays[nextID];
        }

        foreach (var mat in _todGlobalParameters.materials)
        {
            //每个mat，创建关键帧个数的实例
            
        }
    }
}
