using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.element;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.api.ui.compound
{
    public interface IElementListPanelController : IOpenablePanel, ICloseablePanel
    {
        public void SetElementList(IReadOnlyDictionary<Element, Color> elementList);
    }
}