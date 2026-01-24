using Newtonsoft.Json.Serialization;
using UnityEngine;

public class LiquidContainer : ContainerEquipmentBase 
{
    [Header("Pour Settings")]
    public GameObject waterStreamPrefab;
    public float maxFlowRate = 50f;     // Tăng lên vì logic mới tính theo chênh lệch độ cao

    [Header("Container Geometry")]
    public float containerHeight = 0.2f; // Chiều cao từ đáy lên miệng bình
    public float bottomRadius = 0.05f;   // Bán kính đáy (thường bằng hoặc nhỏ hơn miệng)

    [Header("Debug")]
    public bool showGizmos = true;

    // Các biến trạng thái
    private Stream waterStream;
    private bool isPouring = false;
    private const float MAX_LENGTH = 2f;
    private static int _layerMask;

    public static int LayerMaskStatic
    {
        get
        {
            if (_layerMask == 0) _layerMask = ~LayerMask.GetMask("Ignore Raycast");
            return _layerMask;
        }
    }

    void Update()
    {
        // 1. Xác định điểm rót (Pour Point) trong World Space (Điểm thấp nhất của vành miệng)
        Vector3 flowDirWorld;
        Vector3 pourPoint = GetPourPoint(out flowDirWorld);

        // 2. Tính toán độ cao mực nước ảo (Virtual Water Level)
        Vector3 waterSurfacePos = GetVirtualWaterSurfacePosition(flowDirWorld);

        // 3. So sánh độ cao: Nếu mặt nước cao hơn điểm rót -> Đổ
        // Dùng chênh lệch độ cao để tính áp lực dòng chảy (càng chênh cao chảy càng mạnh)
        float heightDifference = waterSurfacePos.y - pourPoint.y;

        // Xử lý trường hợp lật úp hoàn toàn (> 90 độ), logic này vẫn đúng vì
        // waterSurfacePos sẽ nằm ở đáy bình (lúc này đang ở trên cao), còn pourPoint ở dưới thấp.

        if (heightDifference > 0 && currentVolume > 0)
        {
            // Tính độ mạnh dòng chảy dựa trên độ chênh lệch chiều cao (đơn vị mét)
            // Ví dụ: chênh 5cm (0.05) là chảy khá mạnh
            float flowStrength = Mathf.Clamp01(heightDifference / 0.05f);
            PourWater(flowStrength, pourPoint, flowDirWorld);
        }
        else
        {
            StopPouring();
        }
    }

    public Vector3 GetVirtualWaterSurfacePosition(Vector3 flowDirWorld)
    {
        // 1. Tìm TÂM HÌNH HỌC của phần chứa nước (Cylinder Center)
        // Lấy đỉnh nắp trừ đi một nửa chiều cao
        Vector3 cylinderCenter = transform.TransformPoint(
            transform.InverseTransformPoint(spoutCenter.position) - Vector3.up * (containerHeight * 0.5f)
        );

        // 2. Tính "Độ dày" của bình theo phương thẳng đứng (Vertical Span)
        // - Khi bình đứng (dot = 1): Chiều cao nước trải dài theo containerHeight.
        // - Khi bình nằm ngang (dot = 0): Chiều cao nước trải dài theo đường kính (2 * radius).
        // - Khi nghiêng: Nội suy giữa 2 giá trị này.

        float dot = Mathf.Abs(Vector3.Dot(transform.up, Vector3.up));
        float currentSpan = Mathf.Lerp(bottomRadius * 2f, containerHeight, dot);

        // 3. Tính độ lệch từ tâm (Offset) dựa trên % nước
        // - Nếu 50% nước: offset = 0 (Nước nằm ngay tâm bình -> Luôn đúng mọi góc nghiêng)
        // - Nếu > 50%: offset dương (lên trên)
        // - Nếu < 50%: offset âm (xuống dưới)
        float fillRatio = currentVolume / maxVolume;
        float yOffset = (fillRatio - 0.5f) * currentSpan;

        // 4. Tính độ cao Y của mặt nước
        float waterSurfaceY = cylinderCenter.y + yOffset;

        // Khắc phục lỗi nước đầy trào ra sớm:
        // Nếu bình gần đầy (>90%) và đang nghiêng, ta kìm hãm chiều cao lại một chút
        // để tránh việc nước chạm mép quá nhanh do sai số hình học.
        if (fillRatio > 0.9f && dot < 0.8f)
        {
            waterSurfaceY -= 0.005f; // Giảm nhẹ mức nước xuống (5mm)
        }

        // 5. Trả về vị trí (Giữ X, Z tại tâm bình để dễ tính toán)
        return new Vector3(cylinderCenter.x, waterSurfaceY, cylinderCenter.z);
    }

