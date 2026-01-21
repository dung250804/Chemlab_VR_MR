using System;

namespace com.ethnicthv.chemlab.engine.api.error.reaction
{
    public class ReactionException : ChemistryException
    {
        public ReactionException(string message) : base(message)
        {
        }
    }
}