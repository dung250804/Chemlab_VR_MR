using UnityEngine;
using Oculus.Interaction;

public class OneGrabDistanceScaleTransformer : MonoBehaviour, ITransformer
{
    [SerializeField, Tooltip("Tốc độ scale nhanh hay chậm. Mặc định là 1.")]
    private float scaleSpeed = 1f;

    [SerializeField, Tooltip("Giới hạn scale nhỏ nhất và lớn nhất")]
    private Vector2 minMaxScale = new Vector2(0.5f, 3f);

    private IGrabbable _grabbable;
    private Transform _targetTransform;
    
    private Vector3 _initialScale;
    private float _initialDistance;

    public void Initialize(IGrabbable grabbable)
    {
        _grabbable = grabbable;
        _targetTransform = grabbable.Transform;
    }

    public void BeginTransform()
    {
        // Lấy vị trí tay cầm hiện tại (điểm Grab đầu tiên)
        Pose grabPoint = _grabbable.GrabPoints[0];
        
        // Lưu lại scale và khoảng cách ban đầu từ tay đến tâm vật thể
        _initialScale = _targetTransform.localScale;
        _initialDistance = Vector3.Distance(grabPoint.position, _targetTransform.position);
    }

    public void UpdateTransform()
    {
        Pose currentGrabPoint = _grabbable.GrabPoints[0];
        
        // Tính khoảng cách mới
        float currentDistance = Vector3.Distance(currentGrabPoint.position, _targetTransform.position);
        
        if (_initialDistance > 0.001f) // Tránh chia cho 0
        {
            // Tính tỷ lệ thay đổi (có nhân thêm tốc độ scale nếu muốn)
            float ratio = 1f + ((currentDistance / _initialDistance) - 1f) * scaleSpeed;
            
            // Tính toán scale mới đồng đều ở cả 3 trục (Uniform Scale)
            Vector3 newScale = _initialScale * ratio;

            // Giới hạn scale để vật không quá to hoặc quá nhỏ
            float clampedX = Mathf.Clamp(newScale.x, minMaxScale.x, minMaxScale.y);
            float clampedY = Mathf.Clamp(newScale.y, minMaxScale.x, minMaxScale.y);
            float clampedZ = Mathf.Clamp(newScale.z, minMaxScale.x, minMaxScale.y);

            _targetTransform.localScale = new Vector3(clampedX, clampedY, clampedZ);
        }
    }

    public void EndTransform()
    {
        // Reset logic nếu cần khi nhả tay ra
    }

    public float MinScale => minMaxScale.x;
    public float MaxScale => minMaxScale.y;
}