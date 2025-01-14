﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AHD2TimeOfDay
{
    public class NewRelecttionProbe
    {
        [MenuItem("GameObject/AHD2TODSystem/CreateReflectionProbe", false, 2000)]
        public static void CreateNewTimeOfDaysController(MenuCommand menuCommand)
        {
            // 创建一个新的游戏对象
            GameObject ahd2ReflectionProbe = new GameObject("AHD2ReflectionProbe");

            // 确保撤销操作能够撤销这个创建动作
            Undo.RegisterCreatedObjectUndo(ahd2ReflectionProbe, "Create AHD2ReflectionProbe");

            // 将新创建的游戏对象设置为当前选中的对象的子对象
            GameObjectUtility.SetParentAndAlign(ahd2ReflectionProbe, menuCommand.context as GameObject);
            
            // 在新创建的游戏对象上添加指定的脚本组件
            ReflectionProbe scriptComponent = ahd2ReflectionProbe.AddComponent<ReflectionProbe>();
            ReflectorProbe newReflectorProbe = ahd2ReflectionProbe.AddComponent<ReflectorProbe>(); 
            //初始化（生成就烘焙一次）
            newReflectorProbe.bakeable = true;
            ReflectorProbe[] probes = new ReflectorProbe[1];
            probes[0] = newReflectorProbe;
            ReflectorProbeBaker.Bake(probes);
            // 选中新创建的游戏对象
            Selection.activeObject = ahd2ReflectionProbe;
        }
    }
}