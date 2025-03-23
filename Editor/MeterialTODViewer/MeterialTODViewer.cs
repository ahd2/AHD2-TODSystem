using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AHD2TimeOfDay
{
    public class MaterialTODViewer : EditorWindow
    {
        private TimeOfDay materialList;
        private Vector2 scrollPosition;
        private Material selectedMaterial;
        private MaterialEditor materialEditor;

        [MenuItem("Tools/Material List Viewer")]
        public static void ShowWindow()
        {
            GetWindow<MaterialTODViewer>("Material List Viewer");
        }

        void OnGUI()
        {
            GUILayout.Label("Material List Viewer", EditorStyles.boldLabel);

            // 拖拽赋值ScriptableObject
            materialList = (TimeOfDay)EditorGUILayout.ObjectField(
                "Material List Asset",
                materialList,
                typeof(TimeOfDay),
                false);

            if (materialList == null || materialList.materials == null)
            {
                EditorGUILayout.HelpBox("Please assign a Material List asset", MessageType.Info);
                return;
            }

            DrawMaterialList();
            DrawSelectedMaterialInspector();
        }

        void DrawMaterialList()
        {
            float previewSize = 100f;
            float margin = 5f;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(previewSize + 30));
            EditorGUILayout.BeginHorizontal();

            foreach (var material in materialList.materials)
            {
                if (material == null) continue;

                EditorGUILayout.BeginVertical(GUILayout.Width(previewSize + margin));

                // 绘制材质预览
                Texture2D preview = AssetPreview.GetAssetPreview(material);
                bool isSelected = selectedMaterial == material;

                if (preview != null)
                {
                    GUI.backgroundColor = isSelected ? Color.blue : Color.white;
                    if (GUILayout.Button(preview, GUILayout.Width(previewSize), GUILayout.Height(previewSize)))
                    {
                        SelectMaterial(material);
                    }

                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    GUILayout.Box("Loading...", GUILayout.Width(previewSize), GUILayout.Height(previewSize));
                }

                // 显示材质名称
                GUILayout.Label(material.name, EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        void SelectMaterial(Material material)
        {
            selectedMaterial = material;
            DestroyImmediate(materialEditor);
            materialEditor = Editor.CreateEditor(selectedMaterial) as MaterialEditor;
        }

        private Vector2 propertyScrollPos;
        void DrawSelectedMaterialInspector()
        {
            if (selectedMaterial == null || materialEditor == null)
            {
                EditorGUILayout.HelpBox("请在上方选择一个材质", MessageType.Info);
                return;
            }

            // 分割线
            EditorGUILayout.Space(5);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space(5);

            // 属性面板标题
            EditorGUILayout.LabelField($"编辑材质: {selectedMaterial.name}", EditorStyles.boldLabel);

            // 绘制材质属性
            materialEditor.DrawHeader();
            if (materialEditor.isVisible)
            {
                propertyScrollPos = EditorGUILayout.BeginScrollView(propertyScrollPos);
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