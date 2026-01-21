using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace com.ethnicthv.chemlab.client.core.renderer.context
{
    public class CustomResource : ContextItem
    {
        public TextureHandle DepthTexture;
        public TextureHandle NormalTexture;
        public TextureHandle DepthNormalsTexture;
        public TextureHandle TempDepthAttachment;
        
        public override void Reset()
        {
            DepthTexture = TextureHandle.nullHandle;
            NormalTexture = TextureHandle.nullHandle;
            DepthNormalsTexture = TextureHandle.nullHandle;
            TempDepthAttachment = TextureHandle.nullHandle;
        }
    }
}