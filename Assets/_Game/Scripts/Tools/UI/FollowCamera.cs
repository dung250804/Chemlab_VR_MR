using UnityEngine;
public class FollowCamera : MonoBehaviour
{
    public Camera targetCamera;
    public float smoothTime = 8f;
    public bool reverseDirection = false;

    [Header("Lock Axis (giữ nguyên góc quay theo từng trục)")]
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;

    void LateUpdate()
    {
        Camera cam = targetCamera ? targetCamera : Camera.main;
        if (cam == null) return;

        // Vector từ UI → Camera
        Vector3 direction = transform.position - cam.transform.position;
        if (reverseDirection)
            direction = -direction;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        // Rotation mục tiêu
        Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);

        // Xoay mượt
        Quaternion smoothRot = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * smoothTime
        );

        // Lấy Euler để lock trục
        Vector3 euler = smoothRot.eulerAngles;
        Vector3 original = transform.rotation.eulerAngles;

        if (lockX) euler.x = original.x;
        if (lockY) euler.y = original.y;
        if (lockZ) euler.z = original.z;

        transform.rotation = Quaternion.Euler(euler);
    }
}
