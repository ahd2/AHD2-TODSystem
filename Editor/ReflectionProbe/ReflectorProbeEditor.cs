using UnityEditor;
using UnityEngine;

namespace AHD2TimeOfDay
{
    [CustomEditor(typeof(ReflectorProbe)), CanEditMultipleObjects]
    public class ReflectorProbeEditor : Editor
    {
        private SerializedObject reflectionProbes;

        private SerializedProperty importance;
        private SerializedProperty size;
        private SerializedProperty offset;
        private SerializedProperty projection;
        private SerializedProperty intensity;
        private SerializedProperty blend;

        private SerializedProperty resolution;

        private SerializedProperty baked;
        private SerializedProperty bakedNormal;
        private SerializedProperty bakeable;

        private GUIContent[] resolutionNames = new GUIContent[] { new GUIContent("16"), new GUIContent("32"), new GUIContent("64"), new GUIContent("128"),
                                                                  new GUIContent("256"), new GUIContent("512"), new GUIContent("1024"), new GUIContent("2048") };
        private int[] resolutionValues = new int[] { 16, 32, 64, 128, 256, 512, 1024, 2048 };

        private void OnEnable()
        {
            ReflectionProbe[] probes = new ReflectionProbe[serializedObject.targetObjects.Length];
            for (int i = 0; i < serializedObject.targetObjects.Length; i++)
                probes[i] = ((ReflectorProbe)serializedObject.targetObjects[i]).GetComponent<ReflectionProbe>();

            reflectionProbes = new SerializedObject(probes);

            importance = reflectionProbes.FindProperty("m_Importance");
            size = reflectionProbes.FindProperty("m_BoxSize");
            offset = reflectionProbes.FindProperty("m_BoxOffset");
            projection = reflectionProbes.FindProperty("m_BoxProjection");
            intensity = reflectionProbes.FindProperty("m_IntensityMultiplier");
            blend = reflectionProbes.FindProperty("m_BlendDistance");

            resolution = reflectionProbes.FindProperty("m_Resolution");

            bakedNormal = serializedObject.FindProperty("bakedNormal");
            baked = serializedObject.FindProperty("baked");
            bakeable = serializedObject.FindProperty("bakeable");
        }

        public override void OnInspectorGUI()
        {
            reflectionProbes.Update();
            serializedObject.Update();

            EditorGUILayout.LabelField("Runtime");
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(importance, new GUIContent("Importance"));
            EditorGUILayout.PropertyField(intensity, new GUIContent("Intensity"));
            EditorGUILayout.PropertyField(projection, new GUIContent("Box Projection"));
            EditorGUILayout.PropertyField(blend, new GUIContent("Blend Distance"));
            EditorGUILayout.PropertyField(size, new GUIContent("Size"));
            EditorGUILayout.PropertyField(offset, new GUIContent("Offset"));

            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Render");
            EditorGUI.indentLevel++;

            EditorGUILayout.IntPopup(resolution, resolutionNames, resolutionValues);

            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Bake");
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(bakeable, new GUIContent("Bakeable"));
            //一个烘焙贴图展示部分
            EditorGUI.BeginChangeCheck();
            Object objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Diffuse Map"), baked.objectReferenceValue, typeof(Texture), false);
            if (EditorGUI.EndChangeCheck())
                baked.objectReferenceValue = objectReferenceValue;
            
            //一个烘焙贴图展示部分
            EditorGUI.BeginChangeCheck();
            Object objectReferenceValueNormal = EditorGUILayout.ObjectField(new GUIContent("Normal Map"), bakedNormal.objectReferenceValue, typeof(Texture), false);
            if (EditorGUI.EndChangeCheck())
                bakedNormal.objectReferenceValue = objectReferenceValueNormal;

            EditorGUI.indentLevel--;

            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Space(EditorGUIUtility.labelWidth);

            if (GUILayout.Button("Bake"))
            {
                ReflectorProbe[] probes = new ReflectorProbe[serializedObject.targetObjects.Length];
                for (int i = 0; i < probes.Length; i++)
                    probes[i] = serializedObject.targetObjects[i] as ReflectorProbe;

                ReflectorProbeBaker.Bake(probes);
            }

            if (GUILayout.Button("Bake All"))
            {
                ReflectorProbeBaker.BakeReflector();
            }

            if (GUILayout.Button("Clear"))
            {
                ReflectorProbe[] probes = new ReflectorProbe[serializedObject.targetObjects.Length];
                for (int i = 0; i < probes.Length; i++)
                    probes[i] = serializedObject.targetObjects[i] as ReflectorProbe;

                ReflectorProbeBaker.Clear(probes);
            }

            GUILayout.EndHorizontal();

            reflectionProbes.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnSceneGUI()
        {
        }
    }
}
