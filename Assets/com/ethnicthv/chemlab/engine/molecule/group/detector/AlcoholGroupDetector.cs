using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.element;
using com.ethnicthv.chemlab.engine.api.molecule.group;
using com.ethnicthv.chemlab.engine.molecule.group.functional;

namespace com.ethnicthv.chemlab.engine.molecule.group.detector
{
    public class AlcoholGroupDetector : IGroupDetector
    {
        public bool ShouldApplyGroup(DetectingContext context, out IFunctionalGroup[] anchorAtom)
        {
            //Note: anchorAtom = null to avoid warning, this will be assigned after if it's true
            anchorAtom = null;
            
            var formula = context.Molecule.GetFormula();
            var structure = formula.GetStructure();
            var atoms = context.AtomList;
            var oxygen = atoms.FindAll(a => a.GetElement() == Element.Oxygen);

            if (oxygen.Count == 0) return false;

            var anchorAtoms = new LinkedList<IFunctionalGroup>();
            foreach (var atom in oxygen)
            {
                //Note: Rule 1 is to check if the oxygen atom is connected to a carbon atom and a hydrogen atom
                var bonds = structure[atom];
                if (bonds.Count != 2) continue;
                var x = bonds[0].GetDestinationAtom();
                var y = bonds[1].GetDestinationAtom();
                if (x.GetElement() == Element.Hydrogen && y.GetElement() == Element.Carbon)
                {
                    (x, y) = (y, x);
                    goto Rule2;
                }

                if (x.GetElement() == Element.Carbon || y.GetElement() == Element.Hydrogen)
                {
                    goto Rule2;
                }

                continue;
                
                //Note: Rule 2 is to check if the connected carbon atom is not having a double bond to an oxygen atom
                Rule2:
                var cBonds = structure[y];
                if (cBonds.Count != 3) goto Final;
                if (cBonds.Any(b=>b.GetDestinationAtom().GetElement() == Element.Oxygen && b.GetBondType() == Bond.BondType.Double)) continue;
                
                Final:
                anchorAtoms.AddLast(new AlcoholFunctionalGroup(atom, y));
            }

            if (anchorAtoms.Count == 0) return false;

            anchorAtom = anchorAtoms.ToArray();
            return true;
        }

        public MoleculeGroup GetGroup()
        {
            return MoleculeGroup.Alcohol;
        }
    }
}