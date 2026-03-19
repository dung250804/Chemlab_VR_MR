using UnityEngine;

public abstract class ContainerEquipmentBase : LabEquipmentBase
{
    [Header("Settings")]
    public float maxVolume = 100f;      // Thể tích tối đa (ml)
    public float currentVolume = 0;     // Thể tích hiện tại
    public Transform spoutCenter;       // Vị trí TÂM miệng bình (nơi nước chảy ra)
    public float spoutRadius;           // Bán kính của miệng bình
    public float calciusTemp = 25f;           // Nhiệt độ của chất lỏng bên trong (°C)

    public void AddHeat(float deltaTemp)
    {
        calciusTemp += deltaTemp;
    }
}
