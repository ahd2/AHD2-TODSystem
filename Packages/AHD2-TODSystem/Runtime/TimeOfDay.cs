using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 单个时刻的父类。子类为一天中的单个时刻
/// </summary>
[CreateAssetMenu(fileName = "TOD", menuName = "AHD2TODSystem/TimeOfDay", order = 5)]
public class TimeOfDay : ScriptableObject
{
    public TimeOfDay nextTOD;//下一个时刻
    public float startTime;//当前时刻开始时间
    public float endTime;//当前时刻结束时间（即下一时刻开始时间）(时间范围为[0,24)，即24点统一用0点)
    [HideInInspector]public bool isCross24;//这个时刻和下个时刻中间是否越过24点
    [HideInInspector]public float duration;//这个tod会持续的时间
    public Material[] materials;
    
    public void Initialize()
    {
        isCross24 = startTime > endTime;//用代码判断它会不会越过24
        //Debug.Log(IsCrossing24);
        if (isCross24)
        {
            duration = 24 - (startTime - endTime);
        }
        else
        {
            duration = endTime - startTime;
        }
    }
}
