using System.Collections;
using UnityEngine;

public class ElectronicPipette : ContainerEquipmentBase
{
    [Header("Visual")]
    public GameObject waterStreamPrefab;

    [Header("Electronic Control")]
    public float setVolume = 0f;         // volume người dùng chọn

    [Header("Hold Button")]
    public float holdDelay = 0.4f;       // giữ bao lâu thì bắt đầu auto
    public float holdInterval = 0.08f;   // tốc độ + - khi giữ
    [Header("Electronic Pipette Board")]
    public ElectronicPipetteBoard pipetteBoard;

    private const float MAX_LENGTH = 2f;

    // =========================
    // STEP MODE
    // =========================
    public enum VolumeStep
    {
        ML_1,
        ML_0_1,
        ML_0_01,
        ML_0_001
    }

    public VolumeStep volumeStep = VolumeStep.ML_0_1;

    // =========================
    // INTERNAL
    // =========================
    private LiquidContainer touchingContainer;
    private Stream waterStream;

    private float holdTimer;
    private float repeatTimer;
    private int holdDirection; // +1 or -1

    private static int _layerMask;
    static int LayerMaskStatic
    {
        get
        {
            if (_layerMask == 0)
                _layerMask = ~LayerMask.GetMask("Ignore Raycast");
            return _layerMask;
        }
    }

    void Awake()
    {
        if (pipetteBoard != null)
        {
            pipetteBoard.SetCurrentVolumeText(currentVolume, maxVolume);
            pipetteBoard.SetSetVolumeText(setVolume);
            pipetteBoard.SetStepModeText(volumeStep);

            // Buttons
            pipetteBoard.increaseVolumeButton.WhenSelect.AddListener((evt) =>
            {
                IncreaseOnce();
                HoldIncreaseStart();
            });
            pipetteBoard.increaseVolumeButton.WhenRelease.AddListener((evt) => HoldEnd());

            pipetteBoard.decreaseVolumeButton.WhenSelect.AddListener((evt) =>
            {
                DecreaseOnce();
                HoldDecreaseStart();
            });
            pipetteBoard.decreaseVolumeButton.WhenRelease.AddListener((evt) => HoldEnd());
            
            pipetteBoard.stepModeButton.WhenSelect.AddListener((evt) => CycleStep());
            pipetteBoard.aspirateButton.WhenSelect.AddListener((evt) => Aspirate());
            pipetteBoard.dispenseButton.WhenSelect.AddListener((evt) => Dispense());
        }
    }

    // =========================
    void Update()
    {
        HandleHoldInput();
    }

    // =========================
    // STEP
    // =========================
    float GetStepValue()
    {
        return volumeStep switch
        {
            VolumeStep.ML_1     => 1f,
            VolumeStep.ML_0_1   => 0.1f,
            VolumeStep.ML_0_01  => 0.01f,
            VolumeStep.ML_0_001 => 0.001f,
            _ => 0.1f
        };
    }

    int GetDecimals()
    {
        return volumeStep switch
        {
            VolumeStep.ML_1     => 0,
            VolumeStep.ML_0_1   => 1,
            VolumeStep.ML_0_01  => 2,
            VolumeStep.ML_0_001 => 3,
            _ => 2
        };
    }

    float RoundVolume(float v)
    {
        return (float)System.Math.Round(v, GetDecimals());
    }

    // =========================
    // + / -
    // =========================
    public void IncreaseOnce()
    {
        float step = GetStepValue();
        setVolume = Mathf.Clamp(setVolume + step, 0f, maxVolume);
        setVolume = RoundVolume(setVolume);
        pipetteBoard.SetSetVolumeText(setVolume);
    }

    public void DecreaseOnce()
    {
        float step = GetStepValue();
        setVolume = Mathf.Clamp(setVolume - step, 0f, maxVolume);
        setVolume = RoundVolume(setVolume);
        pipetteBoard.SetSetVolumeText(setVolume);
    }

    // =========================
    // HOLD LOGIC
    // =========================
    void HandleHoldInput()
    {
        if (holdDirection == 0) return;

        holdTimer += Time.deltaTime;

        if (holdTimer < holdDelay) return;

        repeatTimer += Time.deltaTime;
        if (repeatTimer >= holdInterval)
        {
            repeatTimer = 0f;

            if (holdDirection > 0)
                IncreaseOnce();
            else
                DecreaseOnce();
        }
    }

    public void HoldIncreaseStart()
    {
        holdDirection = 1;
        holdTimer = 0f;
        repeatTimer = 0f;
    }

    public void HoldDecreaseStart()
    {
        holdDirection = -1;
        holdTimer = 0f;
        repeatTimer = 0f;
    }

    public void HoldEnd()
    {
        holdDirection = 0;
        holdTimer = 0f;
        repeatTimer = 0f;
    }

    // =========================
    // STEP BUTTON
    // =========================
    public void CycleStep()
    {
        volumeStep = (VolumeStep)(((int)volumeStep + 1) % 4);
        setVolume = RoundVolume(setVolume);
        pipetteBoard.SetStepModeText(volumeStep);
    }

    // =========================
    // ASPIRATE
    // =========================
    public void Aspirate()
    {
        if (touchingContainer == null) return;
        if (currentVolume >= maxVolume) return;

        float need = setVolume - currentVolume;
        if (need <= 0f) return;

        var mixture = touchingContainer.GetMixture();
        if (mixture == null) return;

        float sucked = touchingContainer.Drain(need);
        AddMixture(mixture, sucked);

        pipetteBoard.SetCurrentVolumeText(currentVolume, maxVolume);
        NormalizeState();
    }

    // =========================
    // DISPENSE
    // =========================
    public void Dispense()
    {
        if (currentVolume <= 0f) return;

        LiquidContainer target = touchingContainer ? touchingContainer : DetectLiquidBelow(MAX_LENGTH);

        float amount = Mathf.Min(setVolume, currentVolume);

        var mixture = GetMixture();
        if (mixture == null) return;

        StartPour(touchingContainer == null);
        target?.AddMixture(mixture, amount);
        currentVolume -= amount;
        currentVolume = Mathf.Max(currentVolume, 0f);
        pipetteBoard.SetCurrentVolumeText(currentVolume, maxVolume);
        StartCoroutine(DelayedStopPour(0.2f));
        NormalizeState();
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
    // RAYCAST
    // =========================
    LiquidContainer DetectLiquidBelow(float length)
    {
        RaycastHit hit;
        Ray ray = new Ray(spoutCenter.position, Vector3.down);

        if (Physics.Raycast(ray, out hit, length, LayerMaskStatic))
            return hit.collider.GetComponentInParent<LiquidContainer>();

        return null;
    }

    // =========================
    // TRIGGER
    // =========================
    void OnTriggerEnter(Collider other)
    {
        if (touchingContainer) return;
        touchingContainer = other.GetComponentInParent<LiquidContainer>();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<LiquidContainer>() == touchingContainer)
            touchingContainer = null;
    }

    // =========================
    // UI TEXT (OPTIONAL)
    // =========================
    public string GetSetVolumeText()
    {
        return volumeStep switch
        {
            VolumeStep.ML_1     => $"{setVolume:0} ml",
            VolumeStep.ML_0_1   => $"{setVolume:0.0} ml",
            VolumeStep.ML_0_01  => $"{setVolume:0.00} ml",
            VolumeStep.ML_0_001 => $"{setVolume:0.000} ml",
            _ => $"{setVolume} ml"
        };
    }

    private IEnumerator DelayedStopPour(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopPour();
    }
}
