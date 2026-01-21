using System.Collections.Generic;
using com.ethnicthv.chemlab.client.core.renderer.context;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace com.ethnicthv.chemlab.client.core.renderer.pass
{
    public class ChemicalCompoundNormalPass : ScriptableRenderPass
    {
        public static readonly int NormalTextureID = Shader.PropertyToID("_CustomNormalTexture");
        
        private readonly Material _atomMaterial;
        private readonly Mesh _atomMesh;

        public ChemicalCompoundNormalPass(Material atomMaterial, Mesh atomMesh)
        {
            _atomMaterial = atomMaterial;
            _atomMesh = atomMesh;
        }

        private class PassData
        {
            public Stack<Matrix4x4> MatricesStack;
            public Mesh AtomMesh;
            public Material AtomMaterial;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph
                       .AddRasterRenderPass<PassData>("ChemicalCompoundNormalPass", out var passData))
            {
                if (RenderProgram.RP != null)
                {
                    RenderProgram.RP.CheckModelMatrix();
                    passData.AtomMaterial = _atomMaterial;
                    passData.AtomMesh = _atomMesh;
                    passData.MatricesStack = new Stack<Matrix4x4>();

                    RenderProgram.RP.RenderAtom(passData.MatricesStack, RenderState.Depth);

                    builder.AllowPassCulling(false);
                }

                var customData = frameData.Get<CustomResource>();
                
                builder.SetRenderAttachment(customData.NormalTexture, 0);
                builder.SetRenderAttachmentDepth(customData.TempDepthAttachment);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
                builder.SetGlobalTextureAfterPass(customData.NormalTexture, NormalTextureID);
            }
        }

        private static void ExecutePass(PassData data, RasterGraphContext context)
        {
            if (RenderProgram.Instance == null) return;

            var cmd = context.cmd;
            
            if (data.AtomMaterial == null || data.AtomMesh == null) return;

            cmd.DrawMeshInstanced(data.AtomMesh, 0, data.AtomMaterial, 2, data.MatricesStack.ToArray());
        }
    }
}