using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace AHD2TimeOfDay
{
    public class IBLTool : EditorWindow
    {
        [MenuItem("Tools/IBL Tool")]
        public static void ShowWindow()
        {
            GetWindow<IBLTool>("IBL Tool");
        }

        private ReorderableList cubemapList;
        private List<Cubemap> cubemaps = new List<Cubemap>(); //cubemap数组
        Vector2 scrollPos; //滚动条

        private bool isDebug = false;
        private RenderTexture rt0, rt1;
        private int count = 64; //采样次数
        private string path; //生成图片保存路径

        private Material material; //irradiance绘制材质

        private void OnEnable()
        {
            //创建ReorderableList 参数：
            //arg1:序列化物体，arg2:序列化数据，arg3:可否拖动，arg4:是否显示标题，arg5:是否显示添加按钮，arg6:是否显示添加按钮
            cubemapList = new ReorderableList(cubemaps, typeof(Cubemap), true, true, true, true);

            cubemapList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Cubemaps"); };

            cubemapList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index < cubemaps.Count)
                {
                    rect.y += 2;
                    cubemaps[index] = (Cubemap)EditorGUI.ObjectField(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        cubemaps[index],
                        typeof(Cubemap),
                        false
                    );
                }
            };

            cubemapList.onAddCallback = (ReorderableList list) =>
            {
                cubemaps.Add(new Cubemap(32, TextureFormat.RGBA64, false));
            };
            cubemapList.onRemoveCallback = (ReorderableList list) => { cubemaps.RemoveAt(list.index); };
        }

        private void OnGUI()
        {
            GUILayout.Label("IBL Tool", EditorStyles.boldLabel);
            cubemapList.DoLayoutList(); //cubemap列表

            isDebug = EditorGUILayout.Toggle("Debug", isDebug);
            if (isDebug)
            {
                EditorGUI.indentLevel++;
                rt0 = (RenderTexture)EditorGUILayout.ObjectField("Final", rt0, typeof(RenderTexture), false);
                rt1 = (RenderTexture)EditorGUILayout.ObjectField("Original", rt1, typeof(RenderTexture), false);
                EditorGUI.indentLevel--;
            }

            count = EditorGUILayout.IntSlider("Sample Count", count, 4, 1024);

            if (GUILayout.Button("Generate Irradiance Map"))
            {
                //打开文件选择
                // 弹出文件选择窗口  
                string assetPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID); //打开当前Project窗口打开的文件夹
                if (string.IsNullOrEmpty(assetPath))
                {
                    assetPath = "Assets";
                }

                path = EditorUtility.OpenFolderPanel("选择路径", assetPath, "");
                //做一个判空，检测用户关闭弹窗
                if (path == "")
                {
                    GUIUtility.ExitGUI(); //提前结束绘制，不加这个报错不匹配
                    return;
                }

                path = path.Replace(Application.dataPath, "Assets"); //转换相对Asset路径
                for (int i = 0; i < cubemaps.Count; i++)
                {
                    Create(i);
                }

                AssetDatabase.Refresh();
            }
        }

        private void Create(int i) //i为cubemap数组索引
        {
            if (!material)
            {
                material = new Material(Shader.Find("Hidden/IBLMaker_CubeMap")); //绘制irradiancemap的材质
            }

            rt0 = new RenderTexture(cubemaps[i].width * 2, cubemaps[i].width, 0, RenderTextureFormat.ARGBFloat);
            rt1 = new RenderTexture(cubemaps[i].width * 2, cubemaps[i].width, 0, RenderTextureFormat.ARGBFloat);
            rt0.wrapMode = TextureWrapMode.Repeat;
            rt1.wrapMode = TextureWrapMode.Repeat;
            rt0.Create();
            rt1.Create();
            Graphics.Blit(cubemaps[i], rt0, material, 0); //rt0存irradiancemap（这里是用pass0，先从cubemap转成球面投影图
            material.SetTexture("_CubeTex", cubemaps[i]);
            for (int j = 0; j < count; j++)
            {
                EditorUtility.DisplayProgressBar("", "采样进度", (float)j / count);
                Vector3 n = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.0000001f, 1f),
                    Random.Range(-1f, 1f)
                );
                while (n.magnitude > 1)
                    n = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(0.0000001f, 1f),
                        Random.Range(-1f, 1f)
                    ); //Rejection Sampling保证采样点在半球上均一。
                n = n.normalized;
                material.SetVector("_RandomVector", new Vector4(
                    n.x, n.y, n.z,
                    1f / (j + 2)
                ));
                Graphics.Blit(rt0, rt1, material, 1);
                // 翻转（最终rt0是结果）
                (rt0, rt1) = (rt1, rt0);
            }

            //循环结束的时候，rt0是结果，rt是中间结果，我们想看原版图的话，重新blit一遍cubemap
            Graphics.Blit(cubemaps[i], rt1, material, 0);
            EditorUtility.ClearProgressBar();
            // 保存
            Texture2D texture = new Texture2D(cubemaps[i].width * 2, cubemaps[i].width, TextureFormat.RGBAFloat, true);
            var k = RenderTexture.active;
            RenderTexture.active = rt0;
            texture.ReadPixels(new Rect(0, 0, rt0.width, rt0.height), 0, 0);
            RenderTexture.active = k;
            byte[] bytes = texture.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);
            System.IO.FileStream fs = new System.IO.FileStream(path + "/" + cubemaps[i].name + "_irradiance.exr",
                System.IO.FileMode.Create);
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs);
            bw.Write(bytes);
            fs.Close();
            bw.Close();
        }
    }
}