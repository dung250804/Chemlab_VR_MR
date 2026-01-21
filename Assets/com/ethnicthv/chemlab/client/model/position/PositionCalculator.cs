using UnityEngine;
using com.ethnicthv.chemlab.client.model.position.topology;
using com.ethnicthv.chemlab.client.model.position.topology.rings;
using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.molecule.formula;

namespace com.ethnicthv.chemlab.client.model.position
{
    public class PositionCalculator
    {
        private readonly DefaultLinear _linear = new();
        private readonly DefaultRingsTopology _ringsTopology = new();
        
        public Vector3 GetCurrentPosition(IFormula formula, Atom currentAtom, GenericAtomModel previousAtomModel)
        {
            var prevAtomData = formula.CheckAtomData(previousAtomModel.GetAtom());
            
            var inDirection = Vector3.right;
            if (previousAtomModel.ParentAtom != null)
            {
                inDirection = previousAtomModel.GetPosition() - previousAtomModel.ParentAtom.GetPosition();
            }
            
            inDirection.Normalize();
            
            var branchIndex = GetBranchIndex(currentAtom, prevAtomData);
            var maxBranch = prevAtomData.Neighbors.Count;

            if (!prevAtomData.InRing)
            {
                //Note: apply rotation of the parent atom to the final position
                return previousAtomModel.GetRotation() * _linear.GetNextPriorityPosition(inDirection, maxBranch, branchIndex);
            }
            else
            {
                return new Vector3(0, 2, 0);
            }
        }
        
        private static int GetBranchIndex(Atom atom, FormulaAtomData prevAtomData)
        {
            for (var i = 0; i < prevAtomData.Neighbors.Count; i++)
            {
                if (prevAtomData.Neighbors[i] == atom)
                {
                    return i;
                }
            }

            throw new System.Exception("Branch index not found");
        }
    }
}