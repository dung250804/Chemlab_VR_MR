using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.api.ui.options
{
    public interface IOptionsPanelController : IOpenablePanel, ICloseablePanel
    {
        public void SetupOptions(IReadOnlyList<(string, Action)> options, Vector2 position);
    }
}