    void PourWater(float strength, Vector3 pourPoint, Vector3 flowDir)
    {
        isPouring = true;

        float amountToPour = maxFlowRate * strength * Time.deltaTime;
        float actualPour = Mathf.Min(amountToPour, currentVolume);
        currentVolume -= actualPour;

        if (!waterStream)
            waterStream = CreateStream();

        // Cập nhật vị trí dòng chảy
        waterStream.transform.position = pourPoint;
        waterStream.transform.forward = flowDir; // Hướng ra khỏi miệng bình

        if (!waterStream.gameObject.activeSelf)
            waterStream.gameObject.SetActive(true);

        waterStream.Begin();

        // Logic Raycast cũ của bạn giữ nguyên
        DetectHit(actualPour, pourPoint);
    }

    void StopPouring()
    {
        if (waterStream && waterStream.gameObject.activeSelf && isPouring)
        {
            waterStream.End(() =>
            {
                if (waterStream) waterStream.gameObject.SetActive(false);
            });
        }
        isPouring = false;
    }


    Vector3 GetLocalFlowDirXZ()
    {
        // Tìm hướng trọng lực trong Local Space
        Vector3 gravityLocal = transform.InverseTransformDirection(Vector3.down);

        // Chiếu lên mặt phẳng XZ local (mặt cắt ngang của bình)
        Vector3 dir = new Vector3(gravityLocal.x, 0f, gravityLocal.z);

        if (dir.sqrMagnitude < 0.0001f) return Vector3.forward; // Mặc định nếu đứng thẳng

        return dir.normalized; // Hướng này trỏ về phía "thấp nhất" của vành bình trong local
    }

    public Vector3 GetPourPoint(out Vector3 flowDirWorld)
    {
        Vector3 localFlowDir = GetLocalFlowDirXZ();

        // Chuyển hướng đó ra world để dùng cho raycast/stream
        flowDirWorld = transform.TransformDirection(localFlowDir);

        // Tính điểm rót (World Space)
        // Lấy tâm miệng + hướng trũng * bán kính
        Vector3 localPourPoint = spoutCenter.localPosition + localFlowDir * spoutRadius;

        // Đảm bảo Y local khớp với spoutCenter (trường hợp spout center bị lệch)
        localPourPoint.y = spoutCenter.localPosition.y;

        return transform.TransformPoint(localPourPoint);
    }

    // Hàm nhận nước
    public void Fill(float amount)
    {
        currentVolume += amount;
        currentVolume = Mathf.Clamp(currentVolume, 0, maxVolume);
    }

    private Stream CreateStream()
    {
        GameObject go = Instantiate(waterStreamPrefab, transform.position, Quaternion.identity, transform);
        return go.GetComponent<Stream>();
    }

    void DetectHit(float amount, Vector3 pourPoint)
    {
        RaycastHit hit;
        Vector3 rayOrigin = pourPoint + Vector3.down * 0.02f;
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
            var targetCup = hit.collider.GetComponentInParent<LiquidContainer>();
            if (targetCup != null)
            {
                targetCup.Fill(amount);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || spoutCenter == null) return;

        Vector3 flowDirWorld;
        Vector3 pourPoint = GetPourPoint(out flowDirWorld);
        Vector3 waterSurfacePos = GetVirtualWaterSurfacePosition(flowDirWorld);

        // Vẽ điểm rót (Màu đỏ)
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pourPoint, 0.01f);

        // Vẽ mực nước ảo (Màu xanh dương)
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(waterSurfacePos, 0.015f);

        // Vẽ đường nối mức nước (Logic giả lập)
        Vector3 bottomCenterWorld = transform.TransformPoint(transform.InverseTransformPoint(spoutCenter.position) - Vector3.up * containerHeight);
        Vector3 deepestPoint = bottomCenterWorld + (flowDirWorld * bottomRadius);
        Vector3 highestRimPoint = spoutCenter.position - (flowDirWorld * spoutRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(deepestPoint, highestRimPoint); // Đường chéo mực nước

        // Vẽ mặt nước phẳng (minh họa)
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawCube(new Vector3(transform.position.x, waterSurfacePos.y, transform.position.z), new Vector3(0.1f, 0.002f, 0.1f));
    }
}