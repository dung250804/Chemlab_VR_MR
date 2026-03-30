using UnityEngine;

public enum EnumPage
{
    Main = 0,
    MolStructure = 1,
    ChemicalTool = 2,
    ToolSettings = 3,
}
public class UIPageManager : MonoBehaviour
{
    public void ShowPage(EnumPage page)
    {
        int index = (int)page;
        if (index < 0 || index >= transform.childCount) return;

        // Tắt tất cả child
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        // Bật page cần
        transform.GetChild(index).gameObject.SetActive(true);
    }
}