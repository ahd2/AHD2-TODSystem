using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AHD2TimeOfDay
{
    public partial class TimeOfDaysGenerator : EditorWindow
    {
        // 子界面的枚举类型，用于切换
        private enum SubWindowType
        {
            Window1, //生成窗口
            Window2, //新增材质窗口
            Window3 //烘焙环境反射图窗口
        }

        // 当前选中的子界面类型
        private SubWindowType currentSubWindow = SubWindowType.Window1;

        private TODGlobalParameters _todGlobalParameters;
        private List<TempTOD> todList = new List<TempTOD>();
        public List<TimeOfDay> timeOfDays = new List<TimeOfDay>(); //赋值nextTOD的列表(列表一定要初始化)

        //消息提示框提示的消息
        string message = "";
        MessageType messageType = MessageType.None;

        Vector2 scrollPos; //滚动条

        [MenuItem("Tools/TOD生成工具")]
        public static void ShowWindow()
        {
            GetWindow<TimeOfDaysGenerator>("TimeOfDaysGenerator");
        }

        //选择生成tod的文件分布类型
        private int _selectedIndex = 0;

        /// <summary>
        /// OnGUI是每帧调用的。
        /// </summary>
        void OnGUI()
        {
            // 顶部标签页
            GUILayout.BeginHorizontal();
            currentSubWindow =
                (SubWindowType)GUILayout.Toolbar((int)currentSubWindow, new string[] { "TOD生成", "关键帧材质生成" });
            GUILayout.EndHorizontal();

            // 根据选中的标签页显示不同的子界面
            switch (currentSubWindow)
            {
                case SubWindowType.Window1:
                    DrawWindow1();
                    break;
                case SubWindowType.Window2:
                    DrawWindow2();
                    break;
                case SubWindowType.Window3:
                    DrawWindow3();
                    break;
            }
        }

        public void CreateDirectory(string path)
        {
            // 检查路径是否已存在
            if (!Directory.Exists(path))
            {
                // 如果路径不存在，创建它
                Directory.CreateDirectory(path);
            }
        }
    }
}