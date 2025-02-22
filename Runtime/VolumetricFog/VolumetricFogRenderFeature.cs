using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AHD2TimeOfDay
{
    public class VolumetricFogRendererFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class VolumetricFogSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingShadows;
            //计算密度和光照的CS
            public ComputeShader densityAndLightingComputeShader;//后续改为直接通过路径加载
        }

        private DensityAndLightingPass _densityAndLightingPassPass;
        public VolumetricFogSettings volumetricFogSettings = new VolumetricFogSettings();

        public override void Create()
        {
            _densityAndLightingPassPass = new DensityAndLightingPass(volumetricFogSettings);
        }

        /// <summary>
        /// 每帧调用
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="renderingData"></param>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_densityAndLightingPassPass);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _densityAndLightingPassPass.Dispose();
        }
    }
}

