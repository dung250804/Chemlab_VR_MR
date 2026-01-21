using Newtonsoft.Json.Serialization;
using UnityEngine;

public class LiquidContainer : ContainerEquipmentBase
{
    [Header("Pour")]
    public GameObject waterStreamPrefab; 
    public float maxFlowRate = 10f;     // Tốc độ chảy tối đa (ml/giây)
    
    // Góc nghiêng bắt đầu đổ khi bình đầy (ví dụ 45 độ)
    public float minTiltAngle = 45f;    
    // Góc nghiêng để nước đổ ồng ộc tối đa (ví dụ 90 độ)
    public float maxTiltAngle = 90f;    

    private Stream waterStream; 
    private bool isPouring = false;
    private const float MAX_LENGTH = 2f;
    private static int _layerMask;

    public static int LayerMaskStatic
    {
        get
        {
            if (_layerMask == 0)
            {
                _layerMask = ~LayerMask.GetMask("Ignore Raycast");
            }
            return _layerMask;
        }
    }

    void Update()
    {
        // 1. Tính toán góc nghiêng của bình so với phương thẳng đứng
        // Vector3.down là hướng trọng lực, transform.up là hướng đáy lên nắp bình
        float tilt = Vector3.Angle(Vector3.up, transform.up);

        // 2. Tính toán ngưỡng đổ dựa trên lượng nước còn lại
        // Nếu bình đầy, chỉ cần nghiêng nhẹ là đổ. Nếu vơi, phải nghiêng nhiều hơn.
        float normalizedVolume = currentVolume / maxVolume;
        
        // Công thức nội suy: Bình càng vơi, góc cần nghiêng càng lớn
        float dynamicTiltThreshold = Mathf.Lerp(maxTiltAngle, minTiltAngle, normalizedVolume);

        // 3. Xử lý logic đổ
        if (tilt > dynamicTiltThreshold && currentVolume > 0)
        {
            // Tính toán độ mạnh của dòng chảy dựa trên độ nghiêng dư ra
            float tiltDelta = tilt - dynamicTiltThreshold;
            float flowStrength = Mathf.Clamp01(tiltDelta / 20f); // 20 độ dư ra là max dòng chảy

            PourWater(flowStrength);
        }
        else
        {
            StopPouring();
        }
    }

    void PourWater(float strength)
    {
        isPouring = true;

        float amountToPour = maxFlowRate * strength * Time.deltaTime;
        float actualPour = Mathf.Min(amountToPour, currentVolume);
        currentVolume -= actualPour;

        if (!waterStream)
            waterStream = CreateStream();

        Vector3 flowDir;
        Vector3 pourPoint = GetPourPoint(out flowDir);

        waterStream.transform.position = pourPoint;
        waterStream.transform.forward = flowDir;

        waterStream.gameObject.SetActive(true);
        waterStream.Begin();

        DetectHit(actualPour, pourPoint);
    }


    void StopPouring()
    {
        // Tắt dòng nước
        if (waterStream && waterStream.gameObject.activeSelf && isPouring)
        {
            waterStream.End(() => 
            {
                waterStream.gameObject.SetActive(false);
            });
        }
        isPouring = false;
    }

    void DetectHit(float amount, Vector3 pourPoint)
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.down * 0.02f;
        Ray ray = new Ray(rayOrigin, Vector3.down);
        
        if (Physics.Raycast(ray, out hit, MAX_LENGTH, LayerMaskStatic))
        {
            while (transform.IsChildOf(hit.collider.transform))
            {
                rayOrigin += Vector3.down * 0.02f;
                ray = new Ray(rayOrigin, Vector3.down);
                if (!Physics.Raycast(ray, out hit, MAX_LENGTH, LayerMaskStatic))
                break;
            }
            var targetCup = hit.collider.GetComponent<LiquidContainer>();
            if (targetCup != null)
            {
                targetCup.Fill(amount);
            }
        }
    }


    // Hàm để nhận nước (nếu bình này là cái cốc hứng)
    public void Fill(float amount)
    {
        currentVolume += amount;
        currentVolume = Mathf.Clamp(currentVolume, 0, maxVolume);
    }

    private Stream CreateStream()
    {
        GameObject gameObject = Instantiate(waterStreamPrefab.gameObject, transform.position, Quaternion.identity, transform);
        return gameObject.GetComponent<Stream>();
    }

    Vector3 GetLocalFlowDirXZ()
    {
        Vector3 upInLocal = transform.InverseTransformDirection(Vector3.up);

        // lấy nghiêng XZ
        Vector3 dir = new Vector3(upInLocal.x, 0f, upInLocal.z);

        if (dir.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        // nước chảy ngược hướng nghiêng
        return -dir.normalized;
    }

    Vector3 GetPourPoint(out Vector3 flowDirWorld)
    {
        // local
        Vector3 localFlowDir = GetLocalFlowDirXZ();

        // vị trí local của nước chảy
        Vector3 localPourPoint =
            spoutCenter.localPosition + localFlowDir * spoutRadius;

        // giữ Y = Y của spoutCenter
        localPourPoint.y = spoutCenter.localPosition.y;

        // world
        flowDirWorld = transform.TransformDirection(localFlowDir);
        return transform.TransformPoint(localPourPoint);
    }

    

    void OnDrawGizmosSelected()
    {
        if (spoutCenter == null) return;
        Vector3 localDir = GetLocalFlowDirXZ();
        Vector3 worldDir = transform.TransformDirection(localDir);

        Vector3 center = transform.TransformPoint(spoutCenter.localPosition);
        Vector3 p = center + worldDir * spoutRadius;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(p, 0.005f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(center, p);
    }


}