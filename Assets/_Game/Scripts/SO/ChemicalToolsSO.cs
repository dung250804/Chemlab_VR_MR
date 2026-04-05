using UnityEngine;
using System.Collections.Generic;

public enum ChemicalToolType
{
    BeakerBig,
    BeakerMedium,
    BeakerSmall,
    Pipette,
    ElectricPipette,
}

[System.Serializable]
public class ChemicalTool
{
    public string name;
    public ChemicalToolType type;
    public GameObject prefab;
    public int maxVolume;
}

[CreateAssetMenu(fileName = "ChemicalToolsSO", menuName = "ChemLab/Chemical Tools Database")]
public class ChemicalToolsSO : ScriptableObject
{
    private static ChemicalToolsSO instance;

    public static ChemicalToolsSO Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<ChemicalToolsSO>("SO/ChemicalToolsSO");
            }
            return instance;
        }
    }

    public List<ChemicalTool> chemicalTools = new List<ChemicalTool>();
    public GameObject btnChemicalToolPrefab;
    public static ChemicalTool _cache_chemicalTool;
}