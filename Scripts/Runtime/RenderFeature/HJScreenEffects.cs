using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HJ.Rendering
{
    public class HJScreenEffects : ScriptableRendererFeature
    {
        [SerializeReference]
        public List<EffectFeature> Features = new()
        {
            new ScanlinesFeature(),
            new BloodDisortionFeature(),
            new EyeBlinkFeature(),
            new FearTentanclesFeature()
        };
        
        public override void Create()
        {
            Features.ForEach(feature => feature.OnCreate());
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            foreach (EffectFeature feature in Features)
            {
                ScriptableRenderPass pass = feature.OnGetRenderPass();
                if (pass != null && feature.Enabled) renderer.EnqueuePass(pass);
            }
        }
    }
}
