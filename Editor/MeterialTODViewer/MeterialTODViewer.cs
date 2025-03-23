using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AHD2TimeOfDay
{
    public class MaterialTODViewer : EditorWindow
    {
        private TODGlobalParameters GlobalParameters;
        private Vector2 scrollPosition;
        private Material selectedMaterial;
        private MaterialEditor materialEditor;

        [MenuItem("Tools/Material List Viewer")]
        public static void ShowWindow()
        {
            GetWindow<MaterialTODViewer>("Material List Viewer");
        }
        //选择显示类型
        private int _selectedIndex = 0;

        void OnGUI()
        {
            GUILayout.Label("Material List Viewer", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            // 拖拽赋值ScriptableObject
            GlobalParameters = (TODGlobalParameters)EditorGUILayout.ObjectField(
                "Material List Asset",
                GlobalParameters,
                typeof(TODGlobalParameters),
                false);
            // 定义一个字符串数组作为下拉框的选项
            string[] options = new string[] { "显示单个关键帧所有材质", "显示单个材质所有关键帧" };
            _selectedIndex = EditorGUILayout.Popup(_selectedIndex, options);
            if (GlobalParameters && _selectedIndex == 0)
            {
                // 绘制时间点选择下拉框
                DrawTimeOfDaySelector();
            }
            if (GlobalParameters && _selectedIndex == 1)
            {
                // 绘制材质选择下拉框
                DrawMaterialSelector();
            }
            GUILayout.EndHorizontal();

            if (GlobalParameters == null || GlobalParameters.materials == null)
            {
                EditorGUILayout.HelpBox("拖入全局参数SO", MessageType.Info);
                return;
            }

            //先是一个横向的框，显示一个关键帧的所有材质
            //然后纵向
            DrawMaterialList();
        }
        private int selectedTimeOfDayIndex = 0; // 新增字段保存选中索引
        void DrawTimeOfDaySelector()
        {
            if (GlobalParameters.timeOfDays == null || GlobalParameters.timeOfDays.Length == 0)
            {
                EditorGUILayout.HelpBox("没有配置TOD", MessageType.Warning);
                return;
            }

            // 生成选项名称数组
            string[] options = new string[GlobalParameters.timeOfDays.Length];
            for (int i = 0; i < GlobalParameters.timeOfDays.Length; i++)
            {
                //(需要todFrameList和todlist对齐)
                options[i] = GlobalParameters.todFrameList[i].name;
            }
    
            // 绘制下拉框
            selectedTimeOfDayIndex = EditorGUILayout.Popup(
                "选择TOD",
                selectedTimeOfDayIndex,
                options);
        }
        private int selectedMatIndex = 0; // 新增字段保存选中索引
        void DrawMaterialSelector()
        {
            if (GlobalParameters.materials == null || GlobalParameters.materials.Length == 0)
            {
                EditorGUILayout.HelpBox("没有配置材质", MessageType.Warning);
                return;
            }

            // 生成选项名称数组
            string[] options = new string[GlobalParameters.materials.Length];
            for (int i = 0; i < GlobalParameters.materials.Length; i++)
            {
                //(需要todFrameList和todlist对齐)
                options[i] = GlobalParameters.materials[i].name;
            }
    
            // 绘制下拉框
            selectedMatIndex = EditorGUILayout.Popup(
                "选择材质",
                selectedMatIndex,
                options);
        }
        void DrawMaterialList()
        {
            float previewSize = 100f;
            float margin = 5f;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginHorizontal();

            if (_selectedIndex == 0)//显示单个关键帧所有材质
            {
                //每一个材质，先开一个横向（为了和左右分割），再开一个纵向框，装该材质所有TOD
                //要求全局参数类和tod的材质索引是一样的。
                for (int i = 0; i < GlobalParameters.materials.Length; i++)
                {
                    // 单个材质面板容器
                    EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true));
                    EditorGUILayout.BeginVertical();

                    var tod =GlobalParameters.timeOfDays[selectedTimeOfDayIndex];
                    SelectMaterial(tod.materials[i]);
                    DrawSelectedMaterialInspector(tod.materials[i]);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }else if (_selectedIndex == 1)//显示单个材质所有关键帧
            {
                for (int i = 0; i < GlobalParameters.timeOfDays.Length; i++)
                {
                    // 单个材质面板容器
                    EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true));
                    EditorGUILayout.BeginVertical();

                    var mat =GlobalParameters.timeOfDays[i].materials[selectedMatIndex];
                    SelectMaterial(mat);
                    DrawSelectedMaterialInspector(mat);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        void SelectMaterial(Material material)
        {
            selectedMaterial = material;
            DestroyImmediate(materialEditor);
            materialEditor = Editor.CreateEditor(selectedMaterial) as MaterialEditor;
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(materialEditor.target, true);//让其默认展开
        }

        // 在类中添加字典存储滚动位置
        private Dictionary<Material, Vector2> materialScrollPositions = new Dictionary<Material, Vector2>();
        void DrawSelectedMaterialInspector(Material mat)
        {
            // 获取或创建滚动位置
            if (!materialScrollPositions.ContainsKey(mat))
            {
                materialScrollPositions[mat] = Vector2.zero;
            }
            // 分割线
            EditorGUILayout.Space(5);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space(5);

            // 属性面板标题
            EditorGUILayout.LabelField($"材质: {selectedMaterial.name}", EditorStyles.boldLabel);

            // 绘制材质属性
            materialEditor.DrawHeader();
            if (materialEditor.isVisible)
            {
                materialScrollPositions[mat] = EditorGUILayout.BeginScrollView(materialScrollPositions[mat]);
                EditorGUILayout.BeginVertical();
                materialEditor.PropertiesGUI();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
            }

            // 注册撤销操作
            if (Event.current.commandName == "UndoRedoPerformed")
            {
                materialEditor.RegisterPropertyChangeUndo("Material Change");
            }
        }
    }
}