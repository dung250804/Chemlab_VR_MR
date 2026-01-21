using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.element;

namespace com.ethnicthv.chemlab.engine.serializer
{
    public class Branch
    {
        private readonly List<Node> _nodes = new();
        private Node _startNode;
        private Node _endNode;

        public Branch(Node node)
        {
            _nodes.Add(node);
            _startNode = node;
            _endNode = node;
        }

        public string Serialize()
        {
            return _startNode.Serialize();
        }

        public Node GetStartNode()
        {
            return _startNode;
        }

        public Node GetEndNode()
        {
            return _endNode;
        }

        private List<Node> GetNodes()
        {
            return _nodes;
        }

        public Branch Add(Node node, Bond.BondType bondType)
        {
            _nodes.Add(node);
            var newEdge = new Edge(_endNode, node, bondType);
            _endNode.AddEdge(newEdge);
            node.AddEdge(newEdge);
            node.SetBranch(this);
            node.Visited = true;
            _endNode = node;
            return this;
        }

        public Branch Add(Branch branchToAdd, Bond.BondType bondType)
        {
            var newEdge = new Edge(_endNode, branchToAdd.GetStartNode(), bondType);
            _nodes.AddRange(branchToAdd.GetNodes());

            foreach (var node in branchToAdd.GetNodes())
            {
                node.SetBranch(this);
            }

            branchToAdd.GetStartNode().AddEdge(newEdge);
            _endNode.AddEdge(newEdge);
            _endNode = branchToAdd._endNode;
            return this;
        }

        public Branch Flip()
        {
            foreach (var edge in from node in _nodes from edge in node.GetEdges() where !edge.Marked select edge)
            {
                edge.Flip();
                edge.Marked = true;
            }

            foreach (var edge in _nodes.SelectMany(node => node.GetEdges()))
            {
                edge.Marked = false;
            }

            (_startNode, _endNode) = (_endNode, _startNode);
            return this;
        }

        public float GetMass()
        {
            var total = 0.0F;
            foreach (var node in _nodes)
            {
                total += GetMassForComparisonInSerialization(node.GetAtom());

                total += node.GetSideBranches().Keys.Sum(branch => branch.GetMass());
            }

            return total;
        }

        public float GetMassOfLongestChain()
        {
            return _nodes.Sum(node => GetMassForComparisonInSerialization(node.GetAtom()));
        }

        public static float GetMassForComparisonInSerialization(Atom atom)
        {
            return ElementProperty.GetElementProperty(atom.GetElement()).AtomicMass *
                   (atom.GetElement() == Element.RGroup ? atom.RGroupNumber : 1);
        }
    }
}