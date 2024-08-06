using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TODController : MonoBehaviour
{
    [SerializeField,Range(0f,24f),Tooltip("时间")]
    private float _currentTime;

    [SerializeField, Tooltip("时间流动")] public bool _timeFlow;
    public Light MainLight;//主光源
    private float TempTime;//计算光源旋转用
    private Vector3 starquat;//计算光源旋转用
    private Vector3 endquat;//计算光源旋转用
    void Start()
    {
        InitialLight();
    }

    void Update()
    {
        TODGlobalParameters.timeFlow = _timeFlow;
        if (TODGlobalParameters.timeFlow)
        {
            //如果时间流动。
            TODGlobalParameters.CurrentTime += Time.deltaTime * 5;
            _currentTime = TODGlobalParameters.CurrentTime;
        }
        else
        {
            //不流动则由滑杆控制。
            TODGlobalParameters.CurrentTime = _currentTime;
        }

        RotateLight();
    }
    /// <summary>
    /// 初始化光源相关数据
    /// </summary>
    void InitialLight()
    {
        starquat = new Vector3(0,0,0);
        endquat = new Vector3(360,0,0);
    }
    /// <summary>
    /// 根据当前时间更新主光源旋转
    /// </summary>
    void RotateLight()
    {
        //映射currenttime
        TempTime=(TODGlobalParameters.CurrentTime + 18) % 24;
        //欧拉角插值
        Vector3 quat = Vector3.Lerp(starquat,endquat,TempTime/24);
        MainLight.transform.rotation = Quaternion.Euler(quat);
    }
}
