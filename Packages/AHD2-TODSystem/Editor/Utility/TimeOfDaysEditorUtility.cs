using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AHD2TimeOfDay
{
    public static class TimeOfDaysEditorUtility
    {
        /// <summary>
        /// 创建文件路径
        /// </summary>
        /// <param name="path"></param>
        public static void CreateDirectory(string path)
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
