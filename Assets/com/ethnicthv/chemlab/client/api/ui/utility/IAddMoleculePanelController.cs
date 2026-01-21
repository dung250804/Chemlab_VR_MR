using com.ethnicthv.chemlab.engine.molecule;

namespace com.ethnicthv.chemlab.client.api.ui.utility
{
    public interface IAddMoleculePanelController : ICloseablePanel, IOpenablePanel
    {
        public void SetupPanel(Molecule molecule);
    }
}