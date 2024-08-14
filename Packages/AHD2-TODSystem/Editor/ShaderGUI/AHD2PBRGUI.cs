using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AHD2PBRGUI : ShaderGUI
{
    
    MaterialEditor editor;
    Object[] materials;
    MaterialProperty[] properties;
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        //base.OnGUI(materialEditor, properties);
        //这些参数存为成员变量，为了让它们在整个类里都可以调用。
        editor = materialEditor;
        materials = materialEditor.targets;
        this.properties = properties;
        
        MaterialProperty maintex = FindProperty("_MainTex", properties, true); //第三个参数是未找到属性时是否抛出异常
        GUIContent content = new GUIContent(maintex.displayName, maintex.textureValue, "maintex");//tips 是说明文字，鼠标悬停属性名称时显示 ,text是面板上显示的名称（可以为中文）
        //materialEditor.TexturePropertySingleLine(content, maintex);
        MaterialProperty basecol = FindProperty("_BaseColor", properties, true);
        materialEditor.TexturePropertySingleLine(content, maintex, basecol);//重载方法
        materialEditor.TextureScaleOffsetProperty(maintex);
        
        MaterialProperty normalMap = FindProperty("_NormalMap", properties, true); //第三个参数是未找到属性时是否抛出异常
        GUIContent content1 = new GUIContent("法线贴图", maintex.textureValue, "normalMap");//tips 是说明文字，鼠标悬停属性名称时显示 ,text是面板上显示的名称（可以为中文）
        materialEditor.TexturePropertySingleLine(content1, normalMap);
        
    }
    //根据名字给shader属性赋值浮点数
    bool SetProperty (string name, float value) {
        MaterialProperty property = FindProperty(name, properties, false);
        if (property != null) {
            property.floatValue = value;
            return true;
        }
        return false;
    }
}
