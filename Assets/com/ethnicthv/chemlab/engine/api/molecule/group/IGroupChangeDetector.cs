using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.molecule;

namespace com.ethnicthv.chemlab.engine.api.molecule.group
{
    public interface IGroupDetector
    {
        public bool ShouldApplyGroup(DetectingContext context, out IFunctionalGroup[] anchorAtom);
        public MoleculeGroup GetGroup();
    }
}