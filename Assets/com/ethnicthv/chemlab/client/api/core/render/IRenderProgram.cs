using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.element;
using com.ethnicthv.chemlab.engine.api.molecule.formula;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.api.core.render
{
    public interface IRenderProgram
    {
        void ClearRenderEntity();
        void UnregisterRenderEntity(IFormula getFormula);
        void RegisterRenderEntity(IFormula getFormula, Vector3 zero);
        bool HasAnyRenderEntity();
        (Vector3, Vector3)  GetBound(int i);
        IReadOnlyDictionary<Element, Color> GetElementColors();
    }
}