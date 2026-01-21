using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.molecule.group;
using com.ethnicthv.chemlab.engine.api.reaction;
using com.ethnicthv.chemlab.engine.formula;
using com.ethnicthv.chemlab.engine.molecule;
using com.ethnicthv.chemlab.engine.molecule.group.functional;
using com.ethnicthv.chemlab.engine.util;

namespace com.ethnicthv.chemlab.engine.reaction.generic
{
    public class EsterificationReaction : IReaction, INeedReactantGroups
    {
        private static readonly List<MoleculeGroup> ReactantGroups = new()
        {
            MoleculeGroup.Alcohol,
            MoleculeGroup.CarboxylicAcid
        };

        public void CheckForReaction(ReactionContext context, in IOnlyPushList<IReactingReaction> result)
        {
            ReactionUtil.ForeachDualGroup(context, MoleculeGroup.Alcohol, MoleculeGroup.CarboxylicAcid, result,
                (reactionContext, alcoholMolecule, carboxylMolecule, alcoholPart, carboxylPart, result) =>
                {
                    var alcoholCopy = (Formula)alcoholMolecule.GetFormula().Clone();
                    var carboxylCopy = (Formula)carboxylMolecule.GetFormula().Clone();

                    var alcoholFunctionalGroup = (AlcoholFunctionalGroup)alcoholPart;
                    var carboxylFunctionalGroup = (CarboxylFunctionGroup)carboxylPart;

                    alcoholCopy.RemoveBond(alcoholFunctionalGroup.Oxygen, alcoholFunctionalGroup.Hydrogen);
                    carboxylCopy.RemoveBond(carboxylFunctionalGroup.Carbon, carboxylFunctionalGroup.Oxygen);

                    carboxylCopy.MoveToAtom(carboxylFunctionalGroup.Carbon).AddStructure(alcoholCopy);

                    var ester = Molecule.Builder.Create(true)
                        .Structure(Formula.JoinFormulae(carboxylCopy, alcoholCopy, Bond.BondType.Single)).Build();

                    //TODO: Add the new molecule to the result
                    ReactingReaction.GeneratedReactionBuilder(result).AddReactant(alcoholMolecule)
                        .AddReactant(carboxylMolecule, 1, 0).AddReactant(Molecules.Oleum, 1)
                        .AddProduct(ester).AddProduct(Molecules.SulfuricAcid, 2).Build();
                });
        }

        public List<MoleculeGroup> GetReactantGroups()
        {
            return ReactantGroups;
        }
    }
}