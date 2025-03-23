using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AHD2TimeOfDay
{
    public class MaterialTODViewer : EditorWindow
    {
        private TODGlobalParameters globalParameters;
        private Vector2 scrollPosition;
        private Material selectedMaterial;
        private MaterialEditor materialEditor;

        private int selectedDisplayMode;
        private int selectedTimeOfDayIndex;
        private int selectedMaterialIndex;
        private readonly Dictionary<Material, Vector2> materialScrollPositions = new Dictionary<Material, Vector2>();

        private const string DisplayModeSingleTOD = "显示单个关键帧所有材质";
        private const string DisplayModeSingleMaterial = "显示单个材质所有关键帧";
        private readonly string[] displayModes = { DisplayModeSingleTOD, DisplayModeSingleMaterial };

        [MenuItem("Tools/Material List Viewer")]
        public static void ShowWindow()
        {
            GetWindow<MaterialTODViewer>("Material List Viewer");
        }

        void OnGUI()
        {
            DrawToolbar();
            DrawContent();
        }

        void DrawToolbar()
        {
            GUILayout.Label("Material List Viewer", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                globalParameters = (TODGlobalParameters)EditorGUILayout.ObjectField(
                    "全局参数配置",
                    globalParameters,
                    typeof(TODGlobalParameters),
                    false);

                selectedDisplayMode = EditorGUILayout.Popup(selectedDisplayMode, displayModes);

                if (globalParameters)
                {
                    if (selectedDisplayMode == 0)
                    {
                        DrawTimeOfDaySelector();
                    }
                    else
                    {
                        DrawMaterialSelector();
                    }
                }
            }
        }

        void DrawContent()
        {
            if (!globalParameters || globalParameters.materials == null)
            {
                EditorGUILayout.HelpBox("请拖入全局参数配置文件", MessageType.Info);
                return;
            }

            using (var scrollScope = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.ExpandHeight(true)))
            {
                scrollPosition = scrollScope.scrollPosition;
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawMaterialList();
                }
            }
        }

        void DrawTimeOfDaySelector()
        {
            if (globalParameters.timeOfDays == null || globalParameters.timeOfDays.Length == 0)
            {
                EditorGUILayout.HelpBox("没有配置时间段", MessageType.Warning);
                return;
            }

            var options = new string[globalParameters.timeOfDays.Length];
            for (int i = 0; i < globalParameters.timeOfDays.Length; i++)
            {
                options[i] = globalParameters.todFrameList[i].name;
            }

            selectedTimeOfDayIndex = EditorGUILayout.Popup("选择时间段", selectedTimeOfDayIndex, options);
        }

        void DrawMaterialSelector()
        {
            if (globalParameters.materials == null || globalParameters.materials.Length == 0)
            {
                EditorGUILayout.HelpBox("没有配置材质", MessageType.Warning);
                return;
            }

            var options = new string[globalParameters.materials.Length];
            for (int i = 0; i < globalParameters.materials.Length; i++)
            {
                options[i] = globalParameters.materials[i].name;
            }

            selectedMaterialIndex = EditorGUILayout.Popup("选择材质", selectedMaterialIndex, options);
        }

        void DrawMaterialList()
        {
            if (selectedDisplayMode == 0)
            {
                DrawSingleTimeOfDayMaterials();
            }
            else
            {
                DrawSingleMaterialTimeFrames();
            }
        }

        void DrawSingleTimeOfDayMaterials()
        {
            var tod = globalParameters.timeOfDays[selectedTimeOfDayIndex];
            for (int i = 0; i < globalParameters.materials.Length; i++)
            {
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box, GUILayout.ExpandWidth(true)))
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        var material = tod.materials[i];
                        SelectMaterial(material);
                        DrawMaterialInspector(material);
                    }
                }
            }
        }

        void DrawSingleMaterialTimeFrames()
        {
            for (int i = 0; i < globalParameters.timeOfDays.Length; i++)
            {
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box, GUILayout.ExpandWidth(true)))
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        var material = globalParameters.timeOfDays[i].materials[selectedMaterialIndex];
                        SelectMaterial(material);
                        DrawMaterialInspector(material);
                    }
                }
            }
        }

        void SelectMaterial(Material material)
        {
            if (selectedMaterial == material) return;

            selectedMaterial = material;
            DestroyImmediate(materialEditor);
            materialEditor = Editor.CreateEditor(material) as MaterialEditor;
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(materialEditor.target, true);
        }

        void DrawMaterialInspector(Material material)
        {
            if (!materialScrollPositions.ContainsKey(material))
            {
                materialScrollPositions[material] = Vector2.zero;
            }

            EditorGUILayout.Space(5);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField($"材质: {material.name}", EditorStyles.boldLabel);

            materialEditor.DrawHeader();
            if (materialEditor.isVisible)
            {
                materialScrollPositions[material] = EditorGUILayout.BeginScrollView(materialScrollPositions[material]);
                using (new EditorGUILayout.VerticalScope())
                {
                    materialEditor.PropertiesGUI();
                }
                EditorGUILayout.EndScrollView();
            }

            if (Event.current.commandName == "UndoRedoPerformed")
            {
                materialEditor.RegisterPropertyChangeUndo("材质变更");
            }
        }

        void OnDestroy()
        {
            DestroyImmediate(materialEditor);
            materialScrollPositions.Clear();
        }
    }
}