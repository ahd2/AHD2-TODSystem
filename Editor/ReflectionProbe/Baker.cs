using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AHD2TimeOfDay
{
    /// <summary>
    /// Bake deferred reflection probe.
    /// </summary>
    public class ReflectorProbeBaker
    {
        [MenuItem("Tools/Bake All Reflector")]
        public static void BakeReflector()
        {
            Bake(UnityEngine.Object.FindObjectsOfType<ReflectorProbe>());
        }

        public static void Bake(ReflectorProbe[] probes)
        {
            try
            {
                string path;
                if (!CreateDirectory(out path))
                    return;

                int count = 0;
                for (int i = 0; i < probes.Length; i++)
                {
                    if (probes[i].Baked)
                        count++;
                }

                int current = 0;
                for (int i = 0; i < probes.Length; i++)
                {
                    ReflectorProbe probe = probes[i];

                    CreateCameraData(probe);

                    current++;
                    EditorUtility.DisplayProgressBar("Baking Deferred Probe", "Baking: " + probe.name, current / (float)probes.Length);
                    //基础色路径
                    Texture previous = probe.GetComponent<ReflectorProbe>().Baked;
                    string existing = AssetDatabase.GetAssetPath(previous);
                    //法线路径
                    Texture previousNormal = probe.GetComponent<ReflectorProbe>().BakedNormal;
                    string existingNormal = AssetDatabase.GetAssetPath(previousNormal);
                    
                    Texture2DArray bakedDiffuse = probe.BakeReflection();
                    if (bakedDiffuse == null)
                        continue;

                    if (string.IsNullOrEmpty(existing))
                    {
                        string asset = "Assets" + path + '/' + probe.name + Guid.NewGuid().ToString() + "Diffuse.asset";
                        AssetDatabase.CreateAsset(bakedDiffuse, asset);
                    }
                    else
                    {
                        AssetDatabase.CreateAsset(bakedDiffuse, existing);
                    }
                    
                    Texture2DArray bakedNormal = probe.BakeNormalReflection();
                    if (bakedNormal == null)
                        continue;

                    if (string.IsNullOrEmpty(existingNormal))
                    {
                        string asset = "Assets" + path + '/' + probe.name + Guid.NewGuid().ToString() + "Normal.asset";
                        AssetDatabase.CreateAsset(bakedNormal, asset);
                    }
                    else
                    {
                        AssetDatabase.CreateAsset(bakedNormal, existingNormal);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorSceneManager.MarkAllScenesDirty();
        }

        public static void Clear(ReflectorProbe[] probes)
        {
            for (int i = 0; i < probes.Length; i++)
            {
                //基础色清理
                if (probes[i].Baked == null)
                    continue;

                string path = AssetDatabase.GetAssetPath(probes[i].Baked);
                if (string.IsNullOrEmpty(path))
                    continue;

                AssetDatabase.DeleteAsset(path);
                probes[i].ClearBaking();
                //法线清理
                if (probes[i].BakedNormal == null)
                    continue;

                string pathNormal = AssetDatabase.GetAssetPath(probes[i].BakedNormal);
                if (string.IsNullOrEmpty(pathNormal))
                    continue;

                AssetDatabase.DeleteAsset(pathNormal);
            }

            EditorSceneManager.MarkAllScenesDirty();
        }

        private static bool CreateDirectory(out string path)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(scene.path))
            {
                Debug.LogWarning("Tring to bake reflections from a scene not saved.");
                path = "";
                return false;
            }

            path = scene.path.Split('.')[0];
            string[] subpath = path.Split('/');
            path = "";
            for (int i = 1; i < subpath.Length; i++)
                path += '/' + subpath[i];

            DirectoryInfo dir = new DirectoryInfo(Application.dataPath + path);
            if (!dir.Exists)
            {
                dir.Create();
                AssetDatabase.Refresh();
            }

            return true;
        }
        
        /// <summary>
        /// 烘焙的时候顺便创造相机VP矩阵数据
        /// </summary>
        private static void CreateCameraData(ReflectorProbe probe)
        {
            //如果探针已经有了数据。返回
            if (probe.BakeCameraData)
            {
                return;
            }
            //如果探针没有数据。检测SO是否创建，没创建就创建，创建了就赋值。
            string path;
            if (!CreateDirectory(out path))
                return;
            //要创建的路径
            string asset = "Assets" + path + '/' +  "BakedCarameData.asset";
            BakeCameraData bakeCameraData = AssetDatabase.LoadAssetAtPath<BakeCameraData>(asset);
            if (!bakeCameraData)
            {
                bakeCameraData = ScriptableObject.CreateInstance<BakeCameraData>();
                AssetDatabase.CreateAsset(bakeCameraData, asset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            probe.BakeCameraData = bakeCameraData;
        }
    }
}