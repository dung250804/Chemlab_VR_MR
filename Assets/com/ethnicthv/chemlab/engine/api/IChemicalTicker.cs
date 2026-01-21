namespace com.ethnicthv.chemlab.engine.api
{
    public interface IChemicalTicker
    {
        public void Tick();
        public void RegisterSelf()
        {
            ChemicalTickerHandler.AddTicker(this);
        }
        
        public void UnregisterSelf()
        {
            ChemicalTickerHandler.RemoveTicker(this);
        }
    }
}