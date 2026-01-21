using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.error;
using com.ethnicthv.chemlab.engine.api.error.formula;

namespace com.ethnicthv.chemlab.engine.formula
{
    public delegate Formula FormulaFactory();
    public class FormulaTopology
    {
        public string TopologyNamespace { get; }
        public FormulaFactory Factory { get; }
        public FormulaTopology(FormulaFactory factory, string topologyNamespace)
        {
            Factory = factory;
            TopologyNamespace = topologyNamespace;
            
            RegisterTopology(this);
        }
        
        private static readonly Dictionary<string, FormulaTopology> Topologies = new();
        public static void RegisterTopology(FormulaTopology formulaTopology)
        {
            Topologies[formulaTopology.TopologyNamespace] = formulaTopology;
        }
        
        public static FormulaTopology GetTopology(string topologyNamespace)
        {
            if (Topologies.TryGetValue(topologyNamespace, out var topology))
            {
                return topology;
            }

            throw new TopologyNotFoundException(topologyNamespace);
        }
    }
}