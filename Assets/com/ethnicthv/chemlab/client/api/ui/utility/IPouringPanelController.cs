using com.ethnicthv.chemlab.client.api.core.game;

namespace com.ethnicthv.chemlab.client.api.ui.utility
{
    public interface IPouringPanelController : IOpenablePanel , ICloseablePanel
    {
        public void SetupPanel(IMixtureContainer original, IMixtureContainer target);
    }
}