using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.reaction;
using com.ethnicthv.chemlab.engine.molecule;
using com.ethnicthv.chemlab.engine.util;

public class SulfuricAcidReaction : IReaction
{
    public void CheckForReaction(ReactionContext context, in IOnlyPushList<IReactingReaction> result)
    {
        if (!context.ContainsMolecule(Molecules.SulfuricAcid))
            return;

        result.Push(new SulfuricAcidDissociationReacting());
    }
}

public class SulfuricAcidDissociationReacting : IReactingReaction
{
    public int GetPriority() => 100;

    public IReadOnlyList<Molecule> GetReactants() => new[]
    {
        Molecules.SulfuricAcid
    };

    public int GetReactantMolarRatio(Molecule reactant) => 1;

    public IReadOnlyList<Molecule> GetProducts() => new[]
    {
        Molecules.Proton,
        Molecules.Sulfate
    };

    public int GetProductMolarRatio(Molecule product)
    {
        if (product == Molecules.Proton) return 2;
        if (product == Molecules.Sulfate) return 1;
        return 0;
    }

    public Dictionary<Molecule, int> GetOrders() => new()
    {
        { Molecules.SulfuricAcid, 1 }
    };

    public float GetRateConstant(float temperature) => 1000f;

    public float GetEnthalpyChange() => 0f;

    public bool HasResult() => false;
    public ReactionResult GetResult() => null;

    public bool IsConsumedSolid() => false;
    public bool IsMoleculeCatalyst(Molecule molecule) => false;

    public IReadOnlyList<Molecule> GetSolidReactants() => new Molecule[0];
    public IReadOnlyList<Molecule> GetSolidReactantsAndCatalysts() => new Molecule[0];

    public string GetId() => "h2so4_dissociation";

    public int CompareTo(IReactingReaction other)
    {
        return GetPriority().CompareTo(other.GetPriority());
    }
}