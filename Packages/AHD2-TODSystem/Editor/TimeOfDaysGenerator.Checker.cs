using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 此分布类写一些逻辑检测函数
/// </summary>
public partial class TimeOfDaysGenerator : EditorWindow
{
    //检测全局参数
    private void CheckGlobalParameters()
    {
        if (_todGlobalParameters != null)
        {
            //检测全局参数材质数组是否有空的
            for (int i = 0; i < _todGlobalParameters.materials.Length; i++)
            {
                if (_todGlobalParameters.materials[i] == null)
                {
                    message = "全局参数材质数组有缺失！";
                    messageType = MessageType.Error;
                    return; // 避免在遍历过程中修改列表
                }
            }
        }
    }
    //检测todlist内所有关键帧名字是否合规
    private void CheckName()
    {
        //是否有重名的
        HashSet<string> names = new HashSet<string>();
        if (todList.Count == 0)
        {
            message = "关键帧为空。";
            messageType = MessageType.Error;
            GUIUtility.ExitGUI();//提前结束绘制，不加这个报错不匹配
            return; // 避免在遍历过程中修改列表
        }
        //是否有名字为空的
        foreach (var tod in todList)
        {
            if (tod.name == null)
            {
                message = "有关键帧名字未填入";
                messageType = MessageType.Error;
                GUIUtility.ExitGUI();//提前结束绘制，不加这个报错不匹配
                return; // 避免在遍历过程中修改列表
            }
            if (!names.Add(tod.name))
            {
                message = "有关键帧名字重复";
                messageType = MessageType.Error;
                GUIUtility.ExitGUI();//提前结束绘制，不加这个报错不匹配
                return; // 避免在遍历过程中修改列表
            }
        }
        //检测全局参数是否为空
        if (_todGlobalParameters == null)
        {
            message = "全局参数配置为空";
            messageType = MessageType.Error;
            GUIUtility.ExitGUI();//提前结束绘制，不加这个报错不匹配
            return; // 避免在遍历过程中修改列表
        }
    }

    private void CheckTime()
    {
        todList.Sort((x, y) => x.Time.CompareTo(y.Time));//将列表元素升序排序
        //是否有重名的
        HashSet<float> times = new HashSet<float>();
        foreach (var tod in todList)
        {
            if (!times.Add(tod.Time))
            {
                message = "有关键帧时间值重复";
                messageType = MessageType.Error;
                GUIUtility.ExitGUI();//提前结束绘制，不加这个报错不匹配
                return; // 避免在遍历过程中修改列表
            }
        }
    }
}
