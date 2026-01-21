using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.engine.api.element;

namespace com.ethnicthv.chemlab.engine.api
{
    public class ElementProperty : IElement
    {
        public readonly float AtomicMass;
        public readonly string Symbol;
        public readonly string Name;
        public readonly int AtomicNumber;
        public readonly string ElectronConfiguration;
        public readonly ElementGroup Group;
        public readonly float Density;
        public readonly float MeltingPoint;
        public readonly float BoilingPoint;
        public readonly float Electronegativity;
        public readonly double[] Valences;

        private ElementProperty(float atomicMass, string symbol, string name, int atomicNumber,
            string electronConfiguration,
            ElementGroup group, float density, float meltingPoint, float boilingPoint, float electronegativity,
            double[] valences)
        {
            AtomicMass = atomicMass;
            Symbol = symbol;
            Name = name;
            AtomicNumber = atomicNumber;
            ElectronConfiguration = electronConfiguration;
            Group = group;
            Density = density;
            MeltingPoint = meltingPoint;
            BoilingPoint = boilingPoint;
            Electronegativity = electronegativity;
            Valences = valences;
        }

        public Element GetElement()
        {
            return (Element)AtomicNumber;
        }
        
        public double GetNextLowestValency(double valency) {
            foreach (var validValency in Valences) {
                if (validValency >= valency) return validValency;
            }
            return 0;
        }

        public static ElementProperty GetElementProperty(Element element)
        {
            return ElementProperties[(int)element];
        }

        public static ElementProperty GetElementProperty(int atomicNumber)
        {
            return ElementProperties[atomicNumber];
        }

        public static ElementProperty GetElementProperty(string symbol, bool isName = false)
        {
            return isName ? ElementProperties.First(property => property.Name == symbol) : 
                ElementProperties.First(property => property.Symbol == symbol);
        }

