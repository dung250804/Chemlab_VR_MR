using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.element;

namespace com.ethnicthv.chemlab.engine.serializer
{
    public class Node
    {
        private readonly Atom _atom;
        public bool Visited;
        private readonly List<Edge> _edges;
        private Branch _branch;
        private readonly Dictionary<Branch, Bond.BondType> _sideBranches;

        public Node(Atom atom)
        {
            _atom = atom;
            Visited = false;
            _edges = new List<Edge>();
            _sideBranches = new Dictionary<Branch, Bond.BondType>();
        }

        public string Serialize()
        {
            var s = ElementProperty.GetElementProperty(GetAtom().GetElement()).Symbol;
            var isTerminal = true;
            Edge nextEdge = null;

            foreach (var edge in this._edges)
            {
                if (edge.GetSourceNode() != this) continue;
                isTerminal = false;
                nextEdge = edge;
                break;
            }

            if (_atom.RGroupNumber != 0 && _atom.GetElement() == Element.RGroup)
            {
                s += _atom.RGroupNumber;
            }

            if (_atom.FormalCharge != 0.0D)
            {
                s = s + "^" + (_atom.FormalCharge % 1.0D != 0.0D
                    ? $"{_atom.FormalCharge}"
                    : $"{_atom.FormalCharge:F0}");
            }

            if (!isTerminal && nextEdge != null)
            {
                s += BondSerialize.Serialize(nextEdge.BondType);
            }

            s = GetSideBranches().Aggregate(s, (current, entry) =>
                current + "(" + BondSerialize.Serialize(entry.Value) + entry.Key.Serialize() + ")");

            if (!isTerminal && nextEdge != null)
            {
                s += nextEdge.GetDestinationNode().Serialize();
            }

            return s;
        }

        public Atom GetAtom()
        {
            return _atom;
        }

        public Node AddEdge(Edge edge)
        {
            this._edges.Add(edge);
            return this;
        }

        public Node DeleteEdge(Edge edge)
        {
            this._edges.Remove(edge);
            return this;
        }

        public List<Edge> GetEdges()
        {
            return this._edges;
        }

        public Node SetBranch(Branch branch)
        {
            this._branch = branch;
            return this;
        }

        public Branch GetBranch()
        {
            return this._branch;
        }

        public Node AddSideBranch(Branch branch, Bond.BondType bondType)
        {
            this._sideBranches.Add(branch, bondType);
            return this;
        }

        public Dictionary<Branch, Bond.BondType> GetSideBranches()
        {
            return this._sideBranches;
        }

        public List<(Branch, Bond.BondType)> GetOrderedSideBranches()
        {
            //Note: Convert Dictionary to array of pair
            var sideBranchesAndBondTypes = _sideBranches.Select(
                entry => (entry.Key, entry.Value)).ToList();

            //Note: Sort the array by bond type
            sideBranchesAndBondTypes.Sort((
                entry1, entry2) => entry1.Key.GetMassOfLongestChain()
                .CompareTo(entry2.Key.GetMassOfLongestChain()));

            return sideBranchesAndBondTypes;
        }
    }
}