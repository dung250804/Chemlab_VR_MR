using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoobleLiquid : MonoBehaviour
{
    public enum UpdateMode { Normal, UnscaledTime }
    public UpdateMode updateMode;
    [SerializeField] ContainerEquipmentBase liquidContainer;
    [SerializeField] float MaxWobble = 0.03f;
    [SerializeField] float WobbleSpeedMove = 1f;
    [SerializeField] float Recovery = 1f;
    [SerializeField] float Thickness = 1f;
    [Range(0, 1)] public float CompensateShapeAmount;
    [SerializeField] Mesh mesh;
    [SerializeField] Renderer rend;
    Material mat;
    Vector3 pos;
    Vector3 lastPos;
    Vector3 velocity;
    Quaternion lastRot;
    Vector3 angularVelocity;
    float wobbleAmountX;
    float wobbleAmountZ;
    float wobbleAmountToAddX;
    float wobbleAmountToAddZ;
    float pulse;
    float sinewave;
    float time = 0.5f;
    [SerializeField] float minFillAmount = 0f;
    [SerializeField] float maxFillAmount = 1f;
    [SerializeField]float fillAmount = 0f;
    Vector3 comp;

    Color currentTint;
    Color currentTop;
    Color currentRim;

    private float colorLerpSpeed = 5f;
 
    // Use this for initialization
    void Start()
    {
        GetMeshAndRend();
        mat = rend.material;

        if (liquidContainer != null && liquidContainer.currentVolume > 0 && liquidContainer.GetMixture() != null)
        {
            currentTint = liquidContainer.GetMixture().GetColor();

            CalculateLiquidColors(currentTint, out currentTop, out currentRim);
            
            mat.SetColor("_BottomColor", currentTint);
            mat.SetColor("_TopColor", currentTop);
            mat.SetColor("_Rim_Color", currentRim);
        }
        Debug.Log(currentTint);
    }
 
    private void OnValidate()
    {
        GetMeshAndRend();
    }
 
    void GetMeshAndRend()
    {
        if (mesh == null)
        {
            mesh = GetComponent<MeshFilter>().sharedMesh;
        }
        if (rend == null)
        {
            rend = GetComponent<Renderer>();
        }
    }
    void Update()
    {
        float deltaTime = 0;
        switch (updateMode)
        {
            case UpdateMode.Normal:
                deltaTime = Time.deltaTime;
                break;
 
            case UpdateMode.UnscaledTime:
                deltaTime = Time.unscaledDeltaTime;
                break;
        }
        float inverseLerp = Mathf.InverseLerp(0f, liquidContainer.maxVolume, liquidContainer.currentVolume);
        fillAmount = 1f - Mathf.Lerp(minFillAmount, maxFillAmount, inverseLerp);
        if (liquidContainer.currentVolume <= 0)
        {
            fillAmount = 1;
            UpdatePos(deltaTime);
            return;
        }
    
        time += deltaTime;
 
        if (deltaTime != 0)
        {
 
 
            // decrease wobble over time
            wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, (deltaTime * Recovery));
            wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, (deltaTime * Recovery));
 
 
 
            // make a sine wave of the decreasing wobble
            pulse = 2 * Mathf.PI * WobbleSpeedMove;
            sinewave = Mathf.Lerp(sinewave, Mathf.Sin(pulse * time), deltaTime * Mathf.Clamp(velocity.magnitude + angularVelocity.magnitude, Thickness, 10));
 
            wobbleAmountX = wobbleAmountToAddX * sinewave;
            wobbleAmountZ = wobbleAmountToAddZ * sinewave;
 
 
 
            // velocity
            velocity = (lastPos - transform.position) / deltaTime;
 
            angularVelocity = GetAngularVelocity(lastRot, transform.rotation);
 
            // add clamped velocity to wobble
            wobbleAmountToAddX += Mathf.Clamp((velocity.x + (velocity.y * 0.2f) + angularVelocity.z + angularVelocity.y) * MaxWobble, -MaxWobble, MaxWobble);
            wobbleAmountToAddZ += Mathf.Clamp((velocity.z + (velocity.y * 0.2f) + angularVelocity.x + angularVelocity.y) * MaxWobble, -MaxWobble, MaxWobble);
        }
 
        // send it to the shader
        mat.SetFloat("_WobbleX", wobbleAmountX);
        mat.SetFloat("_WobbleZ", wobbleAmountZ);
 
        // set fill amount
        UpdatePos(deltaTime);
 
        // keep last position
        lastPos = transform.position;
        lastRot = transform.rotation;

        if (liquidContainer != null && liquidContainer.currentVolume > 0 && liquidContainer.GetMixture() != null)
        {
            Color targetColor = liquidContainer.GetMixture().GetColor();

            // Tint = màu thật
            currentTint = Color.Lerp(currentTint, targetColor, Time.deltaTime * colorLerpSpeed);

            CalculateLiquidColors(targetColor, out Color topTarget, out Color rimTarget);
            currentTop = Color.Lerp(currentTop, topTarget, Time.deltaTime * colorLerpSpeed);

            currentRim = Color.Lerp(currentRim, rimTarget, Time.deltaTime * colorLerpSpeed);

            mat.SetColor("_BottomColor", currentTint);
            mat.SetColor("_TopColor", currentTop);
            mat.SetColor("_Rim_Color", currentRim);
        }
    }
 
    void UpdatePos(float deltaTime)
    {
 
        Vector3 worldPos = transform.TransformPoint(new Vector3(mesh.bounds.center.x, mesh.bounds.center.y, mesh.bounds.center.z));
        if (CompensateShapeAmount > 0)
        {
            // only lerp if not paused/normal update
            if (deltaTime != 0)
            {
                comp = Vector3.Lerp(comp, (worldPos - new Vector3(0, GetLowestPoint(), 0)), deltaTime * 10);
            }
            else
            {
                comp = (worldPos - new Vector3(0, GetLowestPoint(), 0));
            }
 
            pos = worldPos - transform.position - new Vector3(0, fillAmount - (comp.y * CompensateShapeAmount), 0);
        }
        else
        {
            pos = worldPos - transform.position - new Vector3(0, fillAmount, 0);
        }
        mat.SetVector("_FillAmount", pos);
    }
 
    //https://forum.unity.com/threads/manually-calculate-angular-velocity-of-gameobject.289462/#post-4302796
    Vector3 GetAngularVelocity(Quaternion foreLastFrameRotation, Quaternion lastFrameRotation)
    {
        var q = lastFrameRotation * Quaternion.Inverse(foreLastFrameRotation);
        // no rotation?
        // You may want to increase this closer to 1 if you want to handle very small rotations.
        // Beware, if it is too close to one your answer will be Nan
        if (Mathf.Abs(q.w) > 1023.5f / 1024.0f)
            return Vector3.zero;
        float gain;
        // handle negatives, we could just flip it but this is faster
        if (q.w < 0.0f)
        {
            var angle = Mathf.Acos(-q.w);
            gain = -2.0f * angle / (Mathf.Sin(angle) * Time.deltaTime);
        }
        else
        {
            var angle = Mathf.Acos(q.w);
            gain = 2.0f * angle / (Mathf.Sin(angle) * Time.deltaTime);
        }
        Vector3 angularVelocity = new Vector3(q.x * gain, q.y * gain, q.z * gain);
 
        if (float.IsNaN(angularVelocity.z))
        {
            angularVelocity = Vector3.zero;
        }
        return angularVelocity;
    }
 
    float GetLowestPoint()
    {
        float lowestY = float.MaxValue;
        Vector3 lowestVert = Vector3.zero;
        Vector3[] vertices = mesh.vertices;
 
        for (int i = 0; i < vertices.Length; i++)
        {
 
            Vector3 position = transform.TransformPoint(vertices[i]);
 
            if (position.y < lowestY)
            {
                lowestY = position.y;
                lowestVert = position;
            }
        }
        return lowestVert.y;
    }

    public static void CalculateLiquidColors(Color tint, out Color topColor, out Color rimColor)
    {
        float h, s, v;
        Color.RGBToHSV(tint, out h, out s, out v);

        // Ta ép mức Saturation tối thiểu để dùng làm gốc tính toán cho Top và Rim.
        float baseSat = Mathf.Max(s, 0.4f); 

        // ==========================================
        // 1. TÍNH TOP COLOR (Màu bề mặt chất lỏng)
        // ==========================================
        float topHue = Mathf.Repeat(h + 0.08f, 1f); 
        
        // ÉP RỰC MÀU: Tăng độ bão hòa lên gấp rưỡi để bề mặt có màu rõ ràng, không bị trắng lóa.
        float topSat = Mathf.Clamp01(baseSat * 1.5f); 
        
        // GIỮ ĐỘ SÁNG AN TOÀN: Chỉ cho sáng bằng màu gốc hoặc nhích lên một tí ti, tuyệt đối không vượt 1.0.
        float topVal = Mathf.Clamp01(v * 1.05f); 
        
        topColor = Color.HSVToRGB(topHue, topSat, topVal);


        // ==========================================
        // 2. TÍNH RIM COLOR (Màu viền phản quang)
        // ==========================================
        float rimHue = Mathf.Repeat(h - 0.08f, 1f);
        
        // TỐI ĐA ĐỘ RỰC: Viền HDR phải có màu sắc đậm đặc nhất có thể. Ép cứng lên 1.0 (100% Saturation).
        float rimSat = 1.0f; 
        
        float rimVal = Mathf.Clamp(v * 1.5f, 1.2f, 2.5f); 

        rimColor = Color.HSVToRGB(rimHue, rimSat, rimVal);
    }
}