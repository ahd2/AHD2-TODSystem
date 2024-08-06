using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理全局参数的类
/// </summary>
public class TODGlobalParameters
{
    private static float _currentTime;
    public static float CurrentTime
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
                Debug.LogError("Invalid value!");
                return; // 阻止无效的赋值
            }
            else
            {
                _currentTime = value;
            }
        }
    }
    //控制时间是否流动
    public static bool timeFlow;
}
