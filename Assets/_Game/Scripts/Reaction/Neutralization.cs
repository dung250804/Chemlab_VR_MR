using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.reaction;
using com.ethnicthv.chemlab.engine.molecule;
using com.ethnicthv.chemlab.engine.util;

public class NeutralizationReaction : IReaction
{
    public void CheckForReaction(ReactionContext context, in IOnlyPushList<IReactingReaction> result)
    {
        if (!context.ContainsMolecule(Molecules.Proton) ||
            !context.ContainsMolecule(Molecules.Hydroxide))
            return;

        result.Push(new NeutralizationReacting());
    }
}

public class NeutralizationReacting : IReactingReaction
{
    public int GetPriority() => 200;

    public IReadOnlyList<Molecule> GetReactants() => new[]
    {
        Molecules.Proton,
        Molecules.Hydroxide
    };

    public int GetReactantMolarRatio(Molecule reactant) => 1;

    public IReadOnlyList<Molecule> GetProducts() => new[]
    {
        Molecules.Water
    };

    public int GetProductMolarRatio(Molecule product) => 1;

    public Dictionary<Molecule, int> GetOrders() => new()
    {
        { Molecules.Proton, 1 },
        { Molecules.Hydroxide, 1 }
    };

    public float GetRateConstant(float temperature) => 2000f;

    public float GetEnthalpyChange() => -57f;

    public bool HasResult() => false;
    public ReactionResult GetResult() => null;

    public bool IsConsumedSolid() => false;
    public bool IsMoleculeCatalyst(Molecule molecule) => false;

    public IReadOnlyList<Molecule> GetSolidReactants() => new Molecule[0];
    public IReadOnlyList<Molecule> GetSolidReactantsAndCatalysts() => new Molecule[0];

    public string GetId() => "neutralization";

    public int CompareTo(IReactingReaction other)
    {
        return GetPriority().CompareTo(other.GetPriority());
    }
}