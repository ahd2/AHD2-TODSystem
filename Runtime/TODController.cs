using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AHD2TimeOfDay
{
    [ExecuteAlways]
    public class TODController : MonoBehaviour
    {
        public TODGlobalParameters todGlobalParameters; //未拖入也会报错，不处理了
        public Light MainLight; //主光源
        public LensFlareComponentSRP LensFlare;
        [SerializeField]public bool isTimeFlow;
        private Vector3 _starquat; //计算光源旋转用
        private Vector3 _endquat; //计算光源旋转用
        private Vector3 _fakeMainlightDirection; //假光源方向，360度转
        private static readonly int AHD2_MainlightColor = Shader.PropertyToID("AHD2_MainlightColor");
        private static readonly int AHD2_MainlightIntensity = Shader.PropertyToID("AHD2_MainlightIntensity");
        private static readonly int AHD2_FakeMainlightDirection = Shader.PropertyToID("AHD2_FakeMainlightDirection");
        private static readonly int TodTimeRatio = Shader.PropertyToID("_todTimeRatio");
        private static readonly int IblBrdfLut = Shader.PropertyToID("_iblBrdfLut");
        private static readonly int AHD2_FoglightColor = Shader.PropertyToID("AHD2_FoglightColor");
        private static readonly int AHD2_HGCoefficient = Shader.PropertyToID("AHD2_HGCoefficient");

        void Start()
        {
            todGlobalParameters.Initailize();
            //todGlobalParameters.CurrentTime = 6;//从6点开始
            foreach (var tod in todGlobalParameters.timeOfDays)
            {
                tod.Initialize();
            }

            todGlobalParameters.FixTimeOfDay(); //调整时间段符合当前时间
            InitialLight(); //光源需要拖入(unity自己会报错，不处理了。)
        }

        void Update()
        {
            todGlobalParameters.BaseUpdate();
            SetGlobalParameters();
            RotateLight();
        }
        
        //退出前保存时间
        private void OnDestroy()
        {
            todGlobalParameters.SavedTime();
        }

        /// <summary>
        /// 初始化光源相关数据
        /// </summary>
        void InitialLight()
        {
            _starquat = new Vector3(-90, 0, 0); //对应0点
            _endquat = new Vector3(270, 0, 0); //对应24点
            //光源设置为color模式
            
        }

        /// <summary>
        /// 根据当前时间更新主光源旋转
        /// </summary>
        void RotateLight()
        {
            //欧拉角插值
            Vector3 quat = Vector3.Lerp(_starquat, _endquat, todGlobalParameters.CurrentTime / 24);//360旋转的假光源角度
            _fakeMainlightDirection = -(Quaternion.Euler(quat) * Vector3.forward).normalized; //要反向，指向光源
        }

        /// <summary>
        /// 设置全局参数
        /// </summary>
        public void SetGlobalParameters()
        {
            //cpu端设置
            //设置主光
            MainLight.color = todGlobalParameters.MainlightColor;
            MainLight.intensity = todGlobalParameters.MainlightIntensity;
            MainLight.transform.rotation = Quaternion.Euler(todGlobalParameters.MainlightDirection);//真光源角度
            //白天开启lensflare
            if (LensFlare)
            {
                LensFlare.enabled = todGlobalParameters._dayOrNight == 0;
                LensFlare.transform.rotation = Quaternion.LookRotation(-_fakeMainlightDirection);
            }
                
            //shader设置
            Shader.SetGlobalColor(AHD2_MainlightColor, todGlobalParameters.MainlightColor);
            Shader.SetGlobalFloat(AHD2_MainlightIntensity, todGlobalParameters.MainlightIntensity);
            Shader.SetGlobalVector(AHD2_FakeMainlightDirection,
                new Vector4(_fakeMainlightDirection.x, _fakeMainlightDirection.y, _fakeMainlightDirection.z, todGlobalParameters._dayOrNight));//传入的是360度旋转不会在晚上反向的方向
            Shader.SetGlobalFloat(TodTimeRatio, todGlobalParameters.todElapsedTimeRatio);
            Shader.SetGlobalTexture(IblBrdfLut, todGlobalParameters.IblBrdfLut);
            Shader.SetGlobalVector(AHD2_FoglightColor, new Vector4(todGlobalParameters.FogLightColor.r, 
                todGlobalParameters.FogLightColor.g, 
                todGlobalParameters.FogLightColor.b, 
                todGlobalParameters.FogLightIntensity));
            Shader.SetGlobalFloat(AHD2_HGCoefficient, todGlobalParameters.HGCoefficient);
        }
    }
}