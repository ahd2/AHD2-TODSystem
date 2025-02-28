using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public class AHD2PBRGUI : ShaderGUI
{
    [Flags]
    protected enum Expandable
    {
        SurfaceOptions = 1 << 0,
        SurfaceInputs = 1 << 1,
        Advanced = 1 << 2,
        Details = 1 << 3,
    }
    
    MaterialEditor editor;
    Object[] materials;
    MaterialProperty[] properties;
    
    //所有要用到的property
    public MaterialProperty maintex { get; set; }
    protected MaterialProperty basecol { get; set; }
    protected MaterialProperty emissioncol { get; set; }
    protected MaterialProperty normalMap { get; set; }
    protected MaterialProperty RMOMap { get; set; }
    protected MaterialProperty metallic { get; set; }
    protected MaterialProperty roughness { get; set; }
    protected MaterialProperty aoMap { get; set; }
    
    //存放下拉框Item的GUIContent（其实没必要翻译，但是翻译了证明我来过）
    protected class Styles
    {
        public static readonly GUIContent SurfaceInputs = EditorGUIUtility.TrTextContent("表面输入",
            "决定物体看起来啥样。");
        public static GUIContent MainTex = EditorGUIUtility.TrTextContent("漫反射图", "What can i say? Man");//tips 是说明文字，鼠标悬停属性名称时显示 ,text是面板上显示的名称（可以为中文）
        public static GUIContent NormalMap = EditorGUIUtility.TrTextContent("法线贴图", "NormalMap");//tips 是说明文字，鼠标悬停属性名称时显示 ,text是面板上显示的名称（可以为中文）
        public static GUIContent RMOMap = EditorGUIUtility.TrTextContent("RMO(粗糙度、金属度、AO)贴图", "RMOMap");
        public static GUIContent Metallic = EditorGUIUtility.TrTextContent("金属度", "Metallic");
        public static GUIContent Roughness = EditorGUIUtility.TrTextContent("粗糙度", "Roughness");
        public static GUIContent AOMap = EditorGUIUtility.TrTextContent("AO贴图", "AOMap");
    }
    
    protected virtual uint materialFilter => uint.MaxValue;
    public virtual void OnOpenGUI(Material material, MaterialEditor materialEditor)
    {
        var filter = (Expandable)materialFilter;
        //注册下拉框item进下拉框列表
        if (filter.HasFlag(Expandable.SurfaceOptions))
            m_MaterialScopeList.RegisterHeaderScope(Styles.SurfaceInputs, (uint)Expandable.SurfaceInputs, DrawSurfaceInputs);
    }
    /// <summary>
    /// 只处理绘制逻辑
    /// </summary>
    /// <param name="obj"></param>
    private void DrawSurfaceInputs(Material obj)
    {
        //materialEditor.TexturePropertySingleLine(content, maintex);
        editor.TexturePropertySingleLine(Styles.MainTex, maintex, basecol);//重载方法
        editor.TextureScaleOffsetProperty(maintex);
        //画法线贴图部分
        editor.TexturePropertySingleLine(Styles.NormalMap, normalMap);
        editor.TextureScaleOffsetProperty(normalMap);
        //画金属度贴图
        bool hasRMOMap = RMOMap.textureValue != null;
        editor.TexturePropertySingleLine(Styles.RMOMap, RMOMap);
        if (!hasRMOMap)//如果没拖入RMO贴图
        {
            EditorGUI.indentLevel += 2;
            //金属度滑杆
            editor.ShaderProperty(metallic, Styles.Metallic);
            //画粗糙度
            editor.ShaderProperty(roughness, Styles.Roughness);
            //AO图
            editor.TexturePropertySingleLine(Styles.AOMap, aoMap);
            EditorGUI.indentLevel-= 2;
        }

        editor.ColorProperty(emissioncol, "自发光颜色");
    }

    public bool m_FirstTimeApply = true;//面板是否首充打开，用于OnOpenGUI函数调用

    readonly MaterialHeaderScopeList m_MaterialScopeList = new MaterialHeaderScopeList(uint.MaxValue & ~(uint)Expandable.Advanced);
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        //这些参数存为成员变量，为了让它们在整个类里都可以调用。
        editor = materialEditor;
        materials = materialEditor.targets;
        this.properties = properties;
        
        FindProperties(properties);//初始化所有成员property
        if (m_FirstTimeApply)
        {
            OnOpenGUI((Material)materials[0], editor);
            m_FirstTimeApply = false;
        }
        m_MaterialScopeList.DrawHeaders(materialEditor, (Material)materials[0]);
    }
    //初始化property
    public virtual void FindProperties(MaterialProperty[] properties)
    {
        var material = editor?.target as Material;
        if (material == null)
            return;

        maintex = FindProperty("_MainTex", properties, true); //第三个参数是未找到属性时是否抛出异常
        basecol = FindProperty("_BaseColor", properties, true);
        emissioncol = FindProperty("_EmissionColor", properties, true);
        normalMap = FindProperty("_NormalMap", properties, true); //第三个参数是未找到属性时是否抛出异常
        RMOMap = FindProperty("_RMOMap", properties, false);
        metallic = FindProperty("_Metallic", properties, false);
        roughness = FindProperty("_Roughness", this.properties, false);
        aoMap = FindProperty("_AOMap", this.properties, false);
    }
    
    // material changed check
    public override void ValidateMaterial(Material material)
    {
        SetMaterialKeywords(material);
    }

    public static void SetMaterialKeywords(Material material)
    {
        var hasRMOMap = material.GetTexture("_RMOMap") != null;
        CoreUtils.SetKeyword(material, "_RMOMAP", hasRMOMap);
    }
}
