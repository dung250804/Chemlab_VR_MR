using System.Collections.Generic;
using Oculus.Interaction;
using TMPro;
using UnityEngine;

public class ElectricHeater : LabEquipmentBase
{
    public enum HeaterUnit
    {
        Celcius,    // °C
        Kelvin      // K
    }

    [Header("Power Settings")]
    [SerializeField] private bool isPowerOn;
    
    [Tooltip("Công suất tối đa của bếp (Jun/s hoặc Watt)")]
    [SerializeField] private float maxHeatPower = 2000f; 
    
    [Tooltip("Hệ số truyền nhiệt. Chênh lệch nhiệt độ càng lớn, truyền càng nhanh.")]
    [SerializeField] private float heatTransferRate = 20f; 

    [Header("Unit")]
    [SerializeField] private HeaterUnit currentUnit = HeaterUnit.Celcius;

    [Header("Buttons")]
    [SerializeField] private PointableUnityEventWrapper powerButton;
    [SerializeField] private PointableUnityEventWrapper modeButton;
    [SerializeField] private PointableUnityEventWrapper increaseButton;
    [SerializeField] private PointableUnityEventWrapper decreaseButton;

    [Header("Hold Button Setting")]
    public float holdDelay = 0.4f;       // giữ bao lâu thì bắt đầu auto
    public float holdInterval = 0.08f;   // tốc độ + - khi giữ

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI displayTempText;
    [SerializeField] private TextMeshProUGUI displayUnitText;
    [SerializeField] private TextMeshProUGUI displayOText;

    [Header("Heating Setting")]
    [SerializeField] private float heatingSpeed = 80f;      // °C / giây khi tăng
    [SerializeField] private float coolingSpeed = 60f;      // °C / giây khi giảm
    [SerializeField] private float roomTemp = 25f;

    private const int MIN_CELCIUS_TEMP = 25;
    private const int MAX_CELCIUS_TEMP = 300;

    private readonly List<ContainerEquipmentBase> objectsOnPlate = new();
    [Header("Temperature Debug")]
    public int targetCelciusTemp = 0;
    public float currentCelciusTemp = 0;

    private float holdTimer;
    private float repeatTimer;
    private int holdDirection; // +1 or -1

    void Awake()
    {
        displayTempText.gameObject.SetActive(isPowerOn);
        displayUnitText.gameObject.SetActive(isPowerOn);
        displayOText.gameObject.SetActive(isPowerOn);

        powerButton.WhenSelect.AddListener(OnPowerSelected);
        modeButton.WhenSelect.AddListener(OnModeSelected);
        increaseButton.WhenSelect.AddListener((evt) =>
        {
            IncreaseOnce();
            HoldIncreaseStart();
        });
        increaseButton.WhenRelease.AddListener((evt) => HoldEnd());

        decreaseButton.WhenSelect.AddListener((evt) =>
        {
            DecreaseOnce();
            HoldDecreaseStart();
        });
        decreaseButton.WhenRelease.AddListener((evt) => HoldEnd());
    }

    void Update()
    {
        HandleHoldInput();
        HandleHeating();
    }

    private void HandleHeating()
    {
        float target = isPowerOn ? targetCelciusTemp : MIN_CELCIUS_TEMP;

        if (Mathf.Approximately(currentCelciusTemp, target))
        {
            ApplyHeatToContainer();
            return;
        }

        if (currentCelciusTemp < target)
        {
            currentCelciusTemp += heatingSpeed * Time.deltaTime;
            if (currentCelciusTemp > target)
                currentCelciusTemp = (int)target;
        }
        else
        {
            currentCelciusTemp -= coolingSpeed * Time.deltaTime;
            if (currentCelciusTemp < target)
                currentCelciusTemp = (int)target;
        }

        ApplyHeatToContainer();
    }

    private void HandleHoldInput()
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

    private void IncreaseOnce()
    {
        targetCelciusTemp = Mathf.Clamp(targetCelciusTemp + 1, MIN_CELCIUS_TEMP, MAX_CELCIUS_TEMP);
        SetSetTempText();
    }

    private void DecreaseOnce()
    {
        targetCelciusTemp = Mathf.Clamp(targetCelciusTemp - 1, MIN_CELCIUS_TEMP, MAX_CELCIUS_TEMP);
        SetSetTempText();
    }

