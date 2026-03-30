using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.element;
using com.ethnicthv.chemlab.engine.api.molecule;
using com.ethnicthv.chemlab.engine.formula;
using UnityEngine;

namespace com.ethnicthv.chemlab.engine.molecule
{
    public class Molecules
    {
        private static Molecule.Builder Builder()
        {
            return Molecule.Builder.Create(false);
        }

        public static readonly Molecule Water = Builder().ID("water")
            .Structure(Formula.Deserialize("linear:HOH"))
            .Color(new Color(0.1f, 0.4f, 0.9f, 1f).ToHex()) // Xanh dương đậm
            .BoilingPoint(100.0F).Density(1000.0F)
            .SpecificHeatCapacity(4160.0F).Tag(MoleculeTag.Solvent)
            .Build();

        public static readonly Molecule Proton = Builder().ID("proton")
            .Structure(Formula.CreateNewFormula(new Atom(Element.Hydrogen, 1)))
            .Color(new Color(0.9f, 0.1f, 0.1f, 1f).ToHex()) // Đỏ tươi rực rỡ
            .Build();

        public static readonly Molecule Hydroxide = Builder().ID("hydroxide")
            .Structure(Formula.CreateNewFormula(new Atom(Element.Hydrogen))
                .AddAtom(new Atom(Element.Oxygen, -1)))
            .Color(new Color(0.1f, 0.1f, 0.8f, 1f).ToHex()) // Xanh lam đậm (Ultramarine)
            .Density(900.0F).Build();

        public static readonly Molecule Oleum = Builder().ID("oleum")
            .Structure(Formula.Deserialize("linear:HOS(=O)(=O)OS(=O)(=O)OH"))
            .Color(new Color(0.5f, 0.1f, 0.7f, 1f).ToHex()) // Tím đậm
            .BoilingPoint(10.0F).Density(1820.0F).SpecificHeatCapacity(2600.0F)
            .Tag(MoleculeTag.AcutelyToxic).Build();

        public static readonly Molecule SulfuricAcid = Builder().ID("sulfuric_acid")
            .Structure(Formula.Deserialize("linear:OS(=O)(=O)O"))
            .Color(new Color(0.8f, 0.4f, 0.1f, 1f).ToHex()) // Cam cháy đậm (để cảnh báo độc)
            .BoilingPoint(337.0F)
            .Density(1830.2F).MolarHeatCapacity(83.68F)
            .Tag(MoleculeTag.AcutelyToxic)
            .Build();

        public static readonly Molecule Sodium = Builder().ID("sodium")
            .Structure(Formula.CreateNewFormula(new Atom(Element.Sodium)))
            .Color(new Color(0.4f, 0.4f, 0.4f, 1f).ToHex()) // Xám tro đậm
            .Solid().Build();

        public static readonly Molecule SodiumIon = Builder().ID("sodium_ion")
            .Structure(Formula.CreateNewFormula(new Atom(Element.Sodium, 1)))
            .Color(new Color(0.9f, 0.7f, 0.1f, 1f).ToHex()) // Vàng nghệ đậm
            .Density(900.0F).Build();

        public static readonly Molecule Chloride = Builder().ID("chloride")
            .Structure(Formula.CreateNewFormula(new Atom(Element.Chlorine, -1)))
            .Color(new Color(0.1f, 0.8f, 0.1f, 1f).ToHex()) // Xanh lục đậm
            .Build();

        public static readonly Molecule Copper = Builder().ID("copper")
            .Structure(Formula.CreateNewFormula(new Atom(Element.Copper)))
            .Color(new Color(0.7f, 0.3f, 0.1f, 1f).ToHex()) // Màu đồng thau đậm
            .Solid().Build();

        public static readonly Molecule Copper2Ion = Builder().ID("copper_II_ion")
            .Structure(Formula.CreateNewFormula(new Atom(Element.Copper, 2)))
            .Color(new Color(0.1f, 0.5f, 0.9f, 1f).ToHex()) // Xanh lam rực
            .Density(900.0F).Build();

