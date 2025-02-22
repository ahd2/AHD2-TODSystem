using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AHD2TimeOfDay
{
    public class ScatterPass : ScriptableRenderPass
    {
        //分为两个个pass，计算每个体积纹素的密度和光照（吸收+内散射），计算总的透射率和内散射
        private ProfilingSampler scatterSampler = new ProfilingSampler("Scatter Pass");
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            throw new System.NotImplementedException();
        }

        public void Setup()
        {
            throw new System.NotImplementedException();
        }
    }
}