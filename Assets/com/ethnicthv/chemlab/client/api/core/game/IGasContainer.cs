using com.ethnicthv.chemlab.engine.mixture;

namespace com.ethnicthv.chemlab.client.api.core.game
{
    public interface IGasContainer
    {
        public Mixture GetGasMixture();
        public void SetGasMixture(Mixture mixture);
        public void AddGasMixture(Mixture mixture, float volume);
        public bool TryGetGasMixture(out Mixture mixture);
        public bool HasGasMixture();
        public float GetGasVolume();
    }
}