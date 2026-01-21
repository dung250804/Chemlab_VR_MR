using System;
using System.Collections.Generic;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.client.core.game;
using com.ethnicthv.chemlab.client.game.util;
using TMPro;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.game
{
    public class HeaterBehaviour : MonoBehaviour, IInstrument, IHeater, IInteractable, IPluggable
    {
        [SerializeField] private TextMeshPro heatModeText;
        [SerializeField] private SpriteRenderer onOffIndicator;
        [SerializeField] private Transform heatablePosition;

        private bool _isOn;
        private HeatMode _heatMode = HeatMode.Heat;
        private float _heatPower;

        private IHeatable _heatable;


        public List<IInteractablePlugin> Plugins { get; } = new();

        private IPluggable _pluggable => this;

        private void Awake()
        {
            _pluggable.TryAddAllPlugins(gameObject);
        }

        private void Start()
        {
            SetMode(HeatMode.Heat);
            SetOnOff(false);
        }

        private void OnEnable()
        {
            InstrumentManager.AddInstrument(gameObject, this);
            InteractableManager.RegisterInteractable(gameObject, this);
        }

        private void OnDisable()
        {
            InstrumentManager.RemoveInstrument(gameObject);
            InteractableManager.UnregisterInteractable(gameObject);
        }

        public void SetHeatable(IHeatable heatable)
        {
            _heatable = heatable;
            if (heatable == null) return;
            heatable.SetHeater(this);
            UpdateHeatable();

            var position = heatablePosition.position;
            ((IInstrument)heatable).GetMainTransform().position = position;
        }

        public bool IsAttachedToHeatable(out IHeatable heatable)
        {
            heatable = _heatable;
            return heatable != null;
        }

        public void OnInteract()
        {
            Debug.Log("Interacted with heater");

            _pluggable.Plugins.ForEach(plugin => plugin.OnInteract());
        }

        public List<(string name, Action onClick)> GetOptions()
        {
            var options = new List<(string name, Action onClick)>
            {
                ("Change mode", ToggleMode),
                ("Turn on/off", ToggleOnOff),
            };

            if (IsAttachedToHeatable(out var heatable))
            {
                options.Add(("Detach", () => HeatingUtil.DetachHeater(heatable, this)));
            }

            _pluggable.Plugins.ForEach(plugin => plugin.OnGetOptions(ref options));

            return options;
        }

        public void OnHover()
        {
            _pluggable.Plugins.ForEach(plugin => plugin.OnHover());
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
            if (other == null) return;
            Debug.Log("Dropped on " + other.name);

            _pluggable.Plugins.ForEach(plugin => plugin.OnDrop(other));
        }

        public List<(string name, Action onClick)> GetDropOptions(GameObject other)
        {
            if (other == null) return null;
            Debug.Log("Checking drop options for " + other.name);

            var options = new List<(string name, Action onClick)>();

            if (InstrumentManager.TryGetInstrument(other, out var otherInstrument))
            {
                Debug.Log("Dropped on " + other.name);
                if (otherInstrument is IHeatable heatable)
                {
                    Debug.Log("Dropped on heatable");
                    options.Add(("Attach", () => HeatingUtil.AttachHeater(heatable, this)));
                }
            }

            _pluggable.Plugins.ForEach(plugin => plugin.OnGetDropOptions(other, ref options));

            return options;
        }

        private void UpdateHeatable()
        {
            _heatable?.SetHeatPower(_isOn ? _heatPower : 0);
        }

        private void UpdateHeatPower()
        {
            _heatPower = _heatMode switch
            {
                HeatMode.Heat => 15000,
                HeatMode.SuperHeat => 50000,
                _ => 0
            };

            UpdateHeatable();
        }

        private void SetMode(HeatMode mode)
        {
            _heatMode = mode;
            heatModeText.text = _heatMode switch
            {
                HeatMode.Heat => "Heat",
                HeatMode.SuperHeat => "S.Heat",
                _ => throw new ArgumentOutOfRangeException()
            };
            UpdateHeatPower();
        }

        private void SetOnOff(bool isOn)
        {
            _isOn = isOn;
            UpdateHeatable();
            onOffIndicator.color = _isOn ? Color.green : Color.red;
        }

        private void ToggleOnOff()
        {
            Debug.Log("Toggling heater");
            SetOnOff(!_isOn);
        }

        private void ToggleMode()
        {
            Debug.Log("Toggling heat mode");
            SetMode(_heatMode == HeatMode.Heat ? HeatMode.SuperHeat : HeatMode.Heat);
        }

        private enum HeatMode
        {
            Heat,
            SuperHeat
        }
    }
}