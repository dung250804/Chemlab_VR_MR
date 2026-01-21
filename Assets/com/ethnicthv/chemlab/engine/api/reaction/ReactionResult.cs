#nullable enable
using System.Collections.Generic;

namespace com.ethnicthv.chemlab.engine.api.reaction
{
    public abstract class ReactionResult {
        protected readonly IReactingReaction? Reaction;
        protected readonly float Moles;
        private readonly bool _oneOff;

        protected ReactionResult(float moles, IReactingReaction reaction) {
            Moles = moles;
            Reaction = reaction;
            _oneOff = moles == 0.0F;
        }

        public float GetRequiredMoles() {
            return Moles;
        }

        public IReactingReaction? GetReaction() {
            return Reaction;
        }

        public bool isOneOff() {
            return _oneOff;
        }

        // public ICollection<PrecipitateReactionResult> getAllPrecipitates() {
        //     return Collections.emptySet();
        // }
    }
}