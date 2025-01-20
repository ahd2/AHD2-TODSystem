using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AHD2TimeOfDay
{
    [ExecuteAlways]
    public class TODController : MonoBehaviour
    {
        public TODGlobalParameters todGlobalParameters; //未拖入也会报错，不处理了
        public Light MainLight; //主光源
        private Vector3 starquat; //计算光源旋转用
        private Vector3 endquat; //计算光源旋转用
        private Vector3 lightDirection; //假光源方向，360度转
        private static readonly int LightColor = Shader.PropertyToID("_lightColor");
        private static readonly int LightDirection = Shader.PropertyToID("_lightDirection");
        private static readonly int TodTimeRatio = Shader.PropertyToID("_todTimeRatio");
        private static readonly int IblBrdfLut = Shader.PropertyToID("_iblBrdfLut");

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
            starquat = new Vector3(-90, 0, 0); //对应0点
            endquat = new Vector3(270, 0, 0); //对应24点
            //光源设置为color模式
            
        }

        /// <summary>
        /// 根据当前时间更新主光源旋转
        /// </summary>
        void RotateLight()
        {
            //欧拉角插值
            Vector3 quat = Vector3.Lerp(starquat, endquat, todGlobalParameters.CurrentTime / 24);
            lightDirection = -(Quaternion.Euler(quat) * Vector3.forward).normalized; //要反向，指向光源
            if (todGlobalParameters._dayOrNight == 1)
            {
                quat.x += 180; //晚上则反向
                //quat.x = quat.x % 360;
            }

            //Debug.Log(quat.x);//后续考虑要不要固定光照，让其不会在10°后再下降。
            // if (quat.x < 10)
            // {
            //     quat.x = 10;
            // }
            //
            // if (quat.x > 170)
            // {
            //     quat.x = 170;
            // }
            MainLight.transform.rotation = Quaternion.Euler(quat);
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
            //shader设置
            //Shader.SetGlobalColor(LightColor, todGlobalParameters._lightColor);
            Shader.SetGlobalVector(LightDirection,
                new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, todGlobalParameters._dayOrNight));
            Shader.SetGlobalFloat(TodTimeRatio, todGlobalParameters.todElapsedTimeRatio);
            Shader.SetGlobalTexture(IblBrdfLut, todGlobalParameters.IblBrdfLut);
        }
    }
}