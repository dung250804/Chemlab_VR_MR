using System;
using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.error.molecule;
using com.ethnicthv.chemlab.engine.api.molecule.group;
using com.ethnicthv.chemlab.engine.mixture;

namespace com.ethnicthv.chemlab.engine.molecule.group
{
    public class GroupDetectingProgram
    {
        public static GroupDetectingProgram Instance => _instance ??= new GroupDetectingProgram();
        private static GroupDetectingProgram _instance;
        
        private readonly Stack<IGroupDetector> _detectors = new();
        
        private GroupDetectingProgram() { }
        
        public void RegisterDetector(IGroupDetector detector)
        {
            _detectors.Push(detector);
        }

        public void CheckMolecule(Molecule molecule)
        {
            CheckMolecule(molecule, out _);
        }

        public void CheckMolecule(Molecule molecule, out List<MoleculeGroup> groups)
        {
            molecule.ClearGroups();
            groups = new List<MoleculeGroup>();
            foreach (var d in _detectors)
            {
                var context = new DetectingContext
                {
                    Molecule = molecule,
                    AtomList = new List<Atom>(molecule.GetFormula().GetAtoms())
                };
                
                if (!d.ShouldApplyGroup(context, out var anchorAtom)) continue;
                
                //Note: Perform adding group to molecule
                var moleculeGroup = d.GetGroup();
                
                molecule.AddGroup(moleculeGroup);
                
                //Note: Check if anchorAtom is null or empty
                if (anchorAtom == null || anchorAtom.Length == 0)
                    throw new MoleculeGroupCheckException("Anchor atom is null or empty.", molecule);
                
                molecule.AddFunctionalGroup(moleculeGroup, anchorAtom);
                groups.Add(moleculeGroup);
            }
        }
    }
}