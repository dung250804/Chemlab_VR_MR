using System;
using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.molecule;

namespace com.ethnicthv.chemlab.engine.api.reaction
{
    public interface IReactingReaction : IComparable<IReactingReaction>
    {
        public int GetPriority();
        bool HasResult();
        ReactionResult GetResult();
        IReadOnlyList<Molecule> GetReactants();
        int GetReactantMolarRatio(Molecule reactant);
        IReadOnlyList<Molecule> GetProducts();
        int GetProductMolarRatio(Molecule product);
        float GetEnthalpyChange();
        bool IsConsumedSolid();
        bool IsMoleculeCatalyst(Molecule molecule);
        IReadOnlyList<Molecule> GetSolidReactants();
        IReadOnlyList<Molecule> GetSolidReactantsAndCatalysts();
        Dictionary<Molecule, int> GetOrders();
        float GetRateConstant(float temperature);
        string GetId();
    }
}