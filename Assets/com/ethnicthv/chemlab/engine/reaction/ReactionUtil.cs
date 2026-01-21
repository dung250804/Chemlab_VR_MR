using System;
using com.ethnicthv.chemlab.engine.api.molecule;
using com.ethnicthv.chemlab.engine.api.molecule.group;
using com.ethnicthv.chemlab.engine.api.reaction;
using com.ethnicthv.chemlab.engine.molecule;
using com.ethnicthv.chemlab.engine.molecule.group.functional;
using com.ethnicthv.chemlab.engine.util;

namespace com.ethnicthv.chemlab.engine.reaction
{
    public class ReactionUtil
    {
        public static void ForeachDualGroup(ReactionContext context, MoleculeGroup groupA, MoleculeGroup groupB, 
            in IOnlyPushList<IReactingReaction> result, 
            in Action<ReactionContext, Molecule, Molecule, IFunctionalGroup, IFunctionalGroup, IOnlyPushList<IReactingReaction>> forwardAction)
        {
            var groupAMolecule = context.GetGroupMembers(groupA);
            var groupBMolecule = context.GetGroupMembers(groupB);
            foreach (var aMolecule in groupAMolecule)
            {
                foreach (var bMolecule in groupBMolecule)
                {
                    foreach (var aPart in aMolecule.GetAtomsInGroup(groupA))
                    {
                        foreach (var bPart in bMolecule.GetAtomsInGroup(groupB))
                        {
                            forwardAction.Invoke(context, aMolecule, bMolecule, aPart, bPart, result);
                        }
                    }
                }
            }
        }
    }
}