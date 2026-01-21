using System;
using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.core;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.client.core.game;
using com.ethnicthv.chemlab.engine.molecule;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.game
{
    public class SolidHolderBehaviour : MonoBehaviour, IInstrument, ISolidContainer, IInteractable, IPluggable
    {
        [SerializeField] private SpriteRenderer solidRenderer;

        private Molecule _solidMolecule;
        private float _solidMoles;
        
        private float _mass;
        
        private SolidDisplay _solidDisplay;


        public List<IInteractablePlugin> Plugins { get; } = new();

        private IPluggable _pluggable => this;

        private void Awake()
        {
            _pluggable.TryAddAllPlugins(gameObject);
        }

        private void OnEnable()
        {
            InstrumentManager.AddInstrument(gameObject, this);
            InteractableManager.RegisterInteractable(gameObject, this);
            
            UpdateSolid();
        }
        
        private void OnDisable()
        {
            InstrumentManager.RemoveInstrument(gameObject);
            InteractableManager.UnregisterInteractable(gameObject);
        }

        public void OnInteract()
        {
            Debug.Log("Interacted with solid holder");
        }

        public List<(string name, Action onClick)> GetOptions()
        {
            var options = new List<(string, Action)>();
            
            _pluggable.ForEachPlugin(p => p.OnGetOptions(ref options));
            
            return options;
        }

        public void OnHover()
        {
            Debug.Log("Hovered over solid holder");
        }

        public (GameObject panelObject, Action<GameObject> setupFunction) GetHoverPanel()
        {
            return (null, null);
        }

        public Transform GetMainTransform()
        {
            return transform.parent;
        }

        public void OnDrop(GameObject other)
        {
            Debug.Log("Dropped on solid holder");
        }

        public List<(string name, Action onClick)> GetDropOptions(GameObject other)
        {
            if (other == null) return null;

            if (!InstrumentManager.TryGetInstrument(other, out var otherInstrument)) return null;
            
            switch (otherInstrument)
            {
                case IMixtureContainer and ISolidContainer mixtureWithSolid:
                    return new List<(string, Action)>
                    {
                        ("Add to mixture", () => AddToMixture(mixtureWithSolid))
                    };
                case ISolidContainer solidContainer:
                    return new List<(string, Action)>
                    {
                        ("Transfer solid", () => TransferSolid(solidContainer))
                    };
            }

            return null;
        }

        public void AddSolidMolecule(Molecule solidMolecule, float moles)
        {
            if (_solidMolecule == null)
            {
                _solidMolecule = solidMolecule;
                
                UpdateSolid();
                
                _solidMoles = moles;
            }
            else if (_solidMolecule == solidMolecule)
            {
                _solidMoles += moles;
            }
            else
            {
                Debug.LogWarning("SolidHolder already contains a different solid molecule");
            }
        }

        public void RemoveSolidMolecule(Molecule solidMolecule)
        {
            if (_solidMolecule != solidMolecule) return;
            _solidMoles = 0;
            _solidMolecule = null;
            UpdateSolid();
        }

        public bool IsSolidEmpty()
        {
            return _solidMolecule == null || _solidMoles <= 0;
        }

        public void ClearSolid()
        {
            _solidMoles = 0;
            _solidMolecule = null;
            UpdateSolid();
        }

        public bool ContainsSolidMolecule(Molecule solidMolecule)
        {
            return _solidMolecule == solidMolecule;
        }

        public float GetSolidMoleculeAmount(Molecule solidMolecule)
        {
            return _solidMolecule == solidMolecule ? _solidMoles : 0;
        }

        public bool TryGetSolidMoleculeAmount(Molecule solidMolecule, out float amount)
        {
            if (_solidMolecule == solidMolecule)
            {
                amount = _solidMoles;
                return true;
            }

            amount = 0;
            return false;
        }

        private void UpdateSolid()
        {
            if (_solidMolecule == null)
            {
                _mass = 0;
            }
            else
            {
                var molarMass = _solidMolecule.GetMass();
                _mass = _solidMoles * molarMass;
            }
            
            UpdateSolidDisplay();
        }
        
        private void UpdateSolidDisplay()
        {
            if (_solidMolecule == null)
            {
                solidRenderer.sprite = null;
                return;
            }
            
            _solidDisplay = SolidDisplayManager.Instance.GetSolidDisplay(_solidMolecule.GetFullID());

            solidRenderer.sprite = _solidDisplay.sprite;
            solidRenderer.color = _solidDisplay.color;
            Debug.Log("Updated solid display");
        }
        
        private void TransferSolid(ISolidContainer other)
        {
            if (other.IsSolidEmpty() || !other.ContainsSolidMolecule(_solidMolecule)) return;
            other.AddSolidMolecule(_solidMolecule, _solidMoles);
            ClearSolid();
        }

        private void AddToMixture(ISolidContainer other)
        {
            if (other is BottleBehaviour)
            {
                Debug.LogWarning("Cannot add solid to bottle");
            }
            other.AddSolidMolecule(_solidMolecule, _solidMoles);
            ClearSolid();
        }
    }
}