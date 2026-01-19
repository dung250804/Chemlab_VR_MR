using UnityEngine;

public abstract class LabEquipmentBase : MonoBehaviour
{
    public bool isGrabbable = true;
    public float weightGram = 80f;
    public virtual void OnGrab() {}
    public virtual void OnRelease() {}
    public virtual float GetRawWeightGram()
    {
        return weightGram;
    }
}
