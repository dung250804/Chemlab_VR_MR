using UnityEngine;
using com.ethnicthv.chemlab.engine.molecule;
public enum MoleculeType
{
    None,
    Water,
    Proton,
    Hydroxide,
    Oleum,
    SulfuricAcid,
    SodiumIon,
    Chloride,
    Copper2Ion,
    Hydrogen,
    HydrochloricAcid,
    Hydrogensulfate,
    AceticAcid,
    Acetate,
    Ammonium,
    Ammonia,
    Sulfate
}

public static class MoleculeRegistry
{
    public static Molecule Get(MoleculeType type)
    {
        switch (type)
        {
            case MoleculeType.None: return null;
            case MoleculeType.Water: return Molecules.Water;
            case MoleculeType.Proton: return Molecules.Proton;
            case MoleculeType.Hydroxide: return Molecules.Hydroxide;
            case MoleculeType.Oleum: return Molecules.Oleum;
            case MoleculeType.SulfuricAcid: return Molecules.SulfuricAcid;
            case MoleculeType.SodiumIon: return Molecules.SodiumIon;
            case MoleculeType.Chloride: return Molecules.Chloride;
            case MoleculeType.Copper2Ion: return Molecules.Copper2Ion;
            case MoleculeType.Hydrogen: return Molecules.Hydrogen;
            case MoleculeType.HydrochloricAcid: return Molecules.HydrochloricAcid;
            case MoleculeType.Hydrogensulfate: return Molecules.Hydrogensulfate;
            case MoleculeType.AceticAcid: return Molecules.AceticAcid;
            case MoleculeType.Acetate: return Molecules.Acetate;
            case MoleculeType.Ammonium: return Molecules.Ammonium;
            case MoleculeType.Ammonia: return Molecules.Ammonia;
            case MoleculeType.Sulfate: return Molecules.Sulfate;

            default: return null;
        }
    }
}