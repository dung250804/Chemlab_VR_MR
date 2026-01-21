using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.molecule.group;
using com.ethnicthv.chemlab.engine.molecule;

namespace com.ethnicthv.chemlab.engine.api.reaction
{
    public class ReactionContext
    {
        private readonly Dictionary<MoleculeGroup, List<Molecule>> _groups;
        private readonly Dictionary<Molecule, float> _molecules;
        
        public ReactionContext(Dictionary<MoleculeGroup, List<Molecule>> groups, Dictionary<Molecule, float> molecules)
        {
            _groups = groups;
            _molecules = molecules;
        }
        
        public bool ContainsMolecule(Molecule molecule)
        {
            return _molecules.ContainsKey(molecule);
        }
        
        public float GetMoleculeAmount(Molecule molecule)
        {
            return _molecules[molecule];
        }
        
        public bool ContainsGroup(MoleculeGroup group)
        {
            return _groups.ContainsKey(group);
        }
        
        public IReadOnlyList<Molecule> GetGroupMembers(MoleculeGroup group)
        {
            return _groups[group];
        }
    }
}