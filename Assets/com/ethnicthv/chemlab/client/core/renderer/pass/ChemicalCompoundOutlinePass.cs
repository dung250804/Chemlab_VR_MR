using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace com.ethnicthv.chemlab.client.core.renderer.pass
{
    public class ChemicalCompoundOutlinePass : ScriptableRenderPass
    {
        public Material OutlineShader;
        
        static Vector4 _scaleBias = new Vector4(1f, 1f, 0f, 0f);

        private static readonly int ClipToViewID = Shader.PropertyToID("_ClipToView");
        private static readonly int ViewFrameID = Shader.PropertyToID("_ViewFrame");
        
        public class PassData
        {
            public Material OutlineMaterial;
            public TextureHandle Source;
            public Camera Camera;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (OutlineShader == null) return;
            
            var resourceData = frameData.Get<UniversalResourceData>();
            
            var source = resourceData.activeColorTexture;

            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = $"CameraColor-{passName}";
            destinationDesc.clearBuffer = false;
            var destination = renderGraph.CreateTexture(destinationDesc); 
            
            renderGraph.AddCopyPass(source, destination, passName: "Copy To or From Temp Texture");

            using (var builder = renderGraph.AddRasterRenderPass("ChemicalCompoundOutlinePass", out PassData passData))
            {
                passData.OutlineMaterial = OutlineShader;
                passData.Source = destination;
                passData.Camera = frameData.Get<UniversalCameraData>().camera;
                
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.UseGlobalTexture(ChemicalCompoundDepthPass.DepthTextureID);
                builder.UseGlobalTexture(ChemicalCompoundNormalPass.NormalTextureID);
                builder.UseTexture(destination);
                
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
        
        private static void ExecutePass(PassData passData, RasterGraphContext context)
        {
            if (passData.OutlineMaterial)
            {
                var clipToView = GL.GetGPUProjectionMatrix(
                    passData.Camera.projectionMatrix, true).inverse;
                
                passData.OutlineMaterial.SetMatrix(ClipToViewID, clipToView);
                passData.OutlineMaterial.SetTexture(ViewFrameID, passData.Source);
                
                Blitter.BlitTexture(context.cmd, passData.Source, _scaleBias, passData.OutlineMaterial, 0);
            }
            else
            {
                Blitter.BlitTexture(context.cmd, passData.Source, _scaleBias, 0, false);
            }
        }
    }
}