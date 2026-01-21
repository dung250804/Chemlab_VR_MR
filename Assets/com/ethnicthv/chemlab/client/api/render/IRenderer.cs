using System.Collections.Generic;
using com.ethnicthv.chemlab.client.core.renderer;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.api.render
{
    public interface IRenderer<T> where T : IRenderable
    {
        public void Render(T renderable, Stack<Matrix4x4> matricesStack, RenderState renderState);
        
        public void RenderGizmos(T renderable);
    }
}