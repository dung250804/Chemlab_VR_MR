using com.ethnicthv.chemlab.client.api.core.game;

namespace com.ethnicthv.chemlab.client.game.util
{
    public abstract class HeatingUtil
    {
        public static void AttachHeater(IHeatable heatable, IHeater heater)
        {
            heater.SetHeatable(heatable);
            heatable.SetHeater(heater);
        }
        
        public static void DetachHeater(IHeatable heatable, IHeater heater)
        {
            heater.SetHeatable(null);
            heatable.SetHeater(null);
            heatable.SetHeatPower(0f);
        }
    }
}