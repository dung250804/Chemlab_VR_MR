using com.ethnicthv.chemlab.engine.mixture;

namespace com.ethnicthv.chemlab.engine.api.mixture
{
    public record Phases(Mixture GasMixture, float GasVolume, Mixture LiquidMixture, float LiquidVolume);
}