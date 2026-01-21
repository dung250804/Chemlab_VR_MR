using System;
using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.engine;
using com.ethnicthv.chemlab.engine.api;
using com.ethnicthv.chemlab.engine.api.mixture;
using com.ethnicthv.chemlab.engine.mixture;
using com.ethnicthv.chemlab.engine.molecule;
using UnityEngine;
using UnityEngine.VFX;

namespace com.ethnicthv.chemlab.client.game.plugin
{
    public class ChemicalTubeBurnerPlugin : MonoBehaviour, IInteractablePlugin, IChemicalTicker, IIgnitable
    {
        [SerializeField] private BottleBehaviour bottleBehaviour;
        [SerializeField] private VisualEffect fireEffect;

        private bool _isIgniting = false;

        private void Start()
        {
            fireEffect.Play();
            fireEffect.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            ChemicalTickerHandler.AddTicker(this);
        }

        private void OnDisable()
        {
            ChemicalTickerHandler.RemoveTicker(this);
        }

        public void OnGetOptions(ref List<(string name, Action onClick)> options)
        {
            options.Add(("Toggle Burn", Burn));
        }

        private void Burn()
        {
            _isIgniting = !_isIgniting;
        }

        private static bool CheckIfAnyGasBurnable(IMixture gasMixture, out Dictionary<Molecule, float> burnableGas)
        {
            burnableGas = gasMixture.GetMolecules().Where(molecule => molecule.IsBurnable())
                .ToDictionary(molecule => molecule, gasMixture.GetMoles);

            if (burnableGas.Count > 0)
            {
                return true;
            }

            burnableGas = null;
            return false;
        }

        private static Color GetBurnColor(Dictionary<Molecule, float> burnableGas)
        {
            //Note: get burn color from gas molecule and blend it to get the color of the fire

            var finalColor = Color.black;
            var totalWeight = 0f;

            foreach (var (molecule, moles) in burnableGas)
            {
                var intensity = molecule.GetBurnIntensity();
                var color = molecule.GetBurnColor();
                
                // Tính trọng số của chất: moles * intensity
                var weight = moles * intensity;

                // Cộng dồn màu sắc có trọng số
                finalColor += color * weight;

                // Cộng dồn tổng trọng số
                totalWeight += weight;
            }

            // Chuẩn hóa màu về khoảng [0, 1]
            if (totalWeight > 0)
                finalColor /= totalWeight;
            else
                finalColor = Color.black;

            return finalColor;
        }

        public void Tick()
        {
            if (bottleBehaviour.HasGasMixture() && _isIgniting)
            {
                Debug.LogWarning("Has gas mixture");
                if (CheckIfAnyGasBurnable(bottleBehaviour.GetGasMixture(), out var burnableGas))
                {
                    fireEffect.gameObject.SetActive(true);
                    Debug.LogWarning("Has burnable gas");
                    fireEffect.SetVector4("Color", GetBurnColor(burnableGas));
                    fireEffect.Play();
                }
                else
                {
                    fireEffect.gameObject.SetActive(false);
                    _isIgniting = false;
                }
            }
            else
            {
                fireEffect.gameObject.SetActive(false);
                _isIgniting = false;
            }
        }

        public void Ignite()
        {
            _isIgniting = true;
        }
    }
}