using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api;

namespace com.ethnicthv.chemlab.engine
{
    public static class ChemicalTickerHandler
    {
        private static readonly List<IChemicalTicker> Mixtures = new();
        
        public static void AddTicker(IChemicalTicker tickerHandler)
        {
            Mixtures.Add(tickerHandler);
        }
        
        public static void RemoveTicker(IChemicalTicker tickerHandler)
        {
            Mixtures.Remove(tickerHandler);
        }
        
        public static void TickAll()
        {
            foreach (var ticker in Mixtures)
            {
                ticker.Tick();
            }
        }
        
    }
}