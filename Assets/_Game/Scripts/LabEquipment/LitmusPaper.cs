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

    private bool isUsed = false;

    private Material mat;

    void Start()
    {
        if (targetRenderer != null)
        {
            mat = targetRenderer.material;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isUsed) return;

        var container = other.GetComponentInParent<LiquidContainer>();
        if (container == null) return;

        float pH = container.GetPH();
        Color targetColor = GetColorFromPH(pH);

        if (container.GetMixture() != null)
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