using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(ReflectionProbe)), ExecuteInEditMode]//需要与反射探针组件共存
public class CustomReflectionProbe : MonoBehaviour
{
    public Cubemap cubemap;
    //public Camera Camera;
    private static Quaternion[] orientations = new Quaternion[]
    {
        Quaternion.LookRotation(Vector3.right, Vector3.down),
        Quaternion.LookRotation(Vector3.left, Vector3.down),
        Quaternion.LookRotation(Vector3.up, Vector3.forward),
        Quaternion.LookRotation(Vector3.down, Vector3.back),
        Quaternion.LookRotation(Vector3.forward, Vector3.down),
        Quaternion.LookRotation(Vector3.back, Vector3.down)
    };

    private void Start()
    {
        cubemap = BakeReflection();
    }

    private void Update()
    {
    }

    public Cubemap BakeReflection()
    {

        int resolution = GetComponent<ReflectionProbe>().resolution;

        Cubemap cubemap = new Cubemap(resolution, TextureFormat.RGB24, true);
        Texture2D reader = new Texture2D(resolution, resolution, TextureFormat.RGB24, true, true);
        
        RenderTexture render1 = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        render1.useMipMap = false;
        render1.Create();

        RenderTexture mirror1 = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        mirror1.useMipMap = false;
        mirror1.Create();

        Camera camera = CreateCameraObjects();
        // camera.gameObject.SetActive(true);
        // camera.transform.position = transform.position;
        camera.targetTexture = render1;
        //
        // camera.hideFlags = HideFlags.HideAndDontSave;
        // camera.enabled = false;
        // camera.gameObject.SetActive(true);
        // camera.cameraType = CameraType.Reflection;
        // camera.fieldOfView = 90;

        for (int face = 0; face < 6; face++)
        {
            camera.transform.rotation = orientations[face];

            Shader.EnableKeyword("NO_REFLECTION");
            Shader shader = Shader.Find("ReflectionProbe/NormalAndDiffuse");
            //Debug.Log(shader);
            camera.SetReplacementShader(shader, "RenderType");
            camera.Render();
            Shader.DisableKeyword("NO_REFLECTION");

            Graphics.Blit(render1, mirror1);

            RenderTexture source =  mirror1;
            RenderTexture destination = render1;

            RenderTexture.active = source;
            reader.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            cubemap.SetPixels(reader.GetPixels(), (CubemapFace)face);
            RenderTexture.active = null;
        }
        return cubemap;
    }
    private Camera CreateCameraObjects()
    {
        var go = new GameObject("Planar Reflections",typeof(Camera));
        var cameraData = go.AddComponent(typeof(UniversalAdditionalCameraData)) as UniversalAdditionalCameraData;

        cameraData.requiresColorOption = CameraOverrideOption.Off;
        cameraData.requiresDepthOption = CameraOverrideOption.Off;
        cameraData.SetRenderer(1);
        var reflectionCamera = go.GetComponent<Camera>();
        reflectionCamera.transform.position = transform.position;
        reflectionCamera.enabled = false;
        go.hideFlags = HideFlags.HideAndDontSave;
        reflectionCamera.fieldOfView = 90;
        reflectionCamera.cameraType = CameraType.Reflection;

        return reflectionCamera;
    }
}