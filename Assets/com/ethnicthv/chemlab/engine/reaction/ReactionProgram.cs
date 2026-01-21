using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.engine.api.molecule;
using com.ethnicthv.chemlab.engine.api.molecule.group;
using com.ethnicthv.chemlab.engine.api.reaction;
using com.ethnicthv.chemlab.engine.molecule;
using com.ethnicthv.chemlab.engine.util;

namespace com.ethnicthv.chemlab.engine.reaction
{
    public class ReactionProgram
    {
        public static ReactionProgram Instance { get; } = new();
        
        private readonly List<IReaction> _reactions = new(); 
        
        private ReactionProgram() { }
        
        public void RegisterReaction(IReaction reaction)
        {
            _reactions.Add(reaction);
        }
        
        public void CheckForReaction(ReactionContext context , in CustomList<IReactingReaction> results)
        {
            foreach (var reaction in _reactions)
            {
                if (reaction is INeedReactantGroups needReactantGroups)
                {
                    var reactantGroups = needReactantGroups.GetReactantGroups();
                    if (reactantGroups.Any(reactantGroup => !context.ContainsGroup(reactantGroup)))
                    {
                        continue;
                    }
                }

                reaction.CheckForReaction(context, results);
            }
        }
    }
}