using UnityEngine;
using UnityEngine.UI;

public class MolecularStructurePanel : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private Transform molStructureDisplayParent;
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
        foreach (var mol in MolStructureSO.Instance.molStructures)
        {
            GameObject btnObj = Instantiate(MolStructureSO.Instance.btnMolStructurePrefab, contentParent);
            btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = mol.name;
            btnObj.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnMolStructureButtonClicked(mol));
        }
    }

    private void OnMolStructureButtonClicked(MolStructure mol)
    {
        wristUI.uiCanvas.SetActive(false);
        wristUI.OnMolDisplay(mol);
    }
}
