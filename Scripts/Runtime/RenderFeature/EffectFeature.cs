using System;
using UnityEngine.Rendering.Universal;

namespace HJ.Rendering
{
    [Serializable]
    public abstract class EffectFeature
    {
        public ScriptableRenderPass RenderPass;
        public bool Enabled = true;
        
        public abstract string Name { get; }
        public abstract void OnCreate();
        
        public virtual ScriptableRenderPass OnGetRenderPass()
        {
            if (RenderPass == null)
                return null;
            
            RenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
            return RenderPass;
        }
    }
}
