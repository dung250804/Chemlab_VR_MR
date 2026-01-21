using com.ethnicthv.chemlab.client.core.renderer.pass;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace com.ethnicthv.chemlab.client.core.renderer
{
    public class ChemicalCompoundRenderFeature : ScriptableRendererFeature
    {
        public Material outlineShader;
        public Material atomMaterial;
        public Material bondMaterial;
        
        [Space]
        public Color singleBondColor = Color.black;
        public Color doubleBondColor = Color.yellow;
        public Color tripleBondColor = Color.red;

        [Space(10)] public Mesh atomMesh;
        public Mesh oneBondMesh;
        public Mesh twoBondMesh;
        public Mesh threeBondMesh;

        private ChemicalCompoundPrePass _prePass;
        private ChemicalCompoundDepthPass _depthPass;
        private ChemicalCompoundNormalPass _normalPass;
        private ChemicalCompoundRenderPass _renderPass;
        private ChemicalCompoundOutlinePass _outlinePass;

        public override void Create()
        {
            _prePass = new ChemicalCompoundPrePass
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPrePasses
            };
            
            _depthPass = new ChemicalCompoundDepthPass(
                atomMaterial, atomMesh
            )
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPrePasses
            };
            
            _normalPass = new ChemicalCompoundNormalPass(
                atomMaterial, atomMesh
            )
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPrePasses
            };
            
            _renderPass = new ChemicalCompoundRenderPass(
                atomMaterial, bondMaterial, 
                singleBondColor, doubleBondColor, tripleBondColor,
                atomMesh, oneBondMesh, twoBondMesh, threeBondMesh
            )
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques
            };
            
            _outlinePass = new ChemicalCompoundOutlinePass
            {
                OutlineShader = outlineShader,
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            //Note: Check camera contain the tag
            if (!renderingData.cameraData.camera.CompareTag("CompoundCamera")) return;
            renderer.EnqueuePass(_prePass);
            renderer.EnqueuePass(_depthPass);
            renderer.EnqueuePass(_normalPass);
            renderer.EnqueuePass(_renderPass);
            renderer.EnqueuePass(_outlinePass);
        }
    }
}