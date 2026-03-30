using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    public Button btnMolecularStructure;
    public Button btnChemicalTools;

    private WristUI wristUI;

    void Awake()
    {
        wristUI = GetComponentInParent<WristUI>();
        btnMolecularStructure.onClick.AddListener(() => wristUI.pageManager.ShowPage(EnumPage.MolStructure));
        btnChemicalTools.onClick.AddListener(() => wristUI.pageManager.ShowPage(EnumPage.ChemicalTool));
    }
}
