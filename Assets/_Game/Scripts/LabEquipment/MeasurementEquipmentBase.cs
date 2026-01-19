using UnityEngine;

public abstract class MeasurementEquipmentBase : LabEquipmentBase
{
    protected float measuredValue;
    public abstract float Measure();
}
