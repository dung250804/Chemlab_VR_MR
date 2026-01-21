using System.Collections.Generic;
using Oculus.Interaction;
using TMPro;
using UnityEngine;

public class LaboratoryBalance : MeasurementEquipmentBase
{
    public enum BalanceUnit
    {
        Gram,       // g
        Milligram,  // mg
        Kilogram,   // kg
        Ounce,      // oz
        Pound       // lb
    }

    [Header("Power")]
    [SerializeField] private bool isPowerOn;

    [Header("Unit")]
    [SerializeField] private BalanceUnit currentUnit = BalanceUnit.Gram;

    [Header("Weight (Base = gram)")]
    [SerializeField] private float rawWeightGram; // trọng lượng thật (gram)

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI displayWeightText;
    [SerializeField] private TextMeshProUGUI displayBalanceUnitText;

    [Header("Buttons")]
    [SerializeField] private PointableUnityEventWrapper powerButton;
    [SerializeField] private PointableUnityEventWrapper modeButton;
    [SerializeField] private PointableUnityEventWrapper tareButton;

    private float _tareOffsetGram;                // offset tare (gram)
    private readonly List<LabEquipmentBase> objectsOnPlate = new();

    private void Awake()
    {
        displayWeightText.gameObject.SetActive(isPowerOn);
        displayBalanceUnitText.gameObject.SetActive(isPowerOn);
        powerButton.WhenSelect.AddListener(OnPowerSelected);
        modeButton.WhenSelect.AddListener(OnModeSelected);
        tareButton.WhenSelect.AddListener(OnTareSelected);
        UpdateLabDisplayText();
    }

    private void OnDestroy()
    {
        powerButton.WhenSelect.RemoveListener(OnPowerSelected);
        modeButton.WhenSelect.RemoveListener(OnModeSelected);
        tareButton.WhenSelect.RemoveListener(OnTareSelected);
    }

    private void Update()
    {
        if (!isPowerOn) return;
        if (objectsOnPlate.Count == 0) return;

        UpdateLabDisplayText();
    }

    private void OnPowerSelected(PointerEvent evt)
    {
        TogglePower();
    }

    private void OnModeSelected(PointerEvent evt)
    {
        NextMode();
    }

    private void OnTareSelected(PointerEvent evt)
    {
        Tare();
    }


    #region Button Actions

    // ===== POWER =====
    public void TogglePower()
    {
        isPowerOn = !isPowerOn;

        if (!isPowerOn)
        {
            rawWeightGram = 0f;
            _tareOffsetGram = 0f;  
        }

        displayWeightText.gameObject.SetActive(isPowerOn);
        displayBalanceUnitText.gameObject.SetActive(isPowerOn);
        UpdateLabDisplayText();

        Debug.Log($"Balance Power: {(isPowerOn ? "ON" : "OFF")}");
    }

    // ===== TARE =====
    public void Tare()
    {
        if (!isPowerOn) return;

        _tareOffsetGram = rawWeightGram;
        UpdateLabDisplayText();
        Debug.Log("Tare set");
    }

    // ===== MODE =====
    public void NextMode()
    {
        if (!isPowerOn) return;

        currentUnit++;

        if ((int)currentUnit >= System.Enum.GetValues(typeof(BalanceUnit)).Length)
            currentUnit = 0;

        UpdateLabDisplayText();
    }

    #endregion

    #region Measurement

    public void UpdateLabDisplayText()
    {
        if (!isPowerOn)
        {
            displayWeightText.text = "";
            displayBalanceUnitText.text = "";
            return;
        }

        float value = Measure();
        string unit = GetUnitLabel();

        string formattedValue = currentUnit switch
        {
            BalanceUnit.Milligram => value.ToString("0.######"),
            BalanceUnit.Gram      => value.ToString("0.######"),
            BalanceUnit.Kilogram  => value.ToString("0.######"),
            BalanceUnit.Ounce     => value.ToString("0.######"),
            BalanceUnit.Pound     => value.ToString("0.######"),
            _ => value.ToString()
        };


        displayWeightText.text = formattedValue;
        displayBalanceUnitText.text = unit;
    }

    public override float Measure()
    {
        if (!isPowerOn)
            return 0f;

        rawWeightGram = 0f;

        foreach (LabEquipmentBase obj in objectsOnPlate)
        {
            rawWeightGram += obj.GetRawWeightGram();
        }

        float netGram = rawWeightGram - _tareOffsetGram;

        switch (currentUnit)
        {
            case BalanceUnit.Gram:
                return netGram;

            case BalanceUnit.Milligram:
                return netGram * 1000f;

            case BalanceUnit.Kilogram:
                return netGram / 1000f;

            case BalanceUnit.Ounce:
                return netGram * 0.0352739619f;

            case BalanceUnit.Pound:
                return netGram * 0.00220462262f;
        }

        return 0f;
    }

    #endregion

    #region External Input

    // Gọi khi có vật đặt lên cân
    public void SetWeightInGram(float weight)
    {
        if (!isPowerOn) return;

        rawWeightGram = weight;
    }

    public string GetUnitLabel()
    {
        switch (currentUnit)
        {
            case BalanceUnit.Gram: return "g";
            case BalanceUnit.Milligram: return "mg";
            case BalanceUnit.Kilogram: return "kg";
            case BalanceUnit.Ounce: return "oz";
            case BalanceUnit.Pound: return "lb";
        }
        return string.Empty;
    }

    #endregion

    #region Trigger Events

    private void OnTriggerEnter(Collider other)
    {   
        if (!other.TryGetComponent(out LabEquipmentBase labEquipment))
            return;
        if (objectsOnPlate.Contains(labEquipment))
            return;
        objectsOnPlate.Add(labEquipment);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out LabEquipmentBase labEquipment))
            return;
        objectsOnPlate.Remove(labEquipment);
        if (objectsOnPlate.Count == 0)
            UpdateLabDisplayText();
    }

    #endregion
}
