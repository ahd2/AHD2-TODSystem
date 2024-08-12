using System.IO;
using UnityEditor;
using UnityEngine;

public class IBLTool : EditorWindow
{
    [MenuItem("Tools/IBL Tool")]
    public static void ShowWindow()
    {
        GetWindow<IBLTool>("IBL Tool");
    }

    private Cubemap cubemap;
    private bool isDebug = false;
    private RenderTexture rt0, rt1;
    private int count = 64;//采样次数
    private string path = "IBL";//生成图片保存路径

    private Material material;//irradiance绘制材质

    private void OnGUI()
    {
        GUILayout.Label("IBL Tool", EditorStyles.boldLabel);
        cubemap = (Cubemap)EditorGUILayout.ObjectField("Cubemap", cubemap, typeof(Cubemap), false);
        isDebug = EditorGUILayout.Toggle("Debug", isDebug);
        if (isDebug)
        {
            EditorGUI.indentLevel++;
            rt0 = (RenderTexture)EditorGUILayout.ObjectField("Final", rt0, typeof(RenderTexture), false);
            rt1 = (RenderTexture)EditorGUILayout.ObjectField("Original", rt1, typeof(RenderTexture), false);
            EditorGUI.indentLevel--;
        }
        count = EditorGUILayout.IntSlider("Sample Count", count, 4, 1024);
        path = EditorGUILayout.TextField("Path", path);

        if (GUILayout.Button("Generate Irradiance Map"))
        {
            Create();
        }
    }

    private void Create()
    {
        if (!material)
        {
            material = new Material(Shader.Find("Hidden/IBLMaker_CubeMap"));//绘制irradiancemap的材质
        }
        rt0 = new RenderTexture(cubemap.width * 2, cubemap.width, 0, RenderTextureFormat.ARGBFloat);
        rt1 = new RenderTexture(cubemap.width * 2, cubemap.width, 0, RenderTextureFormat.ARGBFloat);
        rt0.wrapMode = TextureWrapMode.Repeat;
        rt1.wrapMode = TextureWrapMode.Repeat;
        rt0.Create();
        rt1.Create();
        Graphics.Blit(cubemap, rt0, material, 0);//rt0存irradiancemap（这里是用pass0，先从cubemap转成球面投影图
        material.SetTexture("_CubeTex", cubemap);
        for (int i = 0; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("", "采样进度", (float)i / count);
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
                    );//Rejection Sampling保证采样点在半球上均一。
            n = n.normalized;
            material.SetVector("_RandomVector", new Vector4(
                n.x, n.y, n.z,
                1f / (i + 2)
                ));
            Graphics.Blit(rt0, rt1, material, 1); 
            // 翻转（最终rt0是结果）
            (rt0, rt1) = (rt1, rt0);
        }
        //循环结束的时候，rt0是结果，rt是中间结果，我们想看原版图的话，重新blit一遍cubemap
        Graphics.Blit(cubemap, rt1, material, 0);
        EditorUtility.ClearProgressBar();
        // 保存
        Texture2D texture = new Texture2D(cubemap.width * 2, cubemap.width, TextureFormat.ARGB32, true);
        var k = RenderTexture.active;
        RenderTexture.active = rt0;
        texture.ReadPixels(new Rect(0, 0, rt0.width, rt0.height), 0, 0);
        RenderTexture.active = k;
        byte[] bytes = texture.EncodeToPNG();
        System.IO.FileStream fs = new System.IO.FileStream(System.IO.Path.Combine(Application.dataPath, path) + "/" + texture.name + "_irradiance.png", System.IO.FileMode.Create);
        System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs);
        bw.Write(bytes);
        fs.Close();
        bw.Close();
    }
}