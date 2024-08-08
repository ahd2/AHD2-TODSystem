using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
public partial class TimeOfDaysGenerator : EditorWindow
{
    private TODGlobalParameters _todGlobalParameters;
    private List<TempTOD> todList = new List<TempTOD>();
    public List<TimeOfDay> timeOfDays = new List<TimeOfDay>();//赋值nextTOD的列表(列表一定要初始化)
    
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
        //做一个判空，检测用户关闭弹窗
        if (currentPath == "")
        {
            GUIUtility.ExitGUI();//提前结束绘制，不加这个报错不匹配
            return;
        }
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
            int nextID = (i + 1 == timeOfDays.Count) ? 0 : i + 1;//下一个时刻的索引
            timeOfDays[i].nextTOD = timeOfDays[nextID];
            timeOfDays[i].materials = new Material[_todGlobalParameters.materials.Length]; //初始化材质数组，大小为全局参数材质数组大小
        }
        //赋值材质
        string matPath;
        for (int i = 0; i < _todGlobalParameters.materials.Length; i++)//逐个材质
        {
            //每个mat，创建关键帧个数的实例
            CreateDirectory(currentPath + "/" + _todGlobalParameters.materials[i].name + "_TOD");//尝试创建路径
            for (int j = 0; j < todList.Count; j++)//每个材质逐个TOD赋值
            {
                Debug.Log(j);
                matPath = currentPath + "/" +_todGlobalParameters.materials[i].name + "_TOD" + "/" + todList[j].name + "_" +_todGlobalParameters.materials[i].name + ".mat";//命名规则为时刻+材质名，如Noon_Cloud
                // 创建一个新的材质，复制原始材质的属性
                Material newMaterial = new Material(_todGlobalParameters.materials[i]);
                timeOfDays[j].materials[i] = newMaterial;//后续要改索引，
                // 保存新的材质到Assets文件夹
                AssetDatabase.CreateAsset(newMaterial, matPath);
                AssetDatabase.SaveAssets();
            }
        }
        
        message = "生成成功！";
        messageType = MessageType.Info;
        //清空timeOfDays,todList不清空是因为它也不会在代码中生成
        timeOfDays.Clear();
    }
    
    public void CreateDirectory(string path)
    {
        // 检查路径是否已存在
        if (!Directory.Exists(path))
        {
            // 如果路径不存在，创建它
            Directory.CreateDirectory(path);
        }
    }
}
