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
        InitialLight();//光源需要拖入(unity自己会报错，不处理了。)
    }

    void Update()
    {
        if (todGlobalParameters.timeFlow)
        {
            //如果时间流动。
            todGlobalParameters.CurrentTime += Time.deltaTime * 5;
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
}
