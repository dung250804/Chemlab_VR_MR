using UnityEngine;

public class Pipette : ContainerEquipmentBase
{
    [Header("Visual")]
    public GameObject waterStreamPrefab;

    [Header("Plunger")]
    public Transform plunger;
    public float minPlungerY;
    public float maxPlungerY;

    [Header("Smoothing")]
    public float smoothTime = 0.15f;

    // =========================
    // INTERNAL
    // =========================
    private LiquidContainer touchingContainer;
    private Stream waterStream;

    private float targetVolume;
    private float volumeVelocity;
    private float lastVolume;
    private float lastNormalized;

    const float VOLUME_EPSILON = 0.001f;
    const float MAX_LENGTH = 2f;

    private static int _layerMask;
    public static int LayerMaskStatic
    {
        get
        {
            if (_layerMask == 0)
                _layerMask = ~LayerMask.GetMask("Ignore Raycast");
            return _layerMask;
        }
    }

    // =========================
    void Start()
    {
        targetVolume = currentVolume;
        lastVolume = currentVolume;
        lastNormalized = GetNormalizedPlunger();
    }

    void Update()
    {
        HandlePlungerIntent();
        SmoothVolume();
        HandleLiquidTransfer();
    }

    // =========================
    // PLUNGER → INTENT ONLY
    // =========================
    void HandlePlungerIntent()
    {
        float normalized = GetNormalizedPlunger();
        float delta = normalized - lastNormalized;

        if (Mathf.Abs(delta) > 0.0001f)
        {
            float deltaVolume = delta * maxVolume;
            targetVolume = Mathf.Clamp(targetVolume + deltaVolume, 0, maxVolume);
        }

        lastNormalized = normalized;
    }

    float GetNormalizedPlunger()
    {
        float y = plunger.localPosition.y;
        return Mathf.InverseLerp(minPlungerY, maxPlungerY, y);
    }

    // =========================
    // SMOOTH VOLUME
    // =========================
    void SmoothVolume()
    {
        currentVolume = Mathf.SmoothDamp(
            currentVolume,
            targetVolume,
            ref volumeVelocity,
            smoothTime
        );

        if (Mathf.Abs(currentVolume - targetVolume) < VOLUME_EPSILON)
        {
            currentVolume = targetVolume;
            volumeVelocity = 0f;
        }
    }

    // =========================
    // HÚT / ĐỔ
    // =========================
    void HandleLiquidTransfer()
    {
        float deltaVolume = currentVolume - lastVolume;

        if (Mathf.Abs(deltaVolume) < VOLUME_EPSILON)
        {
            StopPour();
            lastVolume = currentVolume;
            return;
        }

        if (deltaVolume > 0)
        {
            TrySuck(deltaVolume);
        }
        else
        {
            TryPour(-deltaVolume);
        }

        lastVolume = currentVolume;
    }

    // =========================
    // HÚT
    // =========================
    void TrySuck(float amount)
    {
        if (touchingContainer == null)
        {
            targetVolume = currentVolume;
            return;
        }

        float sucked = touchingContainer.Drain(amount);

        currentVolume -= (amount - sucked);
        targetVolume = currentVolume;
    }

    // =========================
    // ĐỔ
    // =========================
    void TryPour(float amount)
    {
        LiquidContainer target = DetectLiquidBelow(MAX_LENGTH);

        if (target == null)
        {
            StopPour();
            targetVolume = currentVolume;
            return;
        }

        StartPour();
        target.Fill(amount);
    }

    // =========================
    // STREAM
    // =========================
    void StartPour()
    {
        if (waterStream == null)
            waterStream = CreateStream();

        waterStream.transform.position = spoutCenter.position;

        if (!waterStream.gameObject.activeSelf)
            waterStream.gameObject.SetActive(true);

        waterStream.Begin();
    }

    void StopPour()
    {
        if (waterStream && waterStream.gameObject.activeSelf)
        {
            waterStream.End(() =>
            {
                if (waterStream)
                    waterStream.gameObject.SetActive(false);
            });
        }
    }

    Stream CreateStream()
    {
        GameObject go = Instantiate(
            waterStreamPrefab,
            spoutCenter.position,
            Quaternion.identity,
            transform
        );
        return go.GetComponent<Stream>();
    }

    // =========================
    // DETECT LIQUID
    // =========================
    LiquidContainer DetectLiquidBelow(float length)
    {
        RaycastHit hit;
        Vector3 origin = spoutCenter.position + Vector3.down * 0.01f;
        Ray ray = new Ray(origin, Vector3.down);

        if (Physics.Raycast(ray, out hit, length, LayerMaskStatic))
        {
            return hit.collider.GetComponentInParent<LiquidContainer>();
        }

        return null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (touchingContainer) return;
        var container = other.GetComponentInParent<LiquidContainer>();
        if (container != null)
        {
            touchingContainer = container;
        }
    }

    void OnTriggerExit(Collider other)
    {
        var container = other.GetComponentInParent<LiquidContainer>();
        if (container != null && container == touchingContainer)
        {
            touchingContainer = null;
        }
    }
}
