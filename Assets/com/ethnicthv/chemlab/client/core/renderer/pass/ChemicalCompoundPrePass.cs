using com.ethnicthv.chemlab.client.core.renderer.context;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace com.ethnicthv.chemlab.client.core.renderer.pass
{
    public class ChemicalCompoundPrePass : ScriptableRenderPass
    {
        private class PassData
        {
            public TextureHandle DepthTexture;
            
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph
                       .AddRasterRenderPass<PassData>("ChemicalCompoundPrePass", out var passData))
            {
                var textureProperties0 =
                    new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
                var depthTexture =
                    UniversalRenderer.CreateRenderGraphTexture(renderGraph, textureProperties0, "_CustomDepthTexture",
                        false);
                
                var textureProperties1 =
                    new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
                var normalTexture =
                    UniversalRenderer.CreateRenderGraphTexture(renderGraph, textureProperties1, "_CustomNormalTexture",
                        false);
                
                var textureProperties2 =
                    new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 32);
                var tempDepthAttachment =
                    UniversalRenderer.CreateRenderGraphTexture(renderGraph, textureProperties2, "_CustomTempDepthAttachment",
                        false);
                
                var customData = frameData.GetOrCreate<CustomResource>();
                customData.DepthTexture = depthTexture;
                customData.NormalTexture = normalTexture;
                customData.TempDepthAttachment = tempDepthAttachment;

                passData.DepthTexture = depthTexture;

                builder.SetRenderAttachment(depthTexture, 0);
                builder.SetRenderAttachment(normalTexture, 1);

                builder.AllowPassCulling(false);
                builder.SetRenderFunc((PassData _, RasterGraphContext context) => ExecutePass(context));
            }
        }

        private static void ExecutePass(RasterGraphContext context)
        {
            context.cmd.ClearRenderTarget(true, true, Color.clear);
        }
    }
}