using UnityEngine;
using DG.Tweening;

public class LitmusPaper : MonoBehaviour
{
    [Header("Renderer")]
    public Renderer targetRenderer;

    [Header("Colors")]
    public Color acidColor = Color.red;
    public Color neutralColor = new Color(0.5f, 0f, 0.5f);
    public Color baseColor = Color.blue;

    [Header("pH Threshold")]
    public float acidThreshold = 6.5f;
    public float baseThreshold = 7.5f;

    [Header("Tween")]
    public float duration = 1.0f; // thời gian đổi màu
    public Ease ease = Ease.OutQuad;

    public bool isUsed = false;

    private Material mat;

    void Start()
    {
        if (targetRenderer != null)
        {
            mat = targetRenderer.material;
            mat.color = neutralColor; // mặc định là trung tính
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isUsed) return;
        
        other.TryGetComponent<LiquidContainer>(out var liquidContainer);
        if (liquidContainer == null)
        {
            liquidContainer = other.GetComponentInParent<LiquidContainer>();
            if (liquidContainer == null) return;
        }

        float pH = liquidContainer.GetPH();
        Debug.Log($"Litmus Paper touched liquid with pH: {pH}");
        Color targetColor = GetColorFromPH(pH);

        if (liquidContainer.GetMixture() != null)
        {
            StartColorChange(targetColor);
            isUsed = true;
        }
        
    }

    void StartColorChange(Color targetColor)
    {
        if (mat == null) return;

        mat.DOColor(targetColor, duration)
            .SetEase(ease);
    }

    Color GetColorFromPH(float pH)
    {
        if (pH < acidThreshold)
            return acidColor;
        else if (pH > baseThreshold)
            return baseColor;
        else
            return neutralColor;
    }
}