        private static readonly ElementProperty[] ElementProperties =
        {
            new(1.0E-4f, "R", "RGroup", 0, "", ElementGroup.NonMetals, 
                0.0f, 0.0f, 0.0f, 0.0f, new double[] { 0 }),
            new(1.008f, "H", "Hydrogen", 1, "1s1", ElementGroup.NonMetals,
                0.08988f, 14.01f, 20.28f, 2.20f, new double[] { 1 }),

            new(4.0026f, "He", "Helium", 2, "1s2", ElementGroup.NonMetals, 0.1786f,
                0.95f, 4.22f, 0.0f, new double [] { 0 }),

            new(6.94f, "Li", "Lithium", 3, "[He] 2s1", ElementGroup.Metals, 0.534f,
                453.69f, 1615f, 0.98f, new double [] { 1 }),

            new(9.0122f, "Be", "Beryllium", 4, "[He] 2s2", ElementGroup.Metals,
                1.85f, 1560f, 2742f, 1.57f, new double [] { 2 }),

            new(10.81f, "B", "Boron", 5, "[He] 2s2 2p1", ElementGroup.Metalloids,
                2.34f, 2349f, 4200f, 2.04f, new double [] { 3 }),

            new(12.011f, "C", "Carbon", 6, "[He] 2s2 2p2", ElementGroup.NonMetals,
                2.267f, 3800f, 4300f, 2.55f, new double [] { 2, 4 }),

            new(14.007f, "N", "Nitrogen", 7, "[He] 2s2 2p3",
                ElementGroup.NonMetals, 0.0012506f, 63.15f, 77.36f, 3.04f, new double [] { 3, 5 }),

            new(15.999f, "O", "Oxygen", 8, "[He] 2s2 2p4", ElementGroup.NonMetals,
                0.001429f, 54.36f, 90.20f, 3.44f, new[] { 0, 1.5d, 2 }),

            new(18.998f, "F", "Fluorine", 9, "[He] 2s2 2p5",
                ElementGroup.NonMetals, 0.001696f, 53.53f, 85.03f, 3.98f, new double [] { 1 }),

            new(20.180f, "Ne", "Neon", 10, "[He] 2s2 2p6", ElementGroup.NonMetals,
                0.9002f, 24.56f, 27.07f, 0.0f, new double [] { 0 }),

            new(22.990f, "Na", "Sodium", 11, "[Ne] 3s1", ElementGroup.Metals, 0.971f,
                370.95f, 1156f, 0.93f, new double [] { 1 }),

            new(24.305f, "Mg", "Magnesium", 12, "[Ne] 3s2", ElementGroup.Metals,
                1.738f, 923f, 1363f, 1.31f, new double [] { 2 }),

            new(26.982f, "Al", "Aluminum", 13, "[Ne] 3s2 3p1", ElementGroup.Metals,
                2.6989f, 933.47f, 2792f, 1.61f, new double [] { 3 }),

            new(28.085f, "Si", "Silicon", 14, "[Ne] 3s2 3p2",
                ElementGroup.Metalloids, 2.3296f, 1687f, 3538f, 1.90f, new double [] { 4 }),

            new(30.974f, "P", "Phosphorus", 15, "[Ne] 3s2 3p3",
                ElementGroup.NonMetals, 1.82f, 317.30f, 550f, 2.19f, new double [] { 3, 5 }),

            new(32.06f, "S", "Sulfur", 16, "[Ne] 3s2 3p4", ElementGroup.NonMetals,
                2.067f, 388.36f, 717.87f, 2.58f, new double [] { 2, 4, 6 }),

            new(35.45f, "Cl", "Chlorine", 17, "[Ne] 3s2 3p5",
                ElementGroup.NonMetals, 0.003214f, 171.6f, 239.11f, 3.16f, new double [] { 1 }),

            new(39.948f, "Ar", "Argon", 18, "[Ne] 3s2 3p6", ElementGroup.NonMetals,
                1.784f, 83.80f, 87.30f, 0.0f, new double [] { 0 }),

            new(39.098f, "K", "Potassium", 19, "[Ar] 4s1", ElementGroup.Metals,
                0.862f, 336.53f, 1032f, 0.82f, new double [] { 1 }),

            new(40.078f, "Ca", "Calcium", 20, "[Ar] 4s2", ElementGroup.Metals,
                1.55f, 1115f, 1757f, 1.00f, new double [] { 2 }),

            new(44.956f, "Sc", "Scandium", 21, "[Ar] 3d1 4s2", ElementGroup.Metals,
                2.989f, 1814f, 3109f, 1.36f, new double [] { 3 }),

            new(47.867f, "Ti", "Titanium", 22, "[Ar] 3d2 4s2", ElementGroup.Metals,
                4.506f, 1941f, 3560f, 1.54f, new double [] { 4 }),

            new(50.942f, "V", "Vanadium", 23, "[Ar] 3d3 4s2", ElementGroup.Metals,
                6.11f, 2183f, 3680f, 1.63f, new double [] { 5 }),

            new(51.996f, "Cr", "Chromium", 24, "[Ar] 3d5 4s1", ElementGroup.Metals,
                7.15f, 2180f, 2944f, 1.66f, new double [] { 3, 6 }),

            new(54.938f, "Mn", "Manganese", 25, "[Ar] 3d5 4s2",
                ElementGroup.Metals, 7.44f, 1519f, 2334f, 1.55f, new double [] { 2, 4, 7 }),

            new(55.845f, "Fe", "Iron", 26, "[Ar] 3d6 4s2", ElementGroup.Metals, 7.874f,
                1811f, 3134f, 1.83f, new double [] { 2, 3, 6 }),

            new(58.933f, "Co", "Cobalt", 27, "[Ar] 3d7 4s2", ElementGroup.Metals,
                8.86f, 1768f, 3200f, 1.88f, new double [] { 2, 3 }),

            new(58.693f, "Ni", "Nickel", 28, "[Ar] 3d8 4s2", ElementGroup.Metals,
                8.912f, 1728f, 3186f, 1.91f, new double [] { 2 }),

            new(63.546f, "Cu", "Copper", 29, "[Ar] 3d10 4s1", ElementGroup.Metals,
                8.96f, 1357.77f, 2835f, 1.90f, new double [] { 1, 2 }),

            new(65.38f, "Zn", "Zinc", 30, "[Ar] 3d10 4s2", ElementGroup.Metals, 7.134f,
                692.88f, 1180f, 1.65f, new double [] { 2 }),

            new(69.723f, "Ga", "Gallium", 31, "[Ar] 3d10 4s2 4p1",
                ElementGroup.Metals, 5.907f, 302.91f, 2673f, 1.81f, new double [] { 3 }),

            new(72.630f, "Ge", "Germanium", 32, "[Ar] 3d10 4s2 4p2",
                ElementGroup.Metalloids, 5.323f, 1211.40f, 3106f, 2.01f, new double [] { 2, 4 }),

            new(74.922f, "As", "Arsenic", 33, "[Ar] 3d10 4s2 4p3",
                ElementGroup.Metalloids, 5.776f, 1090f, 887f, 2.18f, new double [] { 3 }),

            new(78.971f, "Se", "Selenium", 34, "[Ar] 3d10 4s2 4p4",
                ElementGroup.NonMetals, 4.809f, 494f, 958f, 2.55f, new double [] { 2, 4, 6 }),

            new(79.904f, "Br", "Bromine", 35, "[Ar] 3d10 4s2 4p5",
                ElementGroup.NonMetals, 3.122f, 265.8f, 332.0f, 2.96f, new double [] { 1 }),

            new(83.798f, "Kr", "Krypton", 36, "[Ar] 3d10 4s2 4p6",
                ElementGroup.NonMetals, 3.749f, 115.79f, 119.93f, 0.0f, new double [] { 0 }),

            new(85.468f, "Rb", "Rubidium", 37, "[Kr] 5s1", ElementGroup.Metals,
                1.532f, 312.46f, 961f, 0.82f, new double [] { 1 }),

            new(87.62f, "Sr", "Strontium", 38, "[Kr] 5s2", ElementGroup.Metals,
                2.54f, 1050f, 1650f, 0.95f, new double [] { 2 }),

            new(88.906f, "Y", "Yttrium", 39, "[Kr] 4d1 5s2", ElementGroup.Metals,
                4.469f, 1799f, 3609f, 1.22f, new double [] { 3 }),

            new(91.224f, "Zr", "Zirconium", 40, "[Kr] 4d2 5s2",
                ElementGroup.Metals, 6.506f, 2128f, 4682f, 1.33f, new double [] { 4 }),

            new(92.906f, "Nb", "Niobium", 41, "[Kr] 4d4 5s1", ElementGroup.Metals,
                8.57f, 2750f, 5017f, 1.6f, new double [] { 5 }),

            new(95.95f, "Mo", "Molybdenum", 42, "[Kr] 4d5 5s1",
                ElementGroup.Metals, 10.22f, 2896f, 4912f, 2.16f, new double [] { 6 }),

            new(98f, "Tc", "Technetium", 43, "[Kr] 4d5 5s2", ElementGroup.Metals,
                11.5f, 2430f, 4538f, 1.9f, new double [] { 7 }),

            new(101.07f, "Ru", "Ruthenium", 44, "[Kr] 4d7 5s1",
                ElementGroup.Metals, 12.41f, 2607f, 4423f, 2.2f, new double [] { 3, 4, 7 }),

            new(102.91f, "Rh", "Rhodium", 45, "[Kr] 4d8 5s1", ElementGroup.Metals,
                12.41f, 2237f, 3968f, 2.28f, new double [] { 1, 3 }),

            new(106.42f, "Pd", "Palladium", 46, "[Kr] 4d10", ElementGroup.Metals,
                12.02f, 1828.05f, 3236f, 2.20f, new double [] { 2, 4 }),

            new(107.87f, "Ag", "Silver", 47, "[Kr] 4d10 5s1", ElementGroup.Metals,
                10.501f, 1234.93f, 2435f, 1.93f, new double [] { 1, 2, 3 }),

            new(112.41f, "Cd", "Cadmium", 48, "[Kr] 4d10 5s2", ElementGroup.Metals,
                8.69f, 594.22f, 1040f, 1.69f, new double [] { 2 }),

            new(114.82f, "In", "Indium", 49, "[Kr] 4d10 5s2 5p1",
                ElementGroup.Metals, 7.31f, 429.75f, 2345f, 1.78f, new double [] { 3 }),

            new(118.71f, "Sn", "Tin", 50, "[Kr] 4d10 5s2 5p2", ElementGroup.Metals,
                7.287f, 505.08f, 2875f, 1.96f, new double [] { 2, 4 }),

            new(121.76f, "Sb", "Antimony", 51, "[Kr] 4d10 5s2 5p3",
                ElementGroup.Metalloids, 6.685f, 903.78f, 1860f, 2.05f, new double [] { 3, 5 }),

            new(127.60f, "Te", "Tellurium", 52, "[Kr] 4d10 5s2 5p4",
                ElementGroup.Metalloids, 6.232f, 722.66f, 1261f, 2.1f, new double [] { 2, 4, 6 }),

            new(126.90f, "I", "Iodine", 53, "[Kr] 4d10 5s2 5p5",
                ElementGroup.NonMetals, 4.93f, 386.85f, 457.4f, 2.66f, new double [] { 1 }),

            new(131.29f, "Xe", "Xenon", 54, "[Kr] 4d10 5s2 5p6",
                ElementGroup.NonMetals, 5.894f, 161.4f, 165.1f, 0.0f, new double [] { 0 }),

            new(132.91f, "Cs", "Cesium", 55, "[Xe] 6s1", ElementGroup.Metals, 1.873f,
                301.59f, 944f, 0.79f, new double [] { 1 }),

            new(137.33f, "Ba", "Barium", 56, "[Xe] 6s2", ElementGroup.Metals, 3.594f,
                1000f, 2170f, 0.89f, new double [] { 2 }),

            new(138.91f, "La", "Lanthanum", 57, "[Xe] 5d1 6s2",
                ElementGroup.Metals, 6.145f, 1193f, 3737f, 1.10f, new double [] { 3 }),

            new(140.12f, "Ce", "Cerium", 58, "[Xe] 4f1 5d1 6s2", ElementGroup.Metals,
                6.77f, 1068f, 3716f, 1.12f, new double [] { 3, 4 }),

            new(140.91f, "Pr", "Praseodymium", 59, "[Xe] 4f3 6s2",
                ElementGroup.Metals, 6.77f, 1208f, 3793f, 1.13f, new double [] { 3 }),

            new(144.24f, "Nd", "Neodymium", 60, "[Xe] 4f4 6s2",
                ElementGroup.Metals, 7.01f, 1297f, 3347f, 1.14f, new double [] { 3 }),

            new(145f, "Pm", "Promethium", 61, "[Xe] 4f5 6s2",
                ElementGroup.Metals, 7.26f, 1315f, 3273f, 0.0f, new double [] { 3 }),

            new(150.36f, "Sm", "Samarium", 62, "[Xe] 4f6 6s2", ElementGroup.Metals,
                7.52f, 1345f, 2067f, 1.17f, new double [] { 2, 3 }),

            new(151.96f, "Eu", "Europium", 63, "[Xe] 4f7 6s2", ElementGroup.Metals,
                5.243f, 1099f, 1802f, 1.2f, new double [] { 2, 3 }),

            new(157.25f, "Gd", "Gadolinium", 64, "[Xe] 4f7 5d1 6s2",
                ElementGroup.Metals, 7.895f, 1585f, 3546f, 1.20f, new double [] { 3 }),

            new(158.93f, "Tb", "Terbium", 65, "[Xe] 4f9 6s2", ElementGroup.Metals,
                8.229f, 1629f, 3503f, 1.20f, new double [] { 3 }),

            new(162.50f, "Dy", "Dysprosium", 66, "[Xe] 4f10 6s2",
                ElementGroup.Metals, 8.55f, 1680f, 2840f, 1.22f, new double [] { 3 }),

            new(164.93f, "Ho", "Holmium", 67, "[Xe] 4f11 6s2", ElementGroup.Metals,
                8.795f, 1734f, 2993f, 1.23f, new double [] { 3 }),

            new(167.26f, "Er", "Erbium", 68, "[Xe] 4f12 6s2", ElementGroup.Metals,
                9.066f, 1802f, 3141f, 1.24f, new double [] { 3 }),

            new(168.93f, "Tm", "Thulium", 69, "[Xe] 4f13 6s2", ElementGroup.Metals,
                9.321f, 1818f, 2223f, 1.25f, new double [] { 2, 3 }),

            new(173.05f, "Yb", "Ytterbium", 70, "[Xe] 4f14 6s2",
                ElementGroup.Metals, 6.965f, 1097f, 1469f, 1.1f, new double [] { 2, 3 }),

            new(174.97f, "Lu", "Lutetium", 71, "[Xe] 4f14 5d1 6s2",
                ElementGroup.Metals, 9.841f, 1925f, 3675f, 1.27f, new double [] { 3 }),

            new(178.49f, "Hf", "Hafnium", 72, "[Xe] 4f14 5d2 6s2",
                ElementGroup.Metals, 13.31f, 2506f, 4876f, 1.3f, new double [] { 4 }),

            new(180.95f, "Ta", "Tantalum", 73, "[Xe] 4f14 5d3 6s2",
                ElementGroup.Metals, 16.654f, 3290f, 5731f, 1.5f, new double [] { 5 }),

            new(183.84f, "W", "Tungsten", 74, "[Xe] 4f14 5d4 6s2",
                ElementGroup.Metals, 19.25f, 3695f, 5828f, 2.36f, new double [] { 6 }),

            new(186.21f, "Re", "Rhenium", 75, "[Xe] 4f14 5d5 6s2",
                ElementGroup.Metals, 21.02f, 3459f, 5869f, 1.9f, new double [] { 7 }),

            new(190.23f, "Os", "Osmium", 76, "[Xe] 4f14 5d6 6s2",
                ElementGroup.Metals, 22.59f, 3306f, 5285f, 2.2f, new double [] { 4, 6 }),

            new(192.22f, "Ir", "Iridium", 77, "[Xe] 4f14 5d7 6s2",
                ElementGroup.Metals, 22.56f, 2719f, 4701f, 2.2f, new double [] { 3, 4, 6 }),

            new(195.08f, "Pt", "Platinum", 78, "[Xe] 4f14 5d9 6s1",
                ElementGroup.Metals, 21.46f, 2041.4f, 4098f, 2.28f, new double [] { 2, 4 }),

            new(196.97f, "Au", "Gold", 79, "[Xe] 4f14 5d10 6s1", ElementGroup.Metals,
                19.282f, 1337.33f, 3129f, 2.54f, new double [] { 1, 3 }),

            new(200.59f, "Hg", "Mercury", 80, "[Xe] 4f14 5d10 6s2",
                ElementGroup.Metals, 13.5336f, 234.32f, 629.88f, 2.00f, new double [] { 1, 2 }),

            new(204.38f, "Tl", "Thallium", 81, "[Xe] 4f14 5d10 6s2 6p1",
                ElementGroup.Metals, 11.85f, 577f, 1746f, 1.62f, new double [] { 1, 3 }),

            new(207.2f, "Pb", "Lead", 82, "[Xe] 4f14 5d10 6s2 6p2",
                ElementGroup.Metals, 11.342f, 600.61f, 2022f, 2.33f, new double [] { 2, 4 }),

            new(208.98f, "Bi", "Bismuth", 83, "[Xe] 4f14 5d10 6s2 6p3",
                ElementGroup.Metals, 9.807f, 544.7f, 1837f, 2.02f, new double [] { 3, 5 }),

            new(209f, "Po", "Polonium", 84, "[Xe] 4f14 5d10 6s2 6p4",
                ElementGroup.Metals, 9.32f, 527f, 1235f, 2.0f, new double [] { 2, 4, 6 }),

            new(210f, "At", "Astatine", 85, "[Xe] 4f14 5d10 6s2 6p5",
                ElementGroup.NonMetals, 7f, 575f, 610f, 2.2f, new double [] { 1 }),

            new(222f, "Rn", "Radon", 86, "[Xe] 4f14 5d10 6s2 6p6",
                ElementGroup.NonMetals, 9.73f, 202f, 211.3f, 0.0f, new double [] { 0 }),

            new(223f, "Fr", "Francium", 87, "[Rn] 7s1", ElementGroup.Metals, 1.87f,
                300f, 950f, 0.7f, new double [] { 1 }),

            new(226f, "Ra", "Radium", 88, "[Rn] 7s2", ElementGroup.Metals, 5.5f,
                973f, 2010f, 0.9f, new double [] { 2 }),

            new(227f, "Ac", "Actinium", 89, "[Rn] 6d1 7s2", ElementGroup.Metals,
                10.07f, 1323f, 3471f, 1.1f, new double [] { 3 }),

            new(232.04f, "Th", "Thorium", 90, "[Rn] 6d2 7s2", ElementGroup.Metals,
                11.72f, 2023f, 5093f, 1.3f, new double [] { 4 }),

            new(231.04f, "Pa", "Protactinium", 91, "[Rn] 5f2 6d1 7s2",
                ElementGroup.Metals, 15.37f, 1841f, 4300f, 1.5f, new double [] { 5 }),

            new(238.03f, "U", "Uranium", 92, "[Rn] 5f3 6d1 7s2",
                ElementGroup.Metals, 18.95f, 1405.3f, 4404f, 1.38f, new double [] { 6 }),

            new(237f, "Np", "Neptunium", 93, "[Rn] 5f4 6d1 7s2",
                ElementGroup.Metals, 20.45f, 917f, 4175f, 1.36f, new double [] { 5 }),

            new(244f, "Pu", "Plutonium", 94, "[Rn] 5f6 7s2", ElementGroup.Metals,
                19.84f, 912.5f, 3505f, 1.28f, new double [] { 4, 6 }),

            new(243f, "Am", "Americium", 95, "[Rn] 5f7 7s2", ElementGroup.Metals,
                13.67f, 1449f, 2880f, 1.3f, new double [] { 3 }),

            new(247f, "Cm", "Curium", 96, "[Rn] 5f7 6d1 7s2", ElementGroup.Metals,
                13.51f, 1613f, 3383f, 1.3f, new double [] { 3 }),

            new(247f, "Bk", "Berkelium", 97, "[Rn] 5f9 7s2", ElementGroup.Metals,
                14.78f, 1259f, 2900f, 1.3f, new double [] { 3 }),

            new(251f, "Cf", "Californium", 98, "[Rn] 5f10 7s2",
                ElementGroup.Metals, 15.1f, 1173f, 1743f, 1.3f, new double [] { 3 }),

            new(252f, "Es", "Einsteinium", 99, "[Rn] 5f11 7s2",
                ElementGroup.Metals, 8.84f, 1133f, 1269f, 1.3f, new double [] { 3 }),

            new(257f, "Fm", "Fermium", 100, "[Rn] 5f12 7s2", ElementGroup.Metals,
                9.7f, 1800f, 0.0f, 1.3f, new double [] { 3 }),

            new(258f, "Md", "Mendelevium", 101, "[Rn] 5f13 7s2",
                ElementGroup.Metals, 10.3f, 1100f, 0.0f, 1.3f, new double [] { 3 }),

            new(259f, "No", "Nobelium", 102, "[Rn] 5f14 7s2", ElementGroup.Metals,
                9.9f, 1100f, 0.0f, 1.3f, new double [] { 3 }),

            new(262f, "Lr", "Lawrencium", 103, "[Rn] 5f14 7s2 7p1",
                ElementGroup.Metals, 15.6f, 1900f, 0.0f, 1.3f, new double [] { 3 }),

            new(267f, "Rf", "Rutherfordium", 104, "[Rn] 5f14 6d2 7s2",
                ElementGroup.Metals, 23.2f, 2400f, 5800f, 0.0f, new double [] { 4 }),

            new(270f, "Db", "Dubnium", 105, "[Rn] 5f14 6d3 7s2",
                ElementGroup.Metals, 29.3f, 0.0f, 0.0f, 0.0f, new double [] { 5 }),

            new(271f, "Sg", "Seaborgium", 106, "[Rn] 5f14 6d4 7s2",
                ElementGroup.Metals, 35.0f, 0.0f, 0.0f, 0.0f, new double [] { 6 }),

            new(270f, "Bh", "Bohrium", 107, "[Rn] 5f14 6d5 7s2",
                ElementGroup.Metals, 37.1f, 0.0f, 0.0f, 0.0f, new double [] { 7 }),

            new(277f, "Hs", "Hassium", 108, "[Rn] 5f14 6d6 7s2",
                ElementGroup.Metals, 40.7f, 0.0f, 0.0f, 0.0f, new double [] { 8 }),

            new(276f, "Mt", "Meitnerium", 109, "[Rn] 5f14 6d7 7s2",
                ElementGroup.Metals, 37.4f, 0.0f, 0.0f, 0.0f, new double [] { 9 }),

            new(281f, "Ds", "Darmstadtium", 110, "[Rn] 5f14 6d9 7s1",
                ElementGroup.Metals, 34.8f, 0.0f, 0.0f, 0.0f, new double [] { 10 }),

            new(280f, "Rg", "Roentgenium", 111, "[Rn] 5f14 6d10 7s1",
                ElementGroup.Metals, 28.7f, 0.0f, 0.0f, 0.0f, new double [] { 11 }),

            new(285f, "Cn", "Copernicium", 112, "[Rn] 5f14 6d10 7s2",
                ElementGroup.Metals, 23.7f, 0.0f, 0.0f, 0.0f, new double [] { 12 }),

            new(284f, "Nh", "Nihonium", 113, "[Rn] 5f14 6d10 7s2 7p1",
                ElementGroup.Metals, 16.0f, 0.0f, 0.0f, 0.0f, new double [] { 13 }),

            new(289f, "Fl", "Flerovium", 114, "[Rn] 5f14 6d10 7s2 7p2",
                ElementGroup.Metals, 14.0f, 0.0f, 0.0f, 0.0f, new double [] { 14 }),

            new(288f, "Mc", "Moscovium", 115, "[Rn] 5f14 6d10 7s2 7p3",
                ElementGroup.Metals, 13.5f, 0.0f, 0.0f, 0.0f, new double [] { 15 }),

            new(293f, "Lv", "Livermorium", 116, "[Rn] 5f14 6d10 7s2 7p4",
                ElementGroup.Metals, 12.9f, 0.0f, 0.0f, 0.0f, new double [] { 16 }),

            new(294f, "Ts", "Tennessine", 117, "[Rn] 5f14 6d10 7s2 7p5",
                ElementGroup.Metals, 7.17f, 0.0f, 0.0f, 0.0f, new double [] { 17 }),

            new(294f, "Og", "Oganesson", 118, "[Rn] 5f14 6d10 7s2 7p6",
                ElementGroup.Metals, 4.9f, 0.0f, 0.0f, 0.0f, new double [] { 0 })
        };

        public float GetAtomicMass()
        {
            return AtomicMass;
        }

        public int GetAtomicNumber()
        {
            return AtomicNumber;
        }

        public string GetSymbol()
        {
            return Symbol;
        }

        public string GetName()
        {
            return Name;
        }

        public string GetElectronConfiguration()
        {
            return ElectronConfiguration;
        }

        public ElementGroup GetGroup()
        {
            return Group;
        }

        public float GetDensity()
        {
            return Density;
        }

        public float GetMeltingPoint()
        {
            return MeltingPoint;
        }

        public float GetBoilingPoint()
        {
            return BoilingPoint;
        }

        public float GetElectronegativity()
        {
            return Electronegativity;
        }

        public IReadOnlyList<double> GetValences()
        {
            return Valences;
        }
    }
}