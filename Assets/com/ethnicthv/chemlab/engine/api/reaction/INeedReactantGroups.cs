using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.molecule.group;

namespace com.ethnicthv.chemlab.engine.api.reaction
{
    public interface INeedReactantGroups
    {
        public List<MoleculeGroup> GetReactantGroups();
    }
}