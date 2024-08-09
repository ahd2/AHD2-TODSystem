using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TODController : MonoBehaviour
{
    public TODGlobalParameters todGlobalParameters;//未拖入也会报错，不处理了
    public Light MainLight;//主光源
    private Vector3 starquat;//计算光源旋转用
    private Vector3 endquat;//计算光源旋转用
    void Start()
    {
        todGlobalParameters.CurrentTime = 6;//从6点开始
        foreach (var tod in todGlobalParameters.timeOfDays)
        {
            tod.Initialize();
        }
        FixTimeOfDay();//调整时间段符合当前时间
        InitialLight();//光源需要拖入(unity自己会报错，不处理了。)
    }

    void Update()
    {
        if (todGlobalParameters.timeFlow)
        {
            //如果时间流动。
            todGlobalParameters.CurrentTime += Time.deltaTime * 5;
            todGlobalParameters.todTime += Time.deltaTime * 5;
            UpdateTimeOfDay();
            LerpProperties();
        }
        else
        {
            FixTimeOfDay();
            LerpProperties();
        }
        RotateLight();
    }
    /// <summary>
    /// 初始化光源相关数据
    /// </summary>
    void InitialLight()
    {
        starquat = new Vector3(-90,0,0);//对应0点
        endquat = new Vector3(270,0,0);//对应24点
    }
    /// <summary>
    /// 根据当前时间更新主光源旋转
    /// </summary>
    void RotateLight()
    {
        //欧拉角插值
        Vector3 quat = Vector3.Lerp(starquat,endquat,todGlobalParameters.CurrentTime/24);
        MainLight.transform.rotation = Quaternion.Euler(quat);
    }
    /// <summary>
    /// 修正时刻段，让当前时刻段对应上当前时间
    /// </summary>
    void FixTimeOfDay()
    {
        //遍历一天时刻
        foreach (TimeOfDay timeOfDay in todGlobalParameters.timeOfDays)
        {
            //如果当前时刻和下个时刻中间隔了24点
            if (timeOfDay.isCross24)
            {
                //如果时间在当前时刻中
                if (todGlobalParameters.CurrentTime <= timeOfDay.endTime||todGlobalParameters.CurrentTime > timeOfDay.startTime)
                {
                    //切换至该时刻
                    todGlobalParameters.currentTimeOfDay = timeOfDay;
                    //更新当前时刻已经经过的时间
                    if (todGlobalParameters.CurrentTime - timeOfDay.startTime > 0)
                    {
                        todGlobalParameters.todTime = todGlobalParameters.CurrentTime - timeOfDay.startTime;
                    }
                    else
                    {
                        todGlobalParameters.todTime = 24 - timeOfDay.startTime + todGlobalParameters.CurrentTime;
                    }
                    return;
                }
            }
            else
            {
                //如果时间在当前时刻中
                if (todGlobalParameters.CurrentTime < timeOfDay.endTime &&todGlobalParameters.CurrentTime >= timeOfDay.startTime)
                {
                    //切换至该时刻
                    todGlobalParameters.currentTimeOfDay = timeOfDay;
                    //更新当前时刻已经经过的时间
                    todGlobalParameters.todTime = todGlobalParameters.CurrentTime - timeOfDay.startTime;
                    return;
                }
            }
        }
    }
    /// <summary>
    /// 混合关键帧之间的材质或其他数据
    /// </summary>
    void LerpProperties()
    {
        for (int i = 0; i < todGlobalParameters.materials.Length; i++)
        {
            todGlobalParameters.materials[i].Lerp(todGlobalParameters.currentTimeOfDay.materials[i],todGlobalParameters.currentTimeOfDay.nextTOD.materials[i],todGlobalParameters.todTime/todGlobalParameters.currentTimeOfDay.duration);
            //插值光源色（）
        }
    }
    /// <summary>
    /// 更新当前时刻,和fix的区别在于，fix消耗更大（遍历所有tod），在时间流动的时候，我们可以确保tod是线性流动，所以可以用这个函数。
    /// </summary>
    private void UpdateTimeOfDay()
    {
        //如果当前时刻和下个时刻中间隔了24点
        if (todGlobalParameters.currentTimeOfDay.isCross24)
        {
            //如果时间不在当前时刻中
            if (todGlobalParameters.CurrentTime >= todGlobalParameters.currentTimeOfDay.endTime&&todGlobalParameters.CurrentTime < todGlobalParameters.currentTimeOfDay.startTime)
            {
                //进入下一个时刻
                todGlobalParameters.currentTimeOfDay = todGlobalParameters.currentTimeOfDay.nextTOD;
                //把当前时刻已经经过的时间归零
                todGlobalParameters.todTime = 0;
            }
        }
        else
        {
            if (todGlobalParameters.CurrentTime >= todGlobalParameters.currentTimeOfDay.endTime)
            {
                //进入下一个时刻
                todGlobalParameters.currentTimeOfDay = todGlobalParameters.currentTimeOfDay.nextTOD;
                //把当前时刻已经经过的时间归零
                todGlobalParameters.todTime = 0;
            }
        }
    }
}
