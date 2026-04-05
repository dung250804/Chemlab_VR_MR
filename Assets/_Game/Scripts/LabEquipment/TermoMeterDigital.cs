using System.Collections.Generic;
using UnityEngine;
using com.ethnicthv.chemlab.client.core.game;
using UnityEngine.UI;
using TMPro; // Environment

public class TermoMeterDigital : MonoBehaviour
{
    public enum TemperatureUnit
    {
        Kelvin,
        Celsius
    }

    [Header("Settings")]
    public TemperatureUnit unit = TemperatureUnit.Celsius;

    public float responseSpeed = 2f;     // tốc độ phản ứng
    public float envInfluence = 0.3f;    // ảnh hưởng môi trường
    public float stabilizeThreshold = 0.01f;

    public Button resetButton;
    public Image temperatureImage;
    public TMP_Text temperatureText;

    public Gradient temperatureGradient;

    // khoảng nhiệt độ để map màu
    public float minTempC = 0f;
    public float maxTempC = 100f;

    private List<ContainerEquipmentBase> containers = new List<ContainerEquipmentBase>();

    private float displayedTempK = 298.15f; // nhiệt kế đang hiển thị (Kelvin)
    private float targetTempK = 298.15f;
    float startupNoise = 0.5f;

    void Awake()
    {
        resetButton.onClick.AddListener(ResetThermometer);
    }

    void Start()
    {
        float envTemp = Environment.Instance.Temperature;

        displayedTempK = envTemp + Random.Range(-startupNoise, startupNoise);
        targetTempK = envTemp;
    }

    void Update()
    {
        startupNoise = Mathf.Lerp(startupNoise, 0f, Time.deltaTime * 0.5f);
        UpdateTargetTemperature();
        UpdateDisplayedTemperature();
        UpdateVisual();
    }

    void OnTriggerEnter(Collider other)
    {
        var container = other.GetComponentInParent<ContainerEquipmentBase>();
        if (container == null)
            container = other.GetComponentInChildren<ContainerEquipmentBase>();

        if (container != null && !containers.Contains(container))
        {
            containers.Add(container);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var container = other.GetComponentInParent<ContainerEquipmentBase>();
        if (container == null)
            container = other.GetComponentInChildren<ContainerEquipmentBase>();
            
        if (container != null)
        {
            containers.Remove(container);
        }
    }

    void UpdateTargetTemperature()
    {
        if (containers.Count == 0)
        {
            // không chạm chất → về nhiệt độ phòng
            targetTempK = Environment.Instance.Temperature;
            return;
        }

        float totalTemp = 0;
        int count = 0;

        foreach (var c in containers)
        {
            var mix = c.GetMixture();
            if (mix == null) continue;

            totalTemp += mix.GetTemperatureKelvin();
            count++;
        }

        if (count > 0)
            targetTempK = totalTemp / count;
    }

    void UpdateDisplayedTemperature()
    {
        float envTemp = Environment.Instance.Temperature;

        // inertia (độ trễ giống thật)
        displayedTempK = Mathf.Lerp(displayedTempK, targetTempK, Time.deltaTime * responseSpeed);

        // bị ảnh hưởng bởi môi trường
        displayedTempK = Mathf.Lerp(displayedTempK, envTemp, Time.deltaTime * envInfluence);

        // ổn định
        if (Mathf.Abs(displayedTempK - targetTempK) < stabilizeThreshold)
        {
            displayedTempK = targetTempK;
        }

        if (temperatureText != null)
            temperatureText.text = GetTemperatureString();
    }

    // =========================
    // ===== GET TEMP ==========
    // =========================

    public float GetTemperature()
    {
        switch (unit)
        {
            case TemperatureUnit.Celsius:
                return KelvinToCelsius(displayedTempK);
            case TemperatureUnit.Kelvin:
            default:
                return displayedTempK;
        }
    }

    public string GetTemperatureString()
    {
        float temp = GetTemperature();

        switch (unit)
        {
            case TemperatureUnit.Celsius:
                return $"{temp:F1} °C";
            case TemperatureUnit.Kelvin:
                return $"{temp:F2} °K";
            default:
                return temp.ToString("F1");
        }
    }

    // =========================
    // ===== CONVERT ===========
    // =========================

    float KelvinToCelsius(float k) => k - 273.15f;
    float CelsiusToKelvin(float c) => c + 273.15f;

    // =========================
    // ===== BUTTON ============
    // =========================

    public void ToggleUnit()
    {
        if (unit == TemperatureUnit.Celsius)
            unit = TemperatureUnit.Kelvin;
        else
            unit = TemperatureUnit.Celsius;
    }

    public void SetCelsius()
    {
        unit = TemperatureUnit.Celsius;
    }

    public void SetKelvin()
    {
        unit = TemperatureUnit.Kelvin;
    }

    public void ResetThermometer()
    {
        startupNoise = 0.5f;
        float envTemp = Environment.Instance.Temperature;

        displayedTempK = envTemp + Random.Range(-startupNoise, startupNoise);
        targetTempK = envTemp;
    }

    void UpdateVisual()
    {
        float tempC = KelvinToCelsius(displayedTempK);

        // normalize về 0 → 1
        float t = Mathf.InverseLerp(minTempC, maxTempC, tempC);

        Color color = temperatureGradient.Evaluate(t);

        if (temperatureImage != null)
            temperatureImage.color = color;

        if (temperatureText != null)
            temperatureText.color = color;
    }
}