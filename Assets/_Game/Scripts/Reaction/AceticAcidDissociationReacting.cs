using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.reaction;
using com.ethnicthv.chemlab.engine.molecule;
using com.ethnicthv.chemlab.engine.util;

public class AceticAcidReaction : IReaction
{
    public void CheckForReaction(ReactionContext context, in IOnlyPushList<IReactingReaction> result)
    {
        if (!context.ContainsMolecule(Molecules.AceticAcid))
            return;

        result.Push(new AceticAcidDissociationReacting());
    }
}

public class AceticAcidDissociationReacting : IReactingReaction
{
    public int GetPriority() => 50; // thấp hơn acid mạnh

    public IReadOnlyList<Molecule> GetReactants() => new[]
    {
        Molecules.AceticAcid
    };

    public int GetReactantMolarRatio(Molecule reactant) => 1;

    public IReadOnlyList<Molecule> GetProducts() => new[]
    {
        Molecules.Proton,
        Molecules.Acetate
    };

    public int GetProductMolarRatio(Molecule product)
    {
        if (product == Molecules.Proton) return 1;
        if (product == Molecules.Acetate) return 1;
        return 0;
    }

    public Dictionary<Molecule, int> GetOrders() => new()
    {
        { Molecules.AceticAcid, 1 }
    };

    // 🔥 QUAN TRỌNG: rate thấp vì acid yếu
    public float GetRateConstant(float temperature) => 0.05f;

    public float GetEnthalpyChange() => 0f;

    public bool HasResult() => false;
    public ReactionResult GetResult() => null;

    public bool IsConsumedSolid() => false;
    public bool IsMoleculeCatalyst(Molecule molecule) => false;

    public IReadOnlyList<Molecule> GetSolidReactants() => new Molecule[0];
    public IReadOnlyList<Molecule> GetSolidReactantsAndCatalysts() => new Molecule[0];

    public string GetId() => "acetic_acid_dissociation";

    public int CompareTo(IReactingReaction other)
    {
        return GetPriority().CompareTo(other.GetPriority());
    }
}