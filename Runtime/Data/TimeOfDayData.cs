using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AHD2TimeOfDay
{
    [Serializable]
    public class TimeOfDayData : ScriptableObject
    {
#if UNITY_EDITOR
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
        internal class CreateTimeOfDayDataAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var instance = CreateInstance<TimeOfDayData>();
                AssetDatabase.CreateAsset(instance, pathName);
                ResourceReloader.ReloadAllNullIn(instance, "Packages/com.ahd2.tod-system");
                Selection.activeObject = instance;
            }
        }

        [MenuItem("Assets/Create/AHD2TODSystem/TimeOfDay Data", priority = CoreUtils.Sections.section5 + CoreUtils.Priorities.assetsCreateRenderingMenuPriority)]
        static void CreatePostProcessData()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateTimeOfDayDataAsset>(), "TimeOfDayData.asset", null, null);
        }

        internal static TimeOfDayData GetDefaultPostProcessData()
        {
            var path = System.IO.Path.Combine(UniversalRenderPipelineAsset.packagePath, "Runtime/Data/TimeOfDayData.asset");
            return AssetDatabase.LoadAssetAtPath<TimeOfDayData>(path);
        }

#endif

        [Serializable, ReloadGroup]
        public sealed class ShaderResources
        {
            
        }

        [Serializable, ReloadGroup]
        public sealed class TextureResources
        {
            
        }
        
        [Serializable, ReloadGroup]
        public sealed class MeshResources
        {
            
        }

        public ShaderResources shaders;

        public TextureResources textures;

        public MeshResources meshes;
    }
}
