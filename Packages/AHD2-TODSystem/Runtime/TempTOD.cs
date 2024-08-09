using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempTOD
{
    public string name;
    private float _time;
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
