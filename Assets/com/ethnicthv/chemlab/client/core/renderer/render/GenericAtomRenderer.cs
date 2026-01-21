using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.render;
using com.ethnicthv.chemlab.client.core.renderer.type;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.core.renderer.render
{
    public class GenericAtomRenderer : IRenderer<RenderAtomRenderable>
    {
        public void Render(RenderAtomRenderable atomModel, Stack<Matrix4x4> matricesStack, RenderState renderState)
        {
            var atoms = atomModel.Atoms;
            var atomsCount = atoms.Count;

            for (var i = 0; i < atomsCount; i++)
            {
                matricesStack.Push(atoms[i].GetModelMatrix());
            }
        }

        public void RenderGizmos(RenderAtomRenderable renderable)
        {
        }
    }
}