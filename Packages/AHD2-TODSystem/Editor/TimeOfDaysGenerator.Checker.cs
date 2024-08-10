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
        if (_todGlobalParameters)
        {
            if (message == "全局参数配置为空")//拖入全局参数后，把配置为空的消息清空
            {
                message = "";
                messageType = MessageType.Info;
            }
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
            //如果全局参数类没问题，拿到它的todlist
            todList = _todGlobalParameters.todFrameList;
            EditorUtility.SetDirty(_todGlobalParameters);//要用这个标记so为已修改。unity才能把改动保存
        }
        else
        {
            //如果全局参数未拖入
            todList = new List<TempTOD>();//切换到新的空关键帧list
            
            message = "全局参数配置为空";
            messageType = MessageType.Error;
            //GUIUtility.ExitGUI();//提前结束绘制，不加这个报错不匹配
            return; // 避免在遍历过程中修改列表
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

    private void CheckMat()
    {
        //检测要新增的插值材质，和全局参数内已有材质是否有重复的。
        HashSet<Material> mats = new HashSet<Material>();
        foreach (var mat in _todGlobalParameters.materials)
        {
            if (!mats.Add(mat))
            {
                message = "全局参数类内有材质重复！";
                messageType = MessageType.Error;
                GUIUtility.ExitGUI();//提前结束绘制，不加这个报错不匹配
                return; // 避免在遍历过程中修改列表
            }
        }
        foreach (var mat in extraMats)
        {
            if (!mat)
            {
                message = "要添加的材质中有空材质";
                messageType = MessageType.Error;
                GUIUtility.ExitGUI();//提前结束绘制，不加这个报错不匹配
                return; // 避免在遍历过程中修改列表
            }
            if (!mats.Add(mat))
            {
                message = "要添加的材质中有重复材质或与已有材质重复。";
                messageType = MessageType.Error;
                GUIUtility.ExitGUI();//提前结束绘制，不加这个报错不匹配
                return; // 避免在遍历过程中修改列表
            }
        }
    }
}
