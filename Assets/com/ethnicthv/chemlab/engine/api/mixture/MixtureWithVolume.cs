using com.ethnicthv.chemlab.engine.mixture;

namespace com.ethnicthv.chemlab.engine.api.mixture
{
    public class MixtureWithVolume
    {
        public Mixture Mixture { get; set; }
        public float Volume { get; set; }
        
        public MixtureWithVolume(Mixture mixture, float volume)
        {
            Mixture = mixture;
            Volume = volume;
        }
    }
}