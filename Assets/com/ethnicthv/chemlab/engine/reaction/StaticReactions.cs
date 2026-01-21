using com.ethnicthv.chemlab.engine.molecule;

namespace com.ethnicthv.chemlab.engine.reaction
{
    public class StaticReactions
    {
        public static readonly ReactingReaction AcidBaseNeutralization;
        
        public static readonly ReactingReaction SodiumDissolution;

        public static readonly ReactingReaction CopperDissolution;
        
        static StaticReactions()
        {
            AcidBaseNeutralization = ReactingReaction.CreateBuilder()
                .ID("acid_base_neutralization")
                .AddReactant(Molecules.Hydroxide, 1, 1)
                .AddReactant(Molecules.Proton, 1, 1)
                .AddProduct(Molecules.Water)
                .PreexponentialFactor(500F)
                .Build();
            
            SodiumDissolution = ReactingReaction.CreateBuilder()
                .ID("sodium_dissolution")
                .AddReactant(Molecules.Sodium, 2, 1)
                .AddReactant(Molecules.Water, 2, 1)
                .AddProduct(Molecules.SodiumIon, 2)
                .AddProduct(Molecules.Hydroxide, 2)
                .AddProduct(Molecules.Hydrogen)
                .PreexponentialFactor(1.0F)
                .ActivationEnergy(1f)
                .Build();
            
            CopperDissolution = ReactingReaction.CreateBuilder()
                .ID("copper_dissolution")
                .AddReactant(Molecules.Copper, 1, 1)
                .AddReactant(Molecules.Proton, 2, 1)
                .AddProduct(Molecules.Copper2Ion, 1)
                .AddProduct(Molecules.Hydrogen, 1)
                .ActivationEnergy(1f)
                .PreexponentialFactor(1.0F)
                .Build();
            
            
            ReactingReaction.CreateBuilder().Acid(Molecules.AceticAcid, Molecules.Acetate, 4.76f);
            ReactingReaction.CreateBuilder().Acid(Molecules.Ammonium, Molecules.Ammonia, 9.25f);
            ReactingReaction.CreateBuilder().Acid(Molecules.HydrochloricAcid, Molecules.Chloride, -6.3f);
            // ReactingReaction.CreateBuilder().Acid(Molecules.HYDROFLUORIC_ACID, Molecules.FLUORIDE, 3.17f);
            // ReactingReaction.CreateBuilder().Acid(Molecules.HYDROGEN_CYANIDE, Molecules.CYANIDE, 9.2f);
            // ReactingReaction.CreateBuilder().Acid(Molecules.HYDROGEN_IODIDE, Molecules.IODIDE, -9.3f);
            ReactingReaction.CreateBuilder().Acid(Molecules.Hydrogensulfate, Molecules.Sulfate, 1.99f);
            // ReactingReaction.CreateBuilder().Acid(Molecules.HYPOCHLOROUS_ACID, Molecules.HYPOCHLORITE, 7.53f);
            // ReactingReaction.CreateBuilder().Acid(Molecules.NITRIC_ACID, Molecules.NITRATE, -1.3f);
            ReactingReaction.CreateBuilder().Acid(Molecules.SulfuricAcid, Molecules.Hydrogensulfate, -2.18f);
        }
    }
}