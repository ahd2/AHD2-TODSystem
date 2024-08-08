using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理全局参数的类
/// </summary>
[CreateAssetMenu(fileName = "TODGlobalParameters", menuName = "AHD2TODSystem/GlobalParameters", order = 0)]
public class TODGlobalParameters : ScriptableObject
{
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
    public bool timeFlow; //私有的加_，公开的不加
    public Material[] materials;//插值材质数组，场景中使用的材质放这里。
    public TimeOfDay[] timeOfDays;//tod数组
    public TimeOfDay currentTimeOfDay;
    public float todTime;//当前tod已经经过的时间
}
