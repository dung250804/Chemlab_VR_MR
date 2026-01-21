using com.ethnicthv.chemlab.client.api.core.game;

namespace com.ethnicthv.chemlab.client.api.ui.utility
{
    public interface INamingPanelController : ICloseablePanel, IOpenablePanel
    {
        public void SetupPanel(IHasName target);
        public void Rename();
    }
}