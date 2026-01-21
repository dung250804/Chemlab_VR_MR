using com.ethnicthv.chemlab.engine.api;

namespace com.ethnicthv.chemlab.engine.serializer
{
    public class Edge
    {
        public Bond.BondType BondType;
        private Node _srcNode;
        private Node _destNode;
        public bool Marked;

        public Edge(Node srcNode, Node destNode, Bond.BondType bondType)
        {
            _srcNode = srcNode;
            _destNode = destNode;
            BondType = bondType;
            Marked = false;
        }

        public Node GetSourceNode()
        {
            return _srcNode;
        }

        public Node GetDestinationNode()
        {
            return _destNode;
        }

        public void Flip()
        {
            (_srcNode, _destNode) = (_destNode, _srcNode);
        }
    }
}