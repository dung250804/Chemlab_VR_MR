using UnityEngine;

public class RotateInfinity : MonoBehaviour
{
    public Vector3 rotateAxis = new Vector3(0, 1, 0);
    public float baseSpeed = 50f; // tốc độ chuẩn khi scale = 1
    public float minSpeed = 10f;
    public float maxSpeed = 100f;

    public OneGrabDistanceScaleTransformer scaleTransformer;


    void Update()
    {
        float scale = transform.localScale.x; // scale đều XYZ nên lấy x là đủ
        float speed = baseSpeed;
        
        if (scaleTransformer != null)
        {
            // chuyển scale về 0 → 1 dựa trên minScale → maxScale
            float t = Mathf.InverseLerp(scaleTransformer.MinScale, scaleTransformer.MaxScale, scale);
            // đảo ngược vì scale lớn → speed nhỏ
            speed = Mathf.Lerp(maxSpeed, minSpeed, t);

        }
        else
        {
            // scale to -> speed nhỏ, scale nhỏ -> speed lớn
            speed = baseSpeed / scale;
            speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        }

        transform.Rotate(rotateAxis * speed * Time.deltaTime);
    }
}