        public static readonly Molecule Hydrogen = Builder().ID("hydrogen")
            .Structure(Formula.Deserialize("linear:HH"))
            .Color(new Color(0.5f, 0.5f, 0.5f, 1f).ToHex()) // Xám đậm
            .BoilingPointInKelvins(20.271f)
            .UnsolvableGas()
            .Density(70.85f)
            .MolarHeatCapacity(28.84f)
            .Burnable()
            .BurnColor(new Color(0.2f, 0.4f, 1.0f), 1f) // Lửa xanh đậm
            .Build();

        public static readonly Molecule HydrochloricAcid = Builder().ID("hydrochloric_acid")
            .Structure(Formula.Deserialize("linear:ClH"))
            .Color(new Color(0.2f, 0.7f, 0.2f, 1f).ToHex()) // Xanh lá rực
            .BoilingPoint(-85.05f)
            .Density(1490f).SpecificHeatCapacity(798.1f)
            .Tag(MoleculeTag.AcutelyToxic).Tag(MoleculeTag.OzoneDepleter)
            .Build();

        public static readonly Molecule Hydrogensulfate = Builder().ID("hydrogensulfate")
            .Structure(Formula.Deserialize("linear:O=S(=O)(OH)O^-1"))
            .Color(new Color(0.6f, 0.1f, 0.8f, 1f).ToHex()) // Tím rực rỡ
            .Tag(MoleculeTag.AcidRain)
            .Build();

        public static readonly Molecule AceticAcid = Builder().ID("acetic_acid")
            .Structure(Formula.Deserialize("linear:CC(=O)OH"))
            .Color(new Color(0.9f, 0.8f, 0.1f, 1f).ToHex()) // Vàng chanh đậm
            .BoilingPoint(118.5f)
            .Density(1049f).MolarHeatCapacity(123.1f)
            .Tag(MoleculeTag.Smelly).Tag(MoleculeTag.Smog)
            .Build();

        public static readonly Molecule Acetate = Builder().ID("acetate")
            .Structure(Formula.Deserialize("linear:CC~(~O^-0.5)O^-0.5"))
            .Color(new Color(0.2f, 0.5f, 0.8f, 1f).ToHex()) // Xanh da trời đậm
            .Tag(MoleculeTag.Smelly).Tag(MoleculeTag.Smog)
            .Build();

        public static readonly Molecule Ammonium = Builder()
            .ID("ammonium")
            .Structure(Formula.CreateNewFormula(new Atom(Element.Nitrogen, 1))
                .AddAtom(new Atom(Element.Hydrogen), move: false)
                .AddAtom(new Atom(Element.Hydrogen), move: false)
                .AddAtom(new Atom(Element.Hydrogen), move: false)
                .AddAtom(new Atom(Element.Hydrogen), move: false))
            .Color(new Color(0.1f, 0.6f, 0.7f, 1f).ToHex()) // Xanh lơ (Cyan) đậm
            .Build();
        
        public static readonly Molecule Ammonia = Builder()
            .ID("ammonia")
            .Structure(Formula.Deserialize("linear:N"))
            .Color(new Color(0.1f, 0.8f, 0.8f, 1f).ToHex()) // Ngọc lam đậm (Turquoise)
            .BoilingPoint(-33.34f)
            .Density(900f)
            .MolarHeatCapacity(80f)
            .Tag(MoleculeTag.Refrigerant)
            .Tag(MoleculeTag.Smelly)
            .Build();
        
        public static readonly Molecule Sulfate = Builder().ID("sulfate")
            .Structure(Formula.CreateNewFormula(new Atom(Element.Sulfur, 2))
                .AddAtom(new Atom(Element.Oxygen, -1), move: false)
                .AddAtom(new Atom(Element.Oxygen, -1), move: false)
                .AddAtom(new Atom(Element.Oxygen, -1), move: false)
                .AddAtom(new Atom(Element.Oxygen, -1), move: false)
            )
            .Color(new Color(0.8f, 0.1f, 0.5f, 1f).ToHex()) // Hồng tím đậm (Magenta)
            .Tag(MoleculeTag.AcidRain)
            .Build();
    }
}