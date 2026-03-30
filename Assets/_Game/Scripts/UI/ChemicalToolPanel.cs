using UnityEngine;
using UnityEngine.UI;

public class ChemicalToolPanel : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private Button btnBack;
    
    private WristUI wristUI;
    void Awake()
    {
        wristUI = GetComponentInParent<WristUI>();
        btnBack.onClick.AddListener(() => wristUI.pageManager.ShowPage(EnumPage.Main));
        InitButtons();
    }

    private void InitButtons()
    {
        foreach (var tool in ChemicalToolsSO.Instance.chemicalTools)
        {
            GameObject btnObj = Instantiate(ChemicalToolsSO.Instance.btnChemicalToolPrefab, contentParent);
            btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = tool.name;
            btnObj.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnChemicalToolButtonClicked(tool));
        }
    }

    private void OnChemicalToolButtonClicked(ChemicalTool tool)
    {
        ChemicalToolsSO._cache_chemicalTool = tool;
        switch (tool.type)
        {
            case ChemicalToolType.BeakerBig:
            case ChemicalToolType.BeakerMedium:
            case ChemicalToolType.BeakerSmall:
                wristUI.pageManager.ShowPage(EnumPage.ToolSettings);
                break;
            default:
                Transform cam = Camera.main.transform;
                Vector3 spawnPos = cam.position + cam.forward * 0.2f;
                Instantiate(ChemicalToolsSO._cache_chemicalTool.prefab, spawnPos, Quaternion.identity);
                break;
        }
    }
}
