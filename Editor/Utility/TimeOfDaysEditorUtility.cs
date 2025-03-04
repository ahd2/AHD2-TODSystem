using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
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
        /// <summary>
        /// 复制完整目录结构及所有资源（含子目录）
        /// </summary>
        /// <param name="sourcePath">源目录路径（如：Assets/Source）</param>
        /// <param name="targetPath">目标目录路径（如：Assets/Target）</param>
        public static void CopyDirectory(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(sourcePath))
            {
                Debug.LogError($"无法复制目录：源路径不存在 {sourcePath}");
                return;
            }

            CreateDirectory(targetPath); // 确保目标目录存在

            // 复制所有文件
            foreach (string file in Directory.GetFiles(sourcePath))
            {
                if (file.EndsWith(".meta")) continue;

                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(targetPath, fileName);
                
                try
                {
                    AssetDatabase.CopyAsset(file, destFile);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"复制失败：{file} -> {destFile}\n{ e}");
                }
            }

            // 递归复制子目录
            foreach (string subDir in Directory.GetDirectories(sourcePath))
            {
                string dirName = Path.GetFileName(subDir);
                string destSubDir = Path.Combine(targetPath, dirName);
                CopyDirectory(subDir, destSubDir); // 递归调用
            }

            AssetDatabase.Refresh();
            Debug.Log($"目录复制完成：{sourcePath} -> {targetPath}");
        }
    }
}
