using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.core.game;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.core.game
{
    public class InstrumentManager
    {
        private static readonly Dictionary<GameObject, IInstrument> Instruments = new();
        
        public static void AddInstrument(GameObject gameObject, IInstrument instrument)
        {
            Instruments.Add(gameObject, instrument);
        }
        
        public static void RemoveInstrument(GameObject gameObject)
        {
            Instruments.Remove(gameObject);
        }
        
        public static bool TryGetInstrument(GameObject gameObject, out IInstrument instrument)
        {
            return Instruments.TryGetValue(gameObject, out instrument);
        }
    }
}