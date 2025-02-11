using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif
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

        internal static TimeOfDayData GetDefaultTimeOfDayData()
        {
            var path = System.IO.Path.Combine("Packages/com.ahd2.tod-system", "Runtime/Data/TimeOfDayData.asset");
            return AssetDatabase.LoadAssetAtPath<TimeOfDayData>(path);
        }

#endif

        [Serializable, ReloadGroup]
        public sealed class ShaderResources
        {
            //反射探针
            [Reload("Shaders/ReflectionProbe/SphericalHarmonicsComputeShader.compute")]
            public ComputeShader sphericalHarmonicsCS;
            
            [Reload("Shaders/ReflectionProbe/Relight.compute")]
            public ComputeShader relightCS;
            
            [Reload("Shaders/ReflectionProbe/Mirror.shader")]
            public Shader mirrorPS;
        }

        [Serializable, ReloadGroup]
        public sealed class TextureResources
        {
            
        }
        
        [Serializable, ReloadGroup]
        public sealed class MeshResources
        {
            [Reload("Meshes/ReflectionProbe/skybox.fbx")]
            public Mesh skyboxmesh;
        }

        public ShaderResources shaders;

        public TextureResources textures;

        public MeshResources meshes;
    }
}
