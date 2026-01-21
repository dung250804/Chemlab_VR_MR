using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.molecule;
using com.ethnicthv.chemlab.engine.api.molecule.group;
using com.ethnicthv.chemlab.engine.molecule;
using com.ethnicthv.chemlab.engine.util;

namespace com.ethnicthv.chemlab.engine.api.reaction
{
    public interface IReaction
    {
        public void CheckForReaction(ReactionContext context, in IOnlyPushList<IReactingReaction> result);
    }
}