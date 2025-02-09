using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace AHD2TimeOfDay
{
    /// <summary>
    /// 管理全局参数的类
    /// </summary>
    [CreateAssetMenu(fileName = "TODGlobalParameters", menuName = "AHD2TODSystem/GlobalParameters", order = 0)]
    public class TODGlobalParameters : ScriptableObject
    {
        //===============================基本时间信息====================
        #region Variables0
        
        //时间流逝速度
        public float timeFlowSpeed = 2.0f;
        
        [SerializeField, Tooltip("一天等于现实多少分钟？")]
        public float timeFlowScale = 10;
        
        [SerializeField, Range(0f, 24f), Tooltip("时间")]
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
                else if (value < 0)
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
        public bool isTimeFlow; //私有的加_，公开的不加
        
        public TimeOfDay[] timeOfDays; //tod数组
        public TimeOfDay currentTimeOfDay;
        
        public int _dayOrNight; //0为白天1为夜晚
        
        /*[HideInInspector]*/
        public float todElapsedTimeRatio; //当前时段已经经过的时间比例（插值用）
        
        public float todElapsedTime; //当前时段已经经过的时间
        
        [SerializeField] public List<TempTOD> todFrameList = new List<TempTOD>(); //维护关键帧列表，工具用
        
        #endregion
        
        //=========================插值后的全局参数=======================
        #region Variables1

        //光源
        public Color MainlightColor; //光源色
        public float MainlightIntensity;
        public Vector3 MainlightDirection;
        
        //雾效
        public Color FogLightColor;
        public float FogLightIntensity;
        public float HGCoefficient;
        
        public Texture2D IblBrdfLut;

        #endregion
        
        //========================插值后的材质==========================
        public Material[] materials = Array.Empty<Material>(); //插值材质数组，场景中使用的材质放这里。
        
        //======================函数部分================================
        #region 函数区域

        /// <summary>
        /// 初始化参数
        /// </summary>
        public void Initailize()
        {
            //加载时间（如果没有，返回默认值CurrentTime）
            CurrentTime = PlayerPrefs.GetFloat("ahd2_time", CurrentTime);
        }

        public void SavedTime()
        {
            //保存时间
            PlayerPrefs.SetFloat("ahd2_time", CurrentTime);
        }

        /// <summary>
        /// 全局参数类的基础update，
        /// </summary>
        public void BaseUpdate()
        {
            if (isTimeFlow)
            {
                //如果时间流动。
                CurrentTime += Time.deltaTime * timeFlowSpeed * 0.4f / timeFlowScale;
                todElapsedTime += Time.deltaTime * timeFlowSpeed * 0.4f / timeFlowScale;
                UpdateTimeOfDay();
                todElapsedTimeRatio = todElapsedTime / currentTimeOfDay.duration;
                LerpProperties();
            }
            else
            {
                FixTimeOfDay();
                todElapsedTimeRatio = todElapsedTime / currentTimeOfDay.duration;
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
                if (CurrentTime >= currentTimeOfDay.NextTODTime && CurrentTime < currentTimeOfDay.CurrentTODTime)
                {
                    //进入下一个时刻
                    currentTimeOfDay = currentTimeOfDay.nextTOD;
                    //把当前时刻已经经过的时间归零
                    todElapsedTime = 0;
                }
            }
            else
            {
                if (CurrentTime >= currentTimeOfDay.NextTODTime)
                {
                    //进入下一个时刻
                    currentTimeOfDay = currentTimeOfDay.nextTOD;
                    //把当前时刻已经经过的时间归零
                    todElapsedTime = 0;
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
                    if (CurrentTime < timeOfDay.NextTODTime || CurrentTime >= timeOfDay.CurrentTODTime)
                    {
                        //切换至该时刻
                        currentTimeOfDay = timeOfDay;
                        //更新当前时刻已经经过的时间
                        if (CurrentTime - timeOfDay.CurrentTODTime >= 0)
                        {
                            todElapsedTime = CurrentTime - timeOfDay.CurrentTODTime;
                        }
                        else
                        {
                            todElapsedTime = 24 - timeOfDay.CurrentTODTime + CurrentTime;
                        }

                        return;
                    }
                }
                else
                {
                    //如果时间在当前时刻中
                    if (CurrentTime < timeOfDay.NextTODTime && CurrentTime >= timeOfDay.CurrentTODTime)
                    {
                        //切换至该时刻
                        currentTimeOfDay = timeOfDay;
                        //更新当前时刻已经经过的时间
                        todElapsedTime = CurrentTime - timeOfDay.CurrentTODTime;
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
                materials[i].Lerp(currentTimeOfDay.materials[i], currentTimeOfDay.nextTOD.materials[i], todElapsedTimeRatio);
            }

            FogLightColor = Color.Lerp(currentTimeOfDay.FogLightColor, currentTimeOfDay.nextTOD.FogLightColor, todElapsedTimeRatio);
            FogLightIntensity = Mathf.Lerp(currentTimeOfDay.FogLightIntensity, currentTimeOfDay.nextTOD.FogLightIntensity,
                todElapsedTimeRatio);
            HGCoefficient = Mathf.Lerp(currentTimeOfDay.HGCoefficient, currentTimeOfDay.nextTOD.HGCoefficient,
                todElapsedTimeRatio);
        }

        /// <summary>
        /// 计算光源参数
        /// </summary>
        public void CalLightParam()
        {
            //插值光源色和强度
            MainlightColor = Color.Lerp(currentTimeOfDay.MainlightColor, currentTimeOfDay.nextTOD.MainlightColor, todElapsedTimeRatio);
            MainlightIntensity = Mathf.Lerp(currentTimeOfDay.MainlightIntensity, currentTimeOfDay.nextTOD.MainlightIntensity,
                todElapsedTimeRatio);
            MainlightDirection = Vector3.Lerp(currentTimeOfDay.MainlightDirection,
                currentTimeOfDay.nextTOD.MainlightDirection, todElapsedTimeRatio);
            //传入预计算光源方向，a通道为昼夜标记
            _dayOrNight = Convert.ToInt32(currentTimeOfDay.dayOrNight);
        }

        #endregion
    }
}