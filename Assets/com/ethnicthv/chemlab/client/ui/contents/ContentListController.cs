using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.client.api.ui.contents;
using com.ethnicthv.chemlab.engine;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.mixture;
using com.ethnicthv.chemlab.engine.api.molecule;
using com.ethnicthv.chemlab.engine.molecule;
using com.ethnicthv.util.pool;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.contents
{
    public class ContentListController : MonoBehaviour, IContentListController, IChemicalTicker
    {
        [SerializeField] private GameObject contentListItemPrefab;
        [SerializeField] private RectTransform itemContainer;

        private IMixtureContainer _mixtureContainer;
        private OrderedDictionary _moleculeAmounts;
        
        private Pool<IContentListItemController> _itemPool;
        private readonly Dictionary<Molecule, IContentListItemController> _activeItems = new();
        
        private void Awake()
        {
            _itemPool = new Pool<IContentListItemController>(Factory);
        }

        private void OnEnable()
        {
            ChemicalTickerHandler.AddTicker(this);
        }

        private void OnDisable()
        {
            ChemicalTickerHandler.RemoveTicker(this);
            Reset();
        }

        private void Reset()
        {
            _mixtureContainer = null;
            _moleculeAmounts = null;
            var l = new List<Molecule>(_activeItems.Keys);
            foreach (var key in l)
            {
                _itemPool.Return(_activeItems[key]);
                _activeItems.Remove(key);
            }
            UpdateHeight(0);
        }

        public void Setup(IMixtureContainer mixtureContainer)
        {
            Reset();
            _mixtureContainer = mixtureContainer;
            
            var mixture = mixtureContainer.GetMixture();
            var volume = mixtureContainer.GetVolume();
            
            if (mixture == null) return;
            _moleculeAmounts = new OrderedDictionary();

            foreach (var molecule in mixture.GetMolecules())
            {
                var moles = mixture.GetMoles(molecule);
                _moleculeAmounts.Add(molecule, moles * volume);
            }
            
            UpdateList();
        }

        private void UpdateList()
        {
            var mixture = _mixtureContainer.GetMixture();
            var volume = _mixtureContainer.GetVolume();
            
            if (mixture == null) return;
            
            var molecules = mixture.GetMolecules();
            foreach (Molecule molecule in _moleculeAmounts.Keys)
            {
                if (molecules.Contains(molecule)) continue;
                _moleculeAmounts.Remove(molecule);
                _itemPool.Return(_activeItems[molecule]);
                _activeItems.Remove(molecule);
            }

            foreach (var molecule in molecules)
            {
                var moles = mixture.GetMoles(molecule);
                if (_moleculeAmounts.Contains(molecule))
                {
                    _moleculeAmounts[molecule] = moles * volume;
                }
                else
                {
                    _moleculeAmounts.Add(molecule, moles * volume);
                }
            }

            var height = 0;
            
            foreach (Molecule molecule in _moleculeAmounts.Keys)
            {
                if (_activeItems.TryGetValue(molecule, out var activeItem))
                {
                    activeItem.Setup(molecule, (float)_moleculeAmounts[molecule] * volume);
                }
                else
                {
                    var item = _itemPool.Get();
                    item.Setup(molecule, (float) _moleculeAmounts[molecule] * volume);
                    _activeItems.Add(molecule, item);
                }
                
                height += _activeItems[molecule].GetHeight();
            }
            
            UpdateHeight(height);
        }

        private void UpdateHeight(int height)
        {
            itemContainer.sizeDelta = new Vector2(itemContainer.sizeDelta.x, height);
        }

        private IContentListItemController Factory()
        {
            var item = Instantiate(contentListItemPrefab, itemContainer).GetComponent<ContentListItemController>();
            return item;
        }

        public void Tick()
        {
            if (_mixtureContainer?.GetMixture() == null) return;
            UpdateList();
        }
    }
}