using com.ethnicthv.chemlab.engine.api.molecule;

namespace com.ethnicthv.chemlab.client.api.ui.compound
{
    public interface ICompoundPanelController: ICloseablePanel, IOpenablePanel
    {
        public void SetDisplayedMolecule(IMolecule molecule);
    }
}