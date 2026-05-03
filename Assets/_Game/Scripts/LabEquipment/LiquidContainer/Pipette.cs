using com.ethnicthv.chemlab.engine.mixture;
using UnityEngine;

public class Pipette : ContainerEquipmentBase
{
    [Header("Visual")]
    public GameObject waterStreamPrefab;

    [Header("Plunger")]
    public Transform plunger;
    public float minPlungerY;
    public float maxPlungerY;

    // =========================
    // INTERNAL
    // =========================
    private LiquidContainer touchingContainer;
    private Stream waterStream;

    private float targetVolume;
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
        lastNormalized = GetNormalizedPlunger();
    }

    void Update()
    {
        HandlePlungerIntent();
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
    // HÚT / ĐỔ
    // =========================
    void HandleLiquidTransfer()
    {
        float delta = targetVolume - currentVolume;

        if (Mathf.Abs(delta) < VOLUME_EPSILON)
        {
            StopPour();
            return;
        }

        if (delta > 0)
            TrySuck(delta);
        else
            TryPour(-delta);
    }


    // =========================
    // HÚT
    // =========================
    void TrySuck(float amount)
    {
        if (touchingContainer == null)
        {
            targetVolume = currentVolume; // không cho tích nợ
            return;
        }

        float sucked = touchingContainer.Drain(amount);
        Mixture mixture = touchingContainer.GetMixture();
        if (mixture == null) return;

        AddMixture(mixture, sucked);
        NormalizeState();
    }


    // =========================
    // ĐỔ
    // =========================
    void TryPour(float amount)
    {
        if (currentVolume <= 0f)
        {
            currentVolume = 0f;
            return;
        }

        float pourAmount = Mathf.Min(amount, currentVolume);
        currentVolume -= pourAmount;

        LiquidContainer target =
            touchingContainer ? touchingContainer : DetectLiquidBelow(MAX_LENGTH);

        if (pourAmount > 0f)
        {
            StartPour(touchingContainer == null);
            target?.AddMixture(GetMixture(), pourAmount);
        }
    }


    // =========================
    // STREAM
    // =========================
    void StartPour(bool needCreate = true)
    {
        if (!needCreate) return;
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
        other.TryGetComponent<LiquidContainer>(out LiquidContainer container);
        if (container == null)
        {
            container = other.GetComponentInParent<LiquidContainer>();
            if (container == null) return;
        }
        if (container != null)
        {
            touchingContainer = container;
        }
    }

    void OnTriggerExit(Collider other)
    {
        other.TryGetComponent<LiquidContainer>(out LiquidContainer container);
        if (container == null)
        {
            container = other.GetComponentInParent<LiquidContainer>();
            if (container == null) return;
        }
        if (container != null && container == touchingContainer)
        {
            touchingContainer = null;
        }
    }
}
