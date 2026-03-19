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

    [Header("Power")]
    [SerializeField] private bool isPowerOn;

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
    private int targetCelciusTemp = 0;
    [HideInInspector] public float currentCelciusTemp = 0;

    
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
            return;

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
        return currentCelciusTemp + 273;
    }

    private void OnPowerSelected(PointerEvent evt)
    {
        TogglePower();
    }

    private void OnModeSelected(PointerEvent evt)
    {
        NextMode();
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

    private void NextMode()
    {
        if (!isPowerOn) return;

        currentUnit++;

        if ((int)currentUnit >= System.Enum.GetValues(typeof(HeaterUnit)).Length)
            currentUnit = 0;

        displayUnitText.SetText(currentUnit == HeaterUnit.Celcius ? "C" : "K");
        SetSetTempText();
    }

    private void ApplyHeatToContainer()
    {
        if (objectsOnPlate.Count == 0) return;
        if (!isPowerOn) return;

        foreach (var obj in objectsOnPlate)
        {
            float delta = currentCelciusTemp - obj.calciusTemp;
            
            if (delta > 0)
            {
                obj.AddHeat(delta * 0.5f * Time.deltaTime);
            }
        }
    }

    #region Trigger Events

    private void OnTriggerEnter(Collider other)
    {   
        if (!other.TryGetComponent(out ContainerEquipmentBase labEquipment))
            return;
        if (objectsOnPlate.Contains(labEquipment))
            return;
        objectsOnPlate.Add(labEquipment);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out ContainerEquipmentBase labEquipment))
            return;
        objectsOnPlate.Remove(labEquipment);
    }

    #endregion
}