    private void HoldIncreaseStart()
    {
        holdDirection = 1;
        holdTimer = 0f;
        repeatTimer = 0f;
    }

    private void HoldDecreaseStart()
    {
        holdDirection = -1;
        holdTimer = 0f;
        repeatTimer = 0f;
    }

    private void HoldEnd()
    {
        holdDirection = 0;
        holdTimer = 0f;
        repeatTimer = 0f;
    }

    private void SetSetTempText()
    {
        if (currentUnit == HeaterUnit.Celcius)
        {
            displayTempText.SetText(targetCelciusTemp.ToString());
        }
        else
        {
            displayTempText.SetText(GetTargetKelvinTemp().ToString());
        }
    }

    private int GetTargetKelvinTemp()
    {
        return targetCelciusTemp + 273;
    }

    private float GetCurrentKelvinTemp()
    {
        return currentCelciusTemp + 273.15f;
    }

    private void OnPowerSelected(PointerEvent evt)
    {
        TogglePower();
    }

    private void OnModeSelected(PointerEvent evt)
    {
        NextMode();
    }

    private void NextMode()
    {
        if (!isPowerOn) return; 

        currentUnit++;

        if ((int)currentUnit >= System.Enum.GetValues(typeof(HeaterUnit)).Length)
            currentUnit = 0;

        displayUnitText.SetText(currentUnit == HeaterUnit.Celcius ? "C" : "K");
        SetSetTempText();
    }

    private void TogglePower()
    {
        isPowerOn = !isPowerOn;

        if (!isPowerOn)
        {
            targetCelciusTemp = MIN_CELCIUS_TEMP;
        }

        displayTempText.gameObject.SetActive(isPowerOn);
        displayUnitText.gameObject.SetActive(isPowerOn);
        displayOText.gameObject.SetActive(isPowerOn);
        SetSetTempText();

        Debug.Log($"Heater Power: {(isPowerOn ? "ON" : "OFF")}");
    }

    // ====== LOGIC TRUYỀN NHIỆT (JUN) ======
    private void ApplyHeatToContainer()
    {
        if (objectsOnPlate.Count == 0) return;

        // Tắt truyền nhiệt nếu bếp tắt
        if (!isPowerOn && currentCelciusTemp <= MIN_CELCIUS_TEMP)
        {
            foreach (var obj in objectsOnPlate)
            {
                obj.SetHeatPower(0f);
            }
            return;
        }

        float heaterTempK = GetCurrentKelvinTemp();

        foreach (var obj in objectsOnPlate)
        {
            var mixture = obj.GetMixture();
            if (mixture == null) 
            {
                obj.SetHeatPower(0f);
                continue;
            }

            // Giả định Mixture.GetTemperature() trả về độ Kelvin (Chuẩn hóa học)
            float containerTempK = mixture.GetTemperature(); 
            float deltaTemp = heaterTempK - containerTempK;

            // Nếu bếp nóng hơn dung dịch -> Truyền công suất (Jun) vào
            if (deltaTemp > 0)
            {
                // Truyền nhiệt năng dựa trên chênh lệch nhiệt độ, giới hạn ở Max Power
                float appliedPower = Mathf.Min(deltaTemp * heatTransferRate, maxHeatPower);
                obj.SetHeatPower(appliedPower);
            }
            else
            {
                // Dung dịch đã đạt nhiệt độ của bếp hoặc bếp đang nguội hơn -> Không cấp thêm công suất
                obj.SetHeatPower(0f);
            }
        }
    }

    #region Trigger Events

    private void OnTriggerEnter(Collider other)
    {   
        var labEquipment = other.GetComponentInParent<ContainerEquipmentBase>();
        if (labEquipment == null)
            return;

        if (objectsOnPlate.Contains(labEquipment))
            return;
        objectsOnPlate.Add(labEquipment);
    }

    private void OnTriggerExit(Collider other)
    {
        var labEquipment = other.GetComponentInParent<ContainerEquipmentBase>();
        if (labEquipment == null)
            return;
            
        if (objectsOnPlate.Contains(labEquipment))
        {
            // Bắt buộc Set Heat Power về 0 khi nhấc bình ra khỏi bếp
            labEquipment.SetHeatPower(0f); 
            objectsOnPlate.Remove(labEquipment);
        }
    }

    #endregion
}