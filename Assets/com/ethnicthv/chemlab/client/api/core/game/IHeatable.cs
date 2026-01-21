using UnityEngine;

namespace com.ethnicthv.chemlab.client.api.core.game
{
    public interface IHeatable
    {
        public void SetHeater(IHeater heater);
        public void SetHeatPower(float heatPower);
        public bool IsHeating();
        public bool IsAttachedToHeater(out IHeater heater);
    }
}