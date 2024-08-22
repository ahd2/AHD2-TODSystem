using UnityEngine;

public class CustomReflectionProbe : MonoBehaviour
{
    public int cubemapSize = 128;
    private Camera cubemapCamera;
    private GameObject cubemapGO;
    public Cubemap cubemap;

    void Start()
    {
        // 创建一个新的 GameObject 来放置摄像机
        cubemapGO = new GameObject("CubemapCamera");
        cubemapCamera = cubemapGO.AddComponent<Camera>();

        // 设置摄像机属性
        cubemapCamera.backgroundColor = Color.black;
        cubemapCamera.cullingMask = ~(1 << LayerMask.NameToLayer("IgnoreReflection"));
        cubemapCamera.fieldOfView = 90;
        cubemapCamera.aspect = 1;

        // 创建 Cubemap
        cubemap = new Cubemap(cubemapSize, TextureFormat.RGB24, false);

        // 开始捕捉 Cubemap
        cubemapCamera.transform.position = transform.position;
        cubemapCamera.RenderToCubemap(cubemap);

        // 应用 Cubemap
        //ApplyCubemap();
    }

    void ApplyCubemap()
    {
        // 这里你可以将 Cubemap 应用到材质或者 Shader
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.sharedMaterial.SetTexture("_Cube", cubemap);
        }
    }

    void OnDestroy()
    {
        // 清理
        if (cubemapGO)
            Destroy(cubemapGO);
    }
}