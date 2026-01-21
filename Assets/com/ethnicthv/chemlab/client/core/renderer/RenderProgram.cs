using System.Collections.Generic;
using System.Runtime.InteropServices;
using com.ethnicthv.chemlab.client.api.core;
using com.ethnicthv.chemlab.client.api.core.render;
using com.ethnicthv.chemlab.client.core.renderer.render;
using com.ethnicthv.chemlab.engine.api.element;
using com.ethnicthv.chemlab.engine.api.molecule.formula;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.core.renderer
{
    public struct AtomRenderData
    {
        public float4 Color;
    }
    
    [ExecuteInEditMode]
    public class RenderProgram : MonoBehaviour, IRenderProgram
    {
        // <-- program properties -->
        public static IRenderProgram Instance { get; private set; }
        public static RenderProgram RP { get; private set; }

        private readonly RenderProcessor _renderProcessor = new();
        private readonly AtomColorAssigner _colorAssigner = new();
        
        private NativeArray<AtomRenderData> _atomRenderData;
        private GraphicsBuffer _atomRenderDataBuffer;
        // <-- end of program properties -->

        // <-- renderers -->
        private readonly BondRenderer _bondRenderer = new();
        private readonly GenericAtomRenderer _atomRenderer = new();
        // <-- end of renderers -->

        // <-- state -->
        private bool _isDirty;
        // <-- end of state -->
        
        // <-- shader properties -->
        private static readonly int Color = Shader.PropertyToID("_AtomColor");
        // <-- end of shader properties -->

        private void Awake()
        {
            Debug.Log("RenderProgram Awake");
            if (Instance == null)
            {
                Instance = this;
                RP = this;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                RP = this;
            }
            
            _atomRenderData.Dispose();
            _atomRenderDataBuffer?.Dispose();
        }
        
        public bool HasAnyRenderEntity()
        {
            return _renderProcessor.HasAnyRenderEntity();
        }
        
        public (Vector3, Vector3) GetBound(int index)
        {
            return _renderProcessor.GetBound(index);
        }
        
        public void ClearRenderEntity()
        {
            _renderProcessor.Clear();
            _isDirty = true;
        }

        public void UnregisterRenderEntity(IFormula formula)
        {
            _renderProcessor.RemoveFormula(formula);
            _isDirty = true;
        }

        public void RegisterRenderEntity(IFormula formula, Vector3 offset)
        {
            _renderProcessor.AddFormula(formula, offset);
            _isDirty = true;
        }
        
        public int GetAtomCount()
        {
            return _renderProcessor.GetAtomCount();
        }
        
        public IReadOnlyDictionary<Element,Color> GetElementColors()
        {
            return _colorAssigner.GetElementColors();
        }

        public void RenderAtom(Stack<Matrix4x4> matricesStack, out GraphicsBuffer atomRenderData,
            RenderState state = RenderState.Opaque)
        {
            var i = GetAtomCount();
            _renderProcessor.ForeachElement((element, renderable) =>
            {
                var color = _colorAssigner.GetColorForElement(element);
                
                for (var t = 0; t <renderable.Atoms.Count; t++)
                {
                    i--;
                    _atomRenderData[i] = new AtomRenderData
                    {
                        Color = new float4(color.r, color.g, color.b, color.a)
                    };
                }
                
                _atomRenderer.Render(renderable, matricesStack, state);
            });
            
            _atomRenderDataBuffer.SetData(_atomRenderData);
            atomRenderData = _atomRenderDataBuffer;
        }
        
        public void RenderAtom(Stack<Matrix4x4> matricesStack,
            RenderState state = RenderState.Opaque)
        {
            _renderProcessor.ForeachElement((_, renderable) =>
            {
                _atomRenderer.Render(renderable, matricesStack, state);
            });
        }
        
        public void RenderBond(
            Stack<Matrix4x4> matricesStack1, 
            Stack<Matrix4x4> matricesStack2, 
            Stack<Matrix4x4> matricesStack3, 
            RenderState state = RenderState.Opaque)
        {
            _renderProcessor.ForeachSingleBond(model =>
            {
                _bondRenderer.Render(model, matricesStack1, state);
            });
            
            _renderProcessor.ForeachDoubleBond(model =>
            {
                _bondRenderer.Render(model, matricesStack2, state);
            });
            
            _renderProcessor.ForeachTripleBond(model =>
            {
                _bondRenderer.Render(model, matricesStack3, state);
            });
        }

        public void CheckModelMatrix()
        {
            if (!_isDirty) return;
            // Note: Dispose the previous atom render data
            _atomRenderData.Dispose();
            _atomRenderDataBuffer?.Dispose();

            // Note: Refresh the render processor
            _renderProcessor.Refresh();
            _renderProcessor.Recalculate();
            _colorAssigner.Clear();
            _atomRenderData = new NativeArray<AtomRenderData>(_renderProcessor.GetAtomCount(), Allocator.Persistent);
            _atomRenderDataBuffer = _atomRenderData.Length != 0 ? new GraphicsBuffer(GraphicsBuffer.Target.Constant, _atomRenderData.Length, Marshal.SizeOf<AtomRenderData>()) : null;
            _isDirty = false;
        }
    }

    public enum RenderState
    {
        Opaque,
        Depth
    }
}