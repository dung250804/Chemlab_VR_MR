using com.ethnicthv.chemlab.engine.api.element;

namespace com.ethnicthv.chemlab.client.api.ui.element
{
    public interface IElementPanelController : IOpenablePanel, ICloseablePanel
    {
        public void SetupPanel(Element element);
    }
}