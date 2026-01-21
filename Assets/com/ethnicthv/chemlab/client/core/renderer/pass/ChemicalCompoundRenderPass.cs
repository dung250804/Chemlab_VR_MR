using System.Collections.Generic;
using System.Runtime.InteropServices;
using com.ethnicthv.chemlab.client.model.bond;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace com.ethnicthv.chemlab.client.core.renderer.pass
{
    public class ChemicalCompoundRenderPass : ScriptableRenderPass
    {
        private readonly Material _atomMaterial;
        private readonly Material _bondMaterial;

        private readonly Color _singleBondColor;
        private readonly Color _doubleBondColor;
        private readonly Color _tripleBondColor;

        private readonly Mesh _atomMesh;
        private readonly Mesh _oneBondMesh;
        private readonly Mesh _twoBondMesh;
        private readonly Mesh _threeBondMesh;

        private static readonly int Color1 = Shader.PropertyToID("_Color");
        private static readonly int RenderData = Shader.PropertyToID("UnityInstancing_PerUnitData");
        private GraphicsBuffer _buffer;

        public ChemicalCompoundRenderPass(
            Material atomMaterial, Material bondMaterial,
            Color singleBondColor, Color doubleBondColor, Color tripleBondColor,
            Mesh atomMesh, Mesh oneBondMesh, Mesh twoBondMesh, Mesh threeBondMesh)
        {
            _atomMaterial = atomMaterial;
            _bondMaterial = bondMaterial;
            _singleBondColor = singleBondColor;
            _doubleBondColor = doubleBondColor;
            _tripleBondColor = tripleBondColor;
            _atomMesh = atomMesh;
            _oneBondMesh = oneBondMesh;
            _twoBondMesh = twoBondMesh;
            _threeBondMesh = threeBondMesh;
        }

        private class PassData
        {
            public Stack<Matrix4x4> MatricesStack0;
            public Stack<Matrix4x4> MatricesStack1;
            public Stack<Matrix4x4> MatricesStack2;
            public Stack<Matrix4x4> MatricesStack3;
            public Mesh AtomMesh;
            public Mesh OneBondMesh;
            public Mesh TwoBondMesh;
            public Mesh ThreeBondMesh;
            public Material AtomMaterial;
            public Material SingleBondMaterial;
            public Material DoubleBondMaterial;
            public Material TripleBondMaterial;
            public BufferHandle AtomBuffer;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (RenderProgram.RP == null) return;
            if (_bondMaterial == null || _atomMaterial == null) return;
            if (_atomMesh == null || _oneBondMesh == null || _twoBondMesh == null || _threeBondMesh == null) return;
            if (RenderProgram.RP.GetAtomCount() == 0) return;

            using (var builder = renderGraph
                       .AddRasterRenderPass<PassData>("ChemicalCompoundRenderPass", out var passData))
            {
                RenderProgram.RP.CheckModelMatrix();

                var singleBondMaterial = new Material(_bondMaterial);
                singleBondMaterial.SetColor(Color1, _singleBondColor);
                var doubleBondMaterial = new Material(_bondMaterial);
                doubleBondMaterial.SetColor(Color1, _doubleBondColor);
                var tripleBondMaterial = new Material(_bondMaterial);
                tripleBondMaterial.SetColor(Color1, _tripleBondColor);

                passData.AtomMaterial = _atomMaterial;
                passData.SingleBondMaterial = singleBondMaterial;
                passData.DoubleBondMaterial = doubleBondMaterial;
                passData.TripleBondMaterial = tripleBondMaterial;
                passData.AtomMesh = _atomMesh;
                passData.OneBondMesh = _oneBondMesh;
                passData.TwoBondMesh = _twoBondMesh;
                passData.ThreeBondMesh = _threeBondMesh;
                passData.MatricesStack0 = new Stack<Matrix4x4>();
                passData.MatricesStack1 = new Stack<Matrix4x4>();
                passData.MatricesStack2 = new Stack<Matrix4x4>();
                passData.MatricesStack3 = new Stack<Matrix4x4>();

                var volumeComponent =
                    VolumeManager.instance.stack.GetComponent<ChemicalCompoundVolume>();

                //Note: setting the bond radius
                BondModel.BondRadius = volumeComponent.bondRadius.value;

                RenderProgram.RP.RenderAtom(passData.MatricesStack0, out var atomRenderData);
                RenderProgram.RP.RenderBond(
                    passData.MatricesStack1, passData.MatricesStack2, passData.MatricesStack3);

                builder.AllowPassCulling(false);

                var resourceData = frameData.Get<UniversalResourceData>();

                passData.AtomBuffer = builder.UseBuffer(renderGraph.ImportBuffer(atomRenderData));

                builder.AllowGlobalStateModification(true);

                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }

        private static void ExecutePass(PassData data, RasterGraphContext context)
        {
            if (RenderProgram.RP == null) return;

            var cmd = context.cmd;

            data.AtomMaterial.SetConstantBuffer(RenderData, data.AtomBuffer, 0,
                Marshal.SizeOf<AtomRenderData>() * RenderProgram.RP.GetAtomCount());

            cmd.DrawMeshInstanced(data.AtomMesh, 0, data.AtomMaterial, 0, data.MatricesStack0.ToArray());

            // Debug.Log("Drawing bonds : one = " + data.MatricesStack1.Count + ", two = " + data.MatricesStack2.Count + ", three = " + data.MatricesStack3.Count + ".");
            cmd.DrawMeshInstanced(data.OneBondMesh, 0, data.SingleBondMaterial, 0, data.MatricesStack1.ToArray());
            cmd.DrawMeshInstanced(data.TwoBondMesh, 0, data.DoubleBondMaterial, 0, data.MatricesStack2.ToArray());
            cmd.DrawMeshInstanced(data.ThreeBondMesh, 0, data.TripleBondMaterial, 0, data.MatricesStack3.ToArray());
        }
    }
}