using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhMeterDigital : MonoBehaviour
{
    [Header("pH Settings")]
    [SerializeField]
    private float responseSpeed = 4f;

    [SerializeField]
    private float stabilizeThreshold = 0.01f;

    [SerializeField]
    private float neutralPH = 7f;

    [SerializeField]
    private float startupNoise = 0.3f;

    [Header("Visual")]
    [SerializeField]
    private Image pHImage;

    [SerializeField]
    private TMP_Text pHText;

    private Gradient pHGradient;

    [SerializeField]
    [Range(0f, 14f)]
    private float minPH = 0f;

    [SerializeField]
    [Range(0f, 14f)]
    private float maxPH = 14f;

    private readonly List<ContainerEquipmentBase>
        containers = new();

    private float displayedPH;
    private float targetPH;

    // =====================================================
    // Unity
    // =====================================================

    private void Awake()
    {
        SetupGradient();
    }

    private void SetupGradient()
    {
        pHGradient = new Gradient();

        GradientColorKey[] colorKeys =
        {
            // Acid mạnh
            new GradientColorKey(
                new Color(1f, 0f, 0f), // đỏ
                0f
            ),

            // Acid nhẹ
            new GradientColorKey(
                new Color(1f, 0.5f, 0f), // cam
                0.25f
            ),

            // Trung tính
            new GradientColorKey(
                new Color(0f, 1f, 0f), // xanh lá
                0.5f
            ),

            // Bazơ nhẹ
            new GradientColorKey(
                new Color(0f, 0.5f, 1f), // xanh cyan
                0.75f
            ),

            // Bazơ mạnh
            new GradientColorKey(
                new Color(0.3f, 0f, 1f), // tím/xanh dương
                1f
            )
        };

        GradientAlphaKey[] alphaKeys =
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        };

        pHGradient.SetKeys(
            colorKeys,
            alphaKeys
        );
    }

    private void Start()
    {
        ResetPHMeter();
    }

    private void Update()
    {
        UpdateTargetPH();
        UpdateDisplayedPH();
        UpdateVisual();
    }

    // =====================================================
    // Trigger
    // =====================================================

    private void OnTriggerEnter(Collider other)
    {
        ContainerEquipmentBase container =
            other.GetComponentInParent<ContainerEquipmentBase>();

        if (container == null)
        {
            container =
                other.GetComponentInChildren<ContainerEquipmentBase>();
        }

        if (container == null)
            return;

        if (containers.Contains(container))
            return;

        containers.Add(container);
    }

    private void OnTriggerExit(Collider other)
    {
        ContainerEquipmentBase container =
            other.GetComponentInParent<ContainerEquipmentBase>();

        if (container == null)
        {
            container =
                other.GetComponentInChildren<ContainerEquipmentBase>();
        }

        if (container == null)
            return;

        containers.Remove(container);
    }

    // =====================================================
    // pH Logic
    // =====================================================

    private void UpdateTargetPH()
    {
        ContainerEquipmentBase container =
            GetValidContainer();

        if (container == null)
        {
            targetPH = neutralPH;
            return;
        }

        targetPH = container.GetPH();
    }

    private void UpdateDisplayedPH()
    {
        displayedPH = Mathf.Lerp(
            displayedPH,
            targetPH,
            Time.deltaTime * responseSpeed
        );

        if (Mathf.Abs(displayedPH - targetPH)
            < stabilizeThreshold)
        {
            displayedPH = targetPH;
        }

        if (pHText != null)
        {
            pHText.text =
                $"pH {displayedPH:F2}";
        }
    }

    private ContainerEquipmentBase GetValidContainer()
    {
        for (int i = containers.Count - 1; i >= 0; i--)
        {
            if (containers[i] == null)
            {
                containers.RemoveAt(i);
            }
        }

        if (containers.Count == 0)
            return null;

        return containers[0];
    }

    // =====================================================
    // Public
    // =====================================================

    public float GetPH()
    {
        return displayedPH;
    }

    public void ResetPHMeter()
    {
        displayedPH =
            neutralPH +
            Random.Range(
                -startupNoise,
                startupNoise
            );

        targetPH = neutralPH;
    }

    // =====================================================
    // Visual
    // =====================================================

    private void UpdateVisual()
    {
        float t = Mathf.InverseLerp(
            minPH,
            maxPH,
            displayedPH
        );

        Color color =
            pHGradient.Evaluate(t);

        if (pHImage != null)
        {
            pHImage.color = color;
        }

        if (pHText != null)
        {
            pHText.color = color;
        }
    }
}