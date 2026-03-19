using UnityEngine;

public class MolecularStructurePanel : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private Transform molStructureDisplayParent;
    private WristUI wristUI;
    void Awake()
    {
        wristUI = GetComponentInParent<WristUI>();
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
