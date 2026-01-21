using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.render;
using com.ethnicthv.chemlab.client.model;

namespace com.ethnicthv.chemlab.client.core.renderer.type
{
    public class RenderAtomRenderable : IRenderable
    {
        public readonly IReadOnlyList<GenericAtomModel> Atoms;
        
        public RenderAtomRenderable(IReadOnlyList<GenericAtomModel> atoms)
        {
            Atoms = atoms;
        }
    }
}