using System;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.engine.api.mixture;

namespace com.ethnicthv.chemlab.client.api.ui.contents
{
    public interface IContentPanelController : IOpenablePanel, ICloseablePanel
    {
        public void SetupMixtureToDisplay(IMixtureContainer mixtureContainer);
    }
}