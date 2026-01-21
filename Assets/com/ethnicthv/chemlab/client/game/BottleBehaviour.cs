using System;
using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.client.chemistry;
using com.ethnicthv.chemlab.client.core.game;
using com.ethnicthv.chemlab.client.game.util;
using com.ethnicthv.chemlab.client.ui;
using com.ethnicthv.chemlab.engine;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.mixture;
using com.ethnicthv.chemlab.engine.molecule;
using com.ethnicthv.util.pool;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.VFX;
using Environment = com.ethnicthv.chemlab.client.core.game.Environment;

namespace com.ethnicthv.chemlab.client.game
{
    public class BottleBehaviour : MonoBehaviour, IInstrument, IInteractable, IMixtureContainer, IChemicalTicker,
        IHeatable, ISolidContainer, IGasContainer, IPluggable
    {
        // The thermal conductance (in watts per kelvin) of the area of this Vat.
        [SerializeField] private float heatConductivity = 1000f;
        [SerializeField] private float maxVolume = 1f;
        [SerializeField] private GameObject fillerPrefab;
        [SerializeField] private List<SpriteRenderer> fillers;
        [SerializeField] private Transform fillersParent;
        [SerializeField] private VisualEffect bubbles;

        private float _heatPower;
        private IHeater _heater;

        private Mixture _tickGasMixture;
        private float _tickGasVolume;
        private Mixture _contents;
        private float _volume;

        private readonly Dictionary<SpriteRenderer, LiquidPart> _fillerParts = new();

        private GameObjectPool<SpriteRenderer> _fillerPool;

        private static readonly int FillThreshold = Shader.PropertyToID("_FillThreshold");
        private static readonly int Fill = Shader.PropertyToID("_Fill");
        private static readonly int FillerUpperBound = Shader.PropertyToID("_FillUpperBound");


        public List<IInteractablePlugin> Plugins { get; } = new();

        private IPluggable _pluggable => this;

        private void Awake()
        {
            _fillerPool = new GameObjectPool<SpriteRenderer>(CreateFiller, ResetFiller);
            ((IPluggable) this).TryAddAllPlugins(gameObject);
        }

        private void OnEnable()
        {
            InstrumentManager.AddInstrument(gameObject, this);
            InteractableManager.RegisterInteractable(gameObject, this);
            ChemicalTickerHandler.AddTicker(this);
        }

        private void OnDisable()
        {
            InstrumentManager.RemoveInstrument(gameObject);
            InteractableManager.UnregisterInteractable(gameObject);
            ChemicalTickerHandler.RemoveTicker(this);
        }

        private void Start()
        {
            ResetLiquidDisplay();
            UpdateBubble();
        }

        private void UpdateBubble()
        {
            if (_tickGasMixture == null || _tickGasVolume <= 0 || IsEmpty())
            {
                bubbles.Stop();
                return;
            }
            
            bubbles.Play();

            var color = _tickGasMixture?.GetColor() ?? Color.white;
            
            //Note: add some black to the color to make it darker
            color.r = Mathf.Max(0, color.r - 0.1f);
            color.g = Mathf.Max(0, color.g - 0.1f);
            color.b = Mathf.Max(0, color.b - 0.1f);
            
            bubbles.SetVector4("Color", color);

            _tickGasVolume -= 0.025f;
            if (_tickGasVolume <= 0)
            {
                _tickGasMixture = null;
            }
        }

        private SpriteRenderer CreateFiller()
        {
            var filler = Instantiate(fillerPrefab, fillersParent);
            fillers.Add(filler.GetComponent<SpriteRenderer>());
            return filler.GetComponent<SpriteRenderer>();
        }

        private static void ResetFiller(SpriteRenderer filler)
        {
            filler.material.SetFloat(Fill, 0);
            filler.material.SetFloat(FillThreshold, 0);
            filler.gameObject.SetActive(false);
        }

        private void ResetLiquidDisplay()
        {
            foreach (var f in fillers)
            {
                _fillerPool.Return(f);
            }
        }

        private void UpdateLiquidContent()
        {
            ResetLiquidDisplay();
            if (_contents == null || _volume <= 0) return;
            var phases = _contents.SeparatePhases(_volume);
            phases.LiquidMixture.UpdateColor();

            var color = phases.LiquidMixture.GetColor();

            var fill = _volume / maxVolume;

            LiquidPart part = new LiquidPart { height = fill, color = color };

            _fillerParts[_fillerPool.Get()] = part;

            UpdateLiquidDisplay();
        }

        private void UpdateLiquidDisplay()
        {
            //Note: Sort the fillers by the height of the fillers from the lowest to the highest
            fillers.Sort((a, b) => _fillerParts[a].height.CompareTo(_fillerParts[b].height));

            var prevFill = 0f;
            foreach (var f in fillers)
            {
                var part = _fillerParts[f];
                UpdateFiller(f, part.height, prevFill, part.color);
                prevFill = part.height;
            }
        }

        private static void UpdateFiller(SpriteRenderer display, float fillAmount, float threshold, Color color)
        {
            var upperBound = 0.797f;
            display.material.SetFloat(FillThreshold, threshold);
            display.material.SetFloat(Fill, fillAmount);
            display.material.SetFloat(FillerUpperBound, upperBound);
            display.color = color;
            display.gameObject.SetActive(true);
        }

        public void SetHeatPower(float heatPower)
        {
            _heatPower = heatPower;
        }

        public bool IsHeating()
        {
            return _heatPower > 0;
        }

        public bool IsAttachedToHeater(out IHeater heater)
        {
            heater = _heater;
            return heater != null;
        }

        public void SetHeater(IHeater heater)
        {
            _heater = heater;
        }

        public void OnInteract()
        {
            Debug.Log("Interacted with bottle");
        }

        public List<(string name, Action onClick)> GetOptions()
        {
            var options = new List<(string name, Action onClick)>
            {
                ("View content", ViewContent),
            };

            if (IsAttachedToHeater(out var heater))
            {
                options.Add(("Detach from heater", () => HeatingUtil.DetachHeater(this, heater)));
            }
            
            _pluggable.ForEachPlugin(plugin =>
            {
                plugin.OnGetOptions(ref options);
            });

            return options;
        }

        public void OnHover()
        {
            _pluggable.ForEachPlugin(plugin =>
            {
                plugin.OnHover();
            });
        }

        public (GameObject panelObject, Action<GameObject> setupFunction) GetHoverPanel()
        {
            (GameObject panelObject, Action<GameObject> setupFunction) result = (null, null);
            _pluggable.ForEachPlugin(plugin =>
            {
                plugin.OnGetHoverPanel(ref result);
            });
            return result;
        }

        public Transform GetMainTransform()
        {
            return transform.parent;
        }

        public void OnDrop(GameObject other)
        {
            if (other == null) return;
            Debug.Log("Dropped on " + other.name);
            
            _pluggable.ForEachPlugin(plugin =>
            {
                plugin.OnDrop(other);
            });
        }

        public List<(string name, Action onClick)> GetDropOptions(GameObject other)
        {
            if (other == null) return null;

            if (!InstrumentManager.TryGetInstrument(other, out var otherInstrument)) return null;

            var options = new List<(string name, Action onClick)>();

            switch (otherInstrument)
            {
                case IHeater heater:
                    Debug.Log("Dropped on heater");
                    options.Add(("Attach to heater", () => HeatingUtil.AttachHeater(this, heater)));
                    break;
                case IMixtureContainer mixtureContainer:
                    options.Add(("Pour All", () => PourAll(this, mixtureContainer)));
                    options.Add(("Pour", () => Pour(this, mixtureContainer)));
                    break;
            }
            
            _pluggable.ForEachPlugin(plugin =>
            {
                plugin.OnGetDropOptions(other, ref options);
            });
            
            return options;
        }

        public float GetMaxVolume()
        {
            return maxVolume;
        }

        public float GetVolume()
        {
            return _volume;
        }

        public void SetVolume(float volume)
        {
            _volume = volume;
        }

        public Mixture GetMixture()
        {
            return _contents;
        }

        public void SetMixture(Mixture mixture)
        {
            _contents = mixture;
        }

        public void SetMixtureAndVolume(Mixture mixture, float volume)
        {
            SetVolume(volume);
            SetMixture(mixture);
        }

        public (Mixture mixture, float volume) GetMixtureAndVolume()
        {
            return (_contents, _volume);
        }

        public bool IsEmpty()
        {
            return _contents == null || _volume <= 0;
        }

        public void Clear()
        {
            SetMixtureAndVolume(null, 0);
        }

        public void Tick()
        {
            if (_contents != null)
            {
                // Note: Calculate the heat that is transferred to the mixture
                var heat = _heatPower;
                heat += (Environment.Instance.Temperature - _contents.GetTemperature()) *
                        heatConductivity; // Fourier's Law (sort of), the divide by 20 is for 20 ticks per second
                heat /= 20; // divide by 20 is for 20 ticks per second

                // Debug.Log("Temperature: " + Environment.Instance.Temperature + " " + _contents.GetTemperature());
                // Debug.Log("Heating: " + heat + " " + heat / (_volume * _contents.GetVolumetricHeatCapacity()));

                if (Mathf.Abs(heat / (_volume * _contents.GetVolumetricHeatCapacity())) > 0.0000001f && _volume != 0d)
                {
                    // Only bother heating if the temperature change will be somewhat significant
                    _contents.Heat(heat / _volume);
                    _contents.DisturbEquilibrium();
                }

                _contents.Tick(out var shouldUpdateFluidMixture);

                if (shouldUpdateFluidMixture)
                {
                    // Note: remove Gas from the mixture
                    var phases = _contents.SeparatePhases(_volume, true);
                    _contents = phases.LiquidMixture;
                    _volume = phases.LiquidVolume;
                    // Debug.LogError("Liquid volume: " + phases.LiquidVolume + " " +
                    //                _contents.RecalculateVolume(phases.LiquidVolume));
                    if (_tickGasMixture != null)
                    {
                        var mixtures = new Dictionary<Mixture, float>
                        {
                            { _tickGasMixture, 1f }
                        };

                        if (phases.GasVolume > 0)
                        {
                            mixtures.Add(phases.GasMixture, 1f);
                        }

                        var (tickGasMixture, newVolume) = Mixture.Mix(mixtures);

                        _tickGasMixture = tickGasMixture;

                        _tickGasVolume = phases.GasVolume;

                        _tickGasMixture.Scale(1/ newVolume);
                    }
                    else
                    {
                        _tickGasMixture = phases.GasMixture;
                    }
                }
            }

            UpdateLiquidContent();
            UpdateBubble();
        }

        private void ViewContent()
        {
            UIManager.Instance.ContentPanelController.SetupMixtureToDisplay(this);
            UIManager.Instance.ContentPanelController.OpenPanel();
        }

        private static void PourAll(IMixtureContainer original, IMixtureContainer target)
        {
            var (mixture, volume) = original.GetMixtureAndVolume();
            target.SetMixtureAndVolume(mixture, volume);
            original.SetMixtureAndVolume(null, 0);
        }

        private static void Pour(IMixtureContainer original, IMixtureContainer target)
        {
            UIManager.Instance.Utility.PouringPanelController.SetupPanel(original, target);
            UIManager.Instance.Utility.PouringPanelController.OpenPanel();
        }

        public void AddSolidMolecule(Molecule solidMolecule, float moles)
        {
            if (!solidMolecule.IsSolid())
            {
                Debug.LogError("Trying to add non-solid molecule to solid container");
                return;
            }

            if (moles <= 0)
            {
                Debug.LogError("This is mixture, cannot take out solid molecules");
                return;
            }

            Debug.Log("Adding solid molecule");
            _contents.AddMoles(solidMolecule, moles / _volume, out _);
        }

        public void RemoveSolidMolecule(Molecule solidMolecule)
        {
            Debug.LogError("This is mixture, cannot take out solid molecules");
        }

        public bool IsSolidEmpty()
        {
            return false;
        }

        public void ClearSolid()
        {
            Debug.LogError("This is mixture, cannot take out solid molecules");
        }

        public bool ContainsSolidMolecule(Molecule solidMolecule)
        {
            return _contents.ContainMolecule(solidMolecule);
        }

        public float GetSolidMoleculeAmount(Molecule solidMolecule)
        {
            return _contents.GetMoles(solidMolecule);
        }

        public bool TryGetSolidMoleculeAmount(Molecule solidMolecule, out float amount)
        {
            if (!ContainsSolidMolecule(solidMolecule))
            {
                amount = 0;
                return false;
            }

            amount = _contents.GetMoles(solidMolecule);
            return true;
        }

        public Mixture GetGasMixture()
        {
            return _tickGasMixture;
        }

        public void SetGasMixture(Mixture mixture)
        {
            _tickGasMixture = mixture;
        }

        public void AddGasMixture(Mixture mixture, float volume)
        {
            if (_tickGasMixture == null)
            {
                _tickGasMixture = mixture;
                _tickGasVolume = volume;
            }
            else
            {
                var mixtures = new Dictionary<Mixture, float>
                {
                    { _tickGasMixture, _tickGasVolume },
                    { mixture, volume }
                };

                var (tickGasMixture, newVolume) = Mixture.Mix(mixtures);

                _tickGasMixture = tickGasMixture;
                _tickGasMixture.Scale(1/newVolume);
                _tickGasVolume = 1;
            }
        }

        public bool TryGetGasMixture(out Mixture mixture)
        {
            if (_tickGasMixture == null)
            {
                mixture = null;
                return false;
            }
            if (_tickGasVolume <= 0)
            {
                mixture = null;
                return false;
            }
            mixture = _tickGasMixture;
            return true;
        }

        public bool HasGasMixture()
        {
            return _tickGasMixture != null && _tickGasVolume > 0;
        }

        public float GetGasVolume()
        {
            return _tickGasVolume;
        }
    }
}