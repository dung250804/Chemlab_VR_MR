using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MolStructure
{
    public string name;
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "MolStructureSO", menuName = "ChemLab/Mol Structure Database")]
public class MolStructureSO : ScriptableObject
{
    private static MolStructureSO instance;

    public static MolStructureSO Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<MolStructureSO>("SO/MolStructureSO");
            }
            return instance;
        }
    }

    public List<MolStructure> molStructures = new List<MolStructure>();
    public GameObject btnMolStructurePrefab;
}