using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
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
    protected MaterialProperty normalMap { get; set; }
    
    //存放下拉框Item的GUIContent（其实没必要翻译，但是翻译了证明我来过）
    protected class Styles
    {
        public static readonly GUIContent SurfaceInputs = EditorGUIUtility.TrTextContent("表面输入",
            "决定物体看起来啥样。");
        public static GUIContent MainTex = EditorGUIUtility.TrTextContent("漫反射图", "What can i say? Man");//tips 是说明文字，鼠标悬停属性名称时显示 ,text是面板上显示的名称（可以为中文）
        public static GUIContent NormalMap = EditorGUIUtility.TrTextContent("法线贴图", "normalMap");//tips 是说明文字，鼠标悬停属性名称时显示 ,text是面板上显示的名称（可以为中文）
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
    }

    public bool m_FirstTimeApply = true;//面板是否首充打开，用于OnOpenGUI函数调用

    readonly MaterialHeaderScopeList m_MaterialScopeList = new MaterialHeaderScopeList(uint.MaxValue & ~(uint)Expandable.Advanced);
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        //base.OnGUI(materialEditor, properties);
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
        normalMap = FindProperty("_NormalMap", properties, true); //第三个参数是未找到属性时是否抛出异常
    }
}
