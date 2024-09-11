using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 管理全局参数的类
/// </summary>
[CreateAssetMenu(fileName = "TODGlobalParameters", menuName = "AHD2TODSystem/GlobalParameters", order = 0)]
public class TODGlobalParameters : ScriptableObject
{
    public float timeFlowSpeed = 2.0f;
    [SerializeField,Range(0f,24f),Tooltip("时间")]
    private float _currentTime;
    public float CurrentTime
    {
        get { return _currentTime; }
        set
        {
            //保证不管输入什么值，始终在0-24之间循环
            if (value >= 24)
            {
                _currentTime = value % 24;
            }
            else if(value < 0 )
            {
                throw new ArgumentException("currentTime违规。");
            }
            else
            {
                _currentTime = value;
            }
        }
    }
    //控制时间是否流动
    public bool timeFlow; //私有的加_，公开的不加
    public Material[] materials;//插值材质数组，场景中使用的材质放这里。
    public TimeOfDay[] timeOfDays;//tod数组
    public TimeOfDay currentTimeOfDay;
    public float todTime;//当前tod已经经过的时间
    /*[HideInInspector]*/public float todTimeRatio;//当前tod已经经过的时间比例（插值用）
    [SerializeField]public List<TempTOD> todFrameList;//维护关键帧列表，工具用
    
    //光源
    [ColorUsageAttribute(false,true)]
    public Color _lightColor;//光源色
    public int _dayOrNight;//0为白天1为夜晚
    public Texture2D IblBrdfLut;

    //====================================================================================================
    #region 函数区域
    /// <summary>
    /// 初始化参数
    /// </summary>
    public void Initailize()
    {
        lerpTextureComputeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.ahd2.tod-system/Shaders/IBL/LerpTexture.compute");
        if (lerpTextureComputeShader == null)
        {
            throw new FileNotFoundException("Failed to load LerpTexture Compute Shader.");
        }
        s_lerpKernal = lerpTextureComputeShader.FindKernel("LerpTexture");
        irradianceMap = new RenderTexture(128, 64, 0, RenderTextureFormat.ARGBFloat);//固定宽高，后续开放选择
        irradianceMap.enableRandomWrite = true; // 如果需要写入到Render Texture，可能需要这个  
        irradianceMap.Create();  
    }
    /// <summary>
    /// 全局参数类的基础update，
    /// </summary>
    public void BaseUpdate()
    {
        if (timeFlow)
        {
            //如果时间流动。
            CurrentTime += Time.deltaTime * timeFlowSpeed;
            todTime += Time.deltaTime * timeFlowSpeed;
            UpdateTimeOfDay();
            todTimeRatio = todTime / currentTimeOfDay.duration;
            LerpProperties();
        }
        else
        {
            FixTimeOfDay();
            todTimeRatio = todTime / currentTimeOfDay.duration;
            LerpProperties();
        }
        CalLightParam();
    }
    /// <summary>
    /// 更新当前时刻,和fix的区别在于，fix消耗更大（遍历所有tod），在时间流动的时候，我们可以确保tod是线性流动，所以可以用这个函数。
    /// </summary>
    public void UpdateTimeOfDay()
    {
        //如果当前时刻和下个时刻中间隔了24点
        if (currentTimeOfDay.isCross24)
        {
            //如果时间不在当前时刻中
            if (CurrentTime >= currentTimeOfDay.endTime&&CurrentTime < currentTimeOfDay.startTime)
            {
                //进入下一个时刻
                currentTimeOfDay = currentTimeOfDay.nextTOD;
                //把当前时刻已经经过的时间归零
                todTime = 0;
            }
        }
        else
        {
            if (CurrentTime >= currentTimeOfDay.endTime)
            {
                //进入下一个时刻
                currentTimeOfDay = currentTimeOfDay.nextTOD;
                //把当前时刻已经经过的时间归零
                todTime = 0;
            }
        }
    }
    
    /// <summary>
    /// 修正时刻段，让当前时刻段对应上当前时间
    /// </summary>
    public void FixTimeOfDay()
    {
        //遍历一天时刻
        foreach (TimeOfDay timeOfDay in timeOfDays)
        {
            //如果当前时刻和下个时刻中间隔了24点
            if (timeOfDay.isCross24)
            {
                //如果时间在当前时刻中
                if (CurrentTime <= timeOfDay.endTime||CurrentTime > timeOfDay.startTime)
                {
                    //切换至该时刻
                    currentTimeOfDay = timeOfDay;
                    //更新当前时刻已经经过的时间
                    if (CurrentTime - timeOfDay.startTime > 0)
                    {
                        todTime = CurrentTime - timeOfDay.startTime;
                    }
                    else
                    {
                        todTime = 24 - timeOfDay.startTime + CurrentTime;
                    }
                    return;
                }
            }
            else
            {
                //如果时间在当前时刻中
                if (CurrentTime < timeOfDay.endTime &&CurrentTime >= timeOfDay.startTime)
                {
                    //切换至该时刻
                    currentTimeOfDay = timeOfDay;
                    //更新当前时刻已经经过的时间
                    todTime = CurrentTime - timeOfDay.startTime;
                    return;
                }
            }
        }
    }
    
    /// <summary>
    /// 混合关键帧之间的材质或其他数据
    /// </summary>
    public void LerpProperties()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].Lerp(currentTimeOfDay.materials[i],currentTimeOfDay.nextTOD.materials[i],todTimeRatio);
        }
    }
    
    /// <summary>
    /// 计算光源参数
    /// </summary>
    public void CalLightParam()
    {
        //插值光源色（）
        _lightColor = Color.Lerp(currentTimeOfDay.lightColor,currentTimeOfDay.nextTOD.lightColor,todTimeRatio);
        //传入预计算光源方向，a通道为昼夜标记
        _dayOrNight = Convert.ToInt32(currentTimeOfDay.datOrNight);
    }

    private static int s_lerpKernal;
    private ComputeShader lerpTextureComputeShader;
    private static readonly int IrradianceMap0 = Shader.PropertyToID("_irradianceMap0");
    private static readonly int IrradianceMap1 = Shader.PropertyToID("_irradianceMap1");
    private static readonly int TodTimeRatio = Shader.PropertyToID("_todTimeRatio");
    private static readonly int IrradianceMap = Shader.PropertyToID("_IrradianceMap");
    private RenderTexture irradianceMap;

    public void LerpIrradianceMap()
    {
        lerpTextureComputeShader.SetTexture(s_lerpKernal, IrradianceMap, irradianceMap);
        lerpTextureComputeShader.SetTexture(s_lerpKernal, IrradianceMap0, currentTimeOfDay.irrdianceMap);
        lerpTextureComputeShader.SetTexture(s_lerpKernal, IrradianceMap1, currentTimeOfDay.nextTOD.irrdianceMap);
        lerpTextureComputeShader.SetFloat(TodTimeRatio, todTimeRatio);
        lerpTextureComputeShader.Dispatch(s_lerpKernal, 16, 8, 1); //宽高/8，后续更改
        Shader.SetGlobalTexture(IrradianceMap, irradianceMap);
    }

    #endregion
}
