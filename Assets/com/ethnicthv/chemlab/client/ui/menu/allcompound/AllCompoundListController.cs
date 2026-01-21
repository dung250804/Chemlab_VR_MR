using System;
using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.engine.molecule;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.menu.allcompound
{
    public class AllCompoundListController : MonoBehaviour
    {
        [SerializeField] private AllCompoundItemController itemPrefab;
        [SerializeField] private Transform itemContainer;
        
        private readonly HashSet<string> _createdItems = new HashSet<string>();

        private void OnEnable()
        {
            Setup();
        }

        private void Setup()
        {
            var molecules = Molecule.GetAllMolecules();
            var temp = new List<Molecule>(molecules);
            //Note: Sort the molecules by their ID
            temp.Sort((a, b) => string.Compare(a.GetFullID(), b.GetFullID(), StringComparison.Ordinal));

            foreach (var molecule in temp.Where(molecule => !_createdItems.Contains(molecule.GetFullID())))
            {
                CreateItem(molecule);
                _createdItems.Add(molecule.GetFullID());
            }
        }
        
        private void CreateItem(Molecule molecule)
        {
            var item = Instantiate(itemPrefab, itemContainer);
            item.SetMolecule(molecule);
            item.gameObject.SetActive(true);
        }
    }
}