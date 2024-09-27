using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AHD2TimeOfDay
{
    [System.Serializable] //让其可被序列化，存入SO
    public class TempTOD
    {
        [SerializeField] public string name;
        [SerializeField] private float _time;

        public float Time
        {
            get { return _time; }
            set
            {
                //保证不管输入什么值，始终在0-24之间循环
                if (value >= 24 || value < 0)
                {
                    Debug.LogError("无效值。");
                }
                else
                {
                    _time = value;
                }
            }
        }
    }
}