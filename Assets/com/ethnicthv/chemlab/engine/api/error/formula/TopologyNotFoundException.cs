using System;

namespace com.ethnicthv.chemlab.engine.api.error.formula
{
    public class TopologyNotFoundException : Exception
    {
        public TopologyNotFoundException(string topology) : base("Topology not found: " + topology + ".")
        {
        }
    }
}