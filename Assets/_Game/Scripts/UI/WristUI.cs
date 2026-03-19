using UnityEngine;
using UnityEngine.UI;

public class WristUI : MonoBehaviour
{
    [Header("Hand Tracking Joints")]
    public Transform handWrist;
    public Transform indexMetacarpal;
    public Transform littleMetacarpal;

    public Transform indexTip;
    public Transform middleTip;
    public Transform ringTip;
    public Transform littleTip;

    [Header("Controller Wrist (Attach Point)")]
    public Transform controllerWrist;

    [Header("UI Settings")]
    public GameObject uiCanvas;
    public GameObject molStructureDisplayPrefab;
    public Vector3 localOffset = new Vector3(0f, 0.03f, 0.05f);

    [Header("Palm Down Condition (Hand Tracking Only)")]
    public float palmDownThreshold = -0.4f;

    [Header("Fist Detection")]
    public float curlThreshold = 0.085f;  // Hợp với dữ liệu XR Hands thực tế

    [Header("Debug")]
    public bool debug = true;

    // Debug UI text
    private Text debugTextUI;

    private bool previousHandTracking = false;
    private Transform lastWrist = null;
    private bool showUI = false;
    private bool molDisplayActive = false;

    private void Start()
    {
        uiCanvas.SetActive(false);
        molStructureDisplayPrefab.SetActive(false);
        InitDebugUI();
    }

    void Update()
    {
        bool isHandTracking = IsUsingHandTracking();
        Transform activeWrist = GetActiveWrist();

        if (activeWrist == null)
        {
            uiCanvas.SetActive(false);
            return;
        }

        if (isHandTracking)
        {
            if (CheckHandGesture())
                showUI = true;
            else
            {
                showUI = false;
                OnMolHide();
            }
        }
        else
        {
            if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger))
                showUI = true;
            else
            {
                showUI = false;
                OnMolHide();
            }
        }

        uiCanvas.SetActive(showUI && !molDisplayActive);
        if (!showUI && !molDisplayActive) return;

        // Update 1 lần khi thay đổi trạng thái
        if (previousHandTracking != isHandTracking || lastWrist != activeWrist)
        {
            previousHandTracking = isHandTracking;
            lastWrist = activeWrist;

            transform.SetParent(activeWrist, false);
            transform.localPosition = localOffset;
            transform.localRotation = Quaternion.identity;
        }
    }

    Transform GetActiveWrist()
    {
        if (IsUsingHandTracking())
            return handWrist;

        if (controllerWrist != null)
            return controllerWrist;

        return null;
    }

    bool IsUsingHandTracking()
    {
        return handWrist != null && handWrist.gameObject.activeInHierarchy;
    }

    // ======= GESTURE: FIST + PALM DOWN =======
    bool CheckHandGesture()
    {
        // --- 1) Palm down ---
        Vector3 v1 = indexMetacarpal.position - handWrist.position;
        Vector3 v2 = littleMetacarpal.position - handWrist.position;
        Vector3 palmNormal = Vector3.Cross(v1, v2).normalized;

        bool palmDown = palmNormal.y < palmDownThreshold;

        if (debug)
            Debug.Log($"[Palm] normal.y={palmNormal.y:F3}  → palmDown={palmDown}");

        if (!palmDown && !showUI && !molDisplayActive) 
        { 
            UpdateDebugText("Palm not down"); 
            return false; 
        }

        // --- 2) Palm center: mid-point của xương bàn tay ---
        Vector3 metaCenter = (indexMetacarpal.position + littleMetacarpal.position) * 0.5f;
        Vector3 palmCenter = metaCenter + palmNormal * (-0.02f);

        // --- 3) Deep curl ---
        float d1 = Dist(indexTip, palmCenter);
        float d2 = Dist(middleTip, palmCenter);
        float d3 = Dist(ringTip, palmCenter);
        float d4 = Dist(littleTip, palmCenter);

        bool c1 = d1 < curlThreshold;
        bool c2 = d2 < curlThreshold;
        bool c3 = d3 < curlThreshold;
        bool c4 = d4 < curlThreshold;

        bool fist = c1 && c2 && c3 && c4;

        if (debug)
        {
            Debug.Log($"[Curl] index={c1}, middle={c2}, ring={c3}, little={c4}");
            Debug.Log($"[Fist] fist={fist}");

            UpdateDebugText(
                $"PalmDown: {palmDown}\n" +
                $"Index: {d1:F3}\n" +
                $"Middle: {d2:F3}\n" +
                $"Ring: {d3:F3}\n" +
                $"Little: {d4:F3}\n" +
                $"Threshold: {curlThreshold:F3}\n" +
                $"Fist: {fist}"
            );
        }

        if (!showUI && !molDisplayActive)
            return fist && palmDown;  
        return fist; 
    }

    float Dist(Transform tip, Vector3 palm)
    {
        float d = Vector3.Distance(tip.position, palm);
        if (debug)
            Debug.Log($"[Distance] {tip.name} → palmCenter = {d:F3}");
        return d;
    }

    // ====================== DEBUG UI CREATION ======================
    void InitDebugUI()
    {
        if (!debug) return;

        GameObject debugCanvas = new GameObject("WristUIDebugCanvas");
        debugCanvas.transform.SetParent(transform);
        debugCanvas.transform.localPosition = new Vector3(0, 0.05f, 0.05f);

        Canvas canvas = debugCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.transform.localScale = Vector3.one * 0.0025f;

        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(debugCanvas.transform);

        debugTextUI = textObj.AddComponent<Text>();
        debugTextUI.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        debugTextUI.fontSize = 32;
        debugTextUI.color = Color.white;
        debugTextUI.alignment = TextAnchor.UpperLeft;
        debugTextUI.horizontalOverflow = HorizontalWrapMode.Overflow;
        debugTextUI.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rt = debugTextUI.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 600);
    }

    void UpdateDebugText(string msg)
    {
        if (debugTextUI != null)
            debugTextUI.text = msg;
    }

    public void OnMolDisplay(MolStructure mol)
    {
        GameObject molPref = Instantiate(mol.prefab, molStructureDisplayPrefab.transform);
        molPref.transform.localPosition = Vector3.zero;
        molStructureDisplayPrefab.transform.localScale = Vector3.one;
        molStructureDisplayPrefab.SetActive(true);
        uiCanvas.SetActive(false);
        molDisplayActive = true;
    }

    private void OnMolHide()
    {
        foreach (Transform child in molStructureDisplayPrefab.transform)
        {
            Destroy(child.gameObject);
        }
        molStructureDisplayPrefab.SetActive(false);
        molDisplayActive = false;
    }
}
