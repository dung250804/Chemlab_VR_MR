using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.client.api.ui.utility;
using com.ethnicthv.chemlab.client.game;
using com.ethnicthv.chemlab.client.game.plugin;
using com.ethnicthv.chemlab.engine.molecule;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.utility
{
    public class AddMoleculePanelController : MonoBehaviour, IAddMoleculePanelController
    {
        private Molecule _molecule;
        
        [SerializeField] private Transform bottleContainer;
        
        [SerializeField] private Transform itemContainer;
        [SerializeField] private AddMoleculeItemController itemPrefab;
        
        public void OpenPanel()
        {
            //Note: open the Utility Layer
            transform.parent.gameObject.SetActive(true);
            
            //Note: open the naming panel
            gameObject.SetActive(true);
        }

        public void ClosePanel()
        {
            //Note: close the Utility Layer
            transform.parent.gameObject.SetActive(false);
            
            //Note: close the naming panel
            gameObject.SetActive(false);
            
            //Note: reset the target container
            _molecule = null;
        }

        public void SetupPanel(Molecule molecule)
        {
            _molecule = molecule;
            
            foreach (Transform child in itemContainer)
            {
                Destroy(child.gameObject);
            }

            if (_molecule != null)
            {
                if (_molecule.IsSolid())
                {
                    var solidContainers = GetAllSolidContainers();
                    foreach (var (bottleName, solidHolder) in solidContainers)
                    {
                        var item = Instantiate(itemPrefab, itemContainer);
                        item.Set(_molecule, bottleName, null, solidHolder);
                        item.gameObject.SetActive(true);
                    }
                }
                else
                {
                    var bottles = GetAllBottles();
                    foreach (var (bottleName, bottle) in bottles)
                    {
                        var item = Instantiate(itemPrefab, itemContainer);
                        item.Set(_molecule, bottleName, bottle, null);
                        item.gameObject.SetActive(true);
                    }
                }
            }
        }
        
        private Dictionary<string, SolidHolderBehaviour> GetAllSolidContainers()
        {
            var b = new Dictionary<string, SolidHolderBehaviour>();
            var solidHolderBehaviours = bottleContainer.GetComponentsInChildren<SolidHolderBehaviour>();
            foreach (var solidHolder in solidHolderBehaviours)
            {
                if (!solidHolder.IsSolidEmpty()) continue;
                if (TryGetSolidHolderName(solidHolder, out var s))
                {
                    b.Add(s, solidHolder);
                }
            }
            return b;
        }
        
        private static bool TryGetSolidHolderName(SolidHolderBehaviour solidHolderBehaviour, out string name)
        {
            if (((IPluggable)solidHolderBehaviour).TryGetPlugin(out NamePlugin plugin))
            {
                name = plugin.GetName();
                return true;
            }

            name = null;
            return false;
        }
        
        private Dictionary<string, BottleBehaviour> GetAllBottles()
        {
            var b = new Dictionary<string, BottleBehaviour>();
            var bottles = bottleContainer.GetComponentsInChildren<BottleBehaviour>();
            foreach (var bottle in bottles)
            {
                if (!bottle.IsEmpty()) continue;
                if (TryGetBottleName(bottle, out var s))
                {
                    b.Add(s, bottle);
                }
            }
            return b;
        }

        private static bool TryGetBottleName(BottleBehaviour bottleBehaviour, out string name)
        {
            if (((IPluggable)bottleBehaviour).TryGetPlugin(out NamePlugin plugin))
            {
                name = plugin.GetName();
                return true;
            }

            name = null;
            return false;
        }
    }
}