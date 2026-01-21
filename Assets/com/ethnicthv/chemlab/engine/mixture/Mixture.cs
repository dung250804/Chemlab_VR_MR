using System;
using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.engine.api.mixture;
using com.ethnicthv.chemlab.engine.api.molecule.group;
using com.ethnicthv.chemlab.engine.api.reaction;
using com.ethnicthv.chemlab.engine.molecule;
using com.ethnicthv.chemlab.engine.molecule.group;
using com.ethnicthv.chemlab.engine.reaction;
using com.ethnicthv.chemlab.engine.util;
using com.ethnicthv.util;
using UnityEngine;
using Util = com.ethnicthv.chemlab.engine.mixture.MixtureUtil;

namespace com.ethnicthv.chemlab.engine.mixture
{
    public class Mixture : IMixture
    {
        private const float TicksPerSecond = 20;
        public static readonly float ImpurityThreshold = 0.1F;

        //Note: float is used to represent the number of moles of the element in mol in the mixture
        private readonly Dictionary<Molecule, float> _mixtureComposition = new();
        private readonly Dictionary<Molecule, float> _states = new();

        private readonly Dictionary<Molecule, float> _solidMolecules = new();

        private readonly Dictionary<MoleculeGroup, List<Molecule>> _moleculeGroups = new();

        private readonly Dictionary<ReactionResult, float> _reactionResults = new();
        private readonly LinkedList<IReactingReaction> _possibleReaction = new();

        private readonly List<Molecule> _novelMolecules = new();

        private float _temperature;
        private bool _equilibrium;
        private bool _boiling;

        private (float, Molecule) _nextHigherBoilingPoint = (float.MaxValue, null);
        private (float, Molecule) _nextLowerBoilingPoint = (0, null);

        private readonly Queue<Molecule> _newMolecules = new();
        private readonly Dictionary<Molecule, int> _moleculesToRemove = new();

        private bool _isMixtureChecked;
        private Color _color;

        private List<Molecule> _molecules; //Note: Cache the Molecules in the Mixture, remove each tick

        public static Mixture CreateMixture()
        {
            return new Mixture();
        }

        public static Mixture Pure(Molecule molecule, float state = 0.0F)
        {
            var mixture = new Mixture();
            if (molecule.GetCharge() == 0)
            {
                mixture.AddMoles(molecule, molecule.GetPureConcentration(),
                    out mixture._isMixtureChecked);
                return mixture;
            }

            var otherIon = molecule.GetCharge() < 0 ? Molecules.SodiumIon : Molecules.Chloride;
            var chargeMagnitude = Math.Abs(molecule.GetCharge());
            mixture.AddMoles(molecule, 1.0F, out mixture._isMixtureChecked);
            mixture.AddMoles(otherIon, chargeMagnitude, out mixture._isMixtureChecked);

#pragma warning disable CS0612 // Type or member is obsolete
            mixture.RecalculateVolume(1000);
#pragma warning restore CS0612 // Type or member is obsolete

            mixture.CheckMixture();
            return mixture;
        }

        private Mixture()
        {
            _temperature = 298.15F;
        }

        public Color GetColor()
        {
            return _color;
        }

        public bool IsBoiling()
        {
            return _boiling;
        }

        public Mixture SetTemperature(float temperature)
        {
            _temperature = temperature;
            foreach (var molecule in _mixtureComposition.Keys)
            {
                if (molecule.GetBoilingPoint() < temperature)
                {
                    _states[molecule] = 1.0F;
                }
                else
                {
                    _states[molecule] = 0.0F;
                }
            }

            return this;
        }

        public float GetTemperature()
        {
            return _temperature;
        }

        public void SetState(Molecule molecule, float state)
        {
            if (!(state < 0.0F) && !(state > 1.0F))
            {
                if (GetMoles(molecule) > 0.0F)
                {
                    _states.Add(molecule, state);
                }
            }
            else
            {
                throw new Exception("Molecules can range from entirely liquid (state = 0) to entirely gas (state = 1)");
            }
        }

        public void Tick(out bool shouldUpdateFluidMixture)
        {
            shouldUpdateFluidMixture = false;
            _molecules = null;
            if (!_isMixtureChecked)
            {
                CheckMixture();
                _isMixtureChecked = true;
            }

            if (!_equilibrium)
            {
                RunReactions();
                shouldUpdateFluidMixture = true;
            }
            // else
            // {
            //     // Debug.Log("Mixture is at equilibrium");
            // }

            // Note: Remove _moleculesToRemove
            foreach (var molecule in _moleculesToRemove.Keys)
            {
                if (molecule.IsSolid())
                {
                    _solidMolecules.Remove(molecule);
                    continue;
                }
                _mixtureComposition.Remove(molecule);
            }

            _moleculesToRemove.Clear();
        }

        public void RemoveMolecule(Molecule molecule)
        {
            Util.RemoveMolecule(molecule, _moleculesToRemove, out _isMixtureChecked);
        }

        public void SetMoles(Molecule molecule, float moles)
        {
            if (moles <= 0.0F)
            {
                throw new Exception(
                    "Set Moles in Mixture must be greater than 0, if you want to remove a molecule, use RemoveMolecule()");
            }

            var composition2Mutate = molecule.IsSolid() ? _solidMolecules : _mixtureComposition;

            if (!composition2Mutate.ContainsKey(molecule))
            {
                Util.AddMolecule(molecule, moles, _temperature,
                    _newMolecules, GetContent(), _states,
                    _novelMolecules,
                    out _isMixtureChecked);
                return;
            }

            composition2Mutate[molecule] = moles;
        }

        public float GetMoles(Molecule molecule)
        {
            var t = GetContent();
            return molecule.IsSolid() ? _solidMolecules.TryGetValue(molecule, out var valueSolid) ? valueSolid : 0
                : t.TryGetValue(molecule, out var value) ? value : 0;
        }

        public float AddMoles(Molecule molecule, float moles, out bool isMutating)
        {
            var isSolid = molecule.IsSolid();
            
            var t = Util.AddMoles(molecule, moles, _temperature,
                _newMolecules, _moleculesToRemove,
                isSolid ? _solidMolecules : GetContent(), _states, _novelMolecules,
                ref _isMixtureChecked);
            isMutating = _isMixtureChecked;
            
            if (molecule.GetBoilingPoint() < _temperature)
            {
                _states[molecule] = 1.0F;
            }

            return t;
        }

        public IReadOnlyDictionary<Molecule, float> GetMixtureComposition()
        {
            return _mixtureComposition;
        }

        public void ClearMixture()
        {
            _mixtureComposition.Clear();
            _solidMolecules.Clear();
            _isMixtureChecked = false;
        }

        public void Scale(float volumeIncreaseFactor)
        {
            var keys = _mixtureComposition.Keys.ToList();
            foreach (var key in keys)
            {
                _mixtureComposition[key] /= volumeIncreaseFactor;
            }

            var reactionKeys = _reactionResults.Keys.ToList();
            foreach (var key in reactionKeys)
            {
                _reactionResults[key] /= volumeIncreaseFactor;
            }
        }

        public Phases SeparatePhases(float initialVolume, bool doCheck = false)
        {
            Dictionary<Molecule, float> liquidMoles = new();
            Dictionary<Molecule, float> gasMoles = new();
            Dictionary<Molecule, float> solidMoles = new();

            var newLiquidVolume = 0.0F;
            var newGasVolume = 1.0F;

            var liquidMixture = new Mixture();
            var gasMixture = new Mixture();
            
            foreach (var (molecule, concentration) in _mixtureComposition)
            {
                var proportionGaseous = _states[molecule];

                //Note: Liquid
                var molesOfLiquidMolecule = concentration * (1.0F - proportionGaseous) * initialVolume;
                liquidMoles[molecule] = molesOfLiquidMolecule;
                newLiquidVolume += molesOfLiquidMolecule / molecule.GetPureConcentration();

                //Note: Gas
                gasMoles[molecule] = concentration * proportionGaseous * initialVolume;
            }

            foreach (var (molecule, moles) in _solidMolecules)
            {
                solidMoles[molecule] = moles * initialVolume;
            }

            foreach (var (molecule, resultMoles) in liquidMoles)
            {
                if (resultMoles == 0.0F) continue;
                liquidMixture.AddMoles(molecule, resultMoles / newLiquidVolume, out liquidMixture._isMixtureChecked);
                liquidMixture._states[molecule] = 0.0F;
            }

            var emptyGas = true;
            
            foreach (var (molecule, resultMoles) in gasMoles)
            {
                if (resultMoles < 0.000000001F) continue;
                emptyGas = false;
                gasMixture.AddMoles(molecule, resultMoles / newGasVolume, out gasMixture._isMixtureChecked);
                gasMixture._states[molecule] = 1.0F;
            }

            foreach (var (molecule, resultMoles) in solidMoles)
            {
                if (resultMoles == 0.0F) continue;
                liquidMixture.AddMoles(molecule, resultMoles / newLiquidVolume, out liquidMixture._isMixtureChecked);
            }

            foreach (var entry in _reactionResults)
            {
                var resultMoles = entry.Value * initialVolume;
                var newTotalVolume = newLiquidVolume * newGasVolume;
                liquidMixture._reactionResults[entry.Key] = resultMoles / newTotalVolume;
                gasMixture._reactionResults[entry.Key] = resultMoles / newTotalVolume;
            }

            liquidMixture._temperature = _temperature;
            gasMixture._temperature = _temperature;

            if (doCheck)
            {
                liquidMixture.CheckMixture();
                gasMixture.CheckMixture();
            }

            liquidMixture._equilibrium = _equilibrium;
            gasMixture._equilibrium = _equilibrium;

            if (emptyGas)
            {
                newGasVolume = 0.0F;
            }

            return new Phases(gasMixture, newGasVolume, liquidMixture, newLiquidVolume);
        }

        public static (Mixture mixture, float amout) Mix(Dictionary<Mixture, float> mixtures)
        {
            switch (mixtures.Count)
            {
                case 0:
                    return (new Mixture(), 0);
                case 1:
                    var firstKey = mixtures.Keys.GetEnumerator().Current;
                    return firstKey == null ? (new Mixture(), 0) : (firstKey,  mixtures[firstKey]);
            }

            var resultMixture = new Mixture();
            Dictionary<Molecule, float> moleculesAndMoles = new();
            Dictionary<ReactionResult, float> reactionResultsAndMoles = new();
            var totalAmount = 0.0f;
            var totalEnergy = 0.0F;

            foreach (var (mixture, amount) in mixtures)
            {
                totalAmount += amount;

                foreach (var molecule in mixture.GetMolecules())
                {
                    var concentration = mixture.GetMoles(molecule);
                    moleculesAndMoles[molecule] = moleculesAndMoles.GetValueOrDefault(molecule, 0)
                                                  + concentration * amount;
                    totalEnergy += molecule.GetLatentHeat() * concentration *
                                   mixture._states.GetValueOrDefault(molecule, 0) * amount;
                    totalEnergy += molecule.GetMolarHeatCapacity() * concentration * mixture._temperature * amount;
                }

                foreach (var entry in mixture._reactionResults)
                {
                    reactionResultsAndMoles[entry.Key] =
                        reactionResultsAndMoles.GetValueOrDefault(entry.Key, 0) + entry.Value * amount;
                }
            }

            foreach (var (molecule, value) in moleculesAndMoles)
            {
                //resultMixture.internalAddMolecule(molecule, value / totalAmount, false);

                if (molecule.IsSolid())
                {
                    MixtureUtil.AddMolecule(molecule, value / totalAmount, resultMixture._temperature,
                        resultMixture._newMolecules,
                        resultMixture._solidMolecules,
                        resultMixture._states,
                        resultMixture._novelMolecules,
                        out resultMixture._isMixtureChecked);
                    continue;
                }

                MixtureUtil.AddMolecule(molecule, value / totalAmount, resultMixture._temperature,
                    resultMixture._newMolecules,
                    resultMixture._mixtureComposition,
                    resultMixture._states,
                    resultMixture._novelMolecules,
                    out resultMixture._isMixtureChecked);

                resultMixture._states[molecule] = 0.0F;
            }

            foreach (var (reactionResult, value) in reactionResultsAndMoles)
            {
                if (reactionResult.GetReaction() != null)
                {
                    resultMixture.IncrementReactionResults(reactionResult.GetReaction(),
                        value / totalAmount);
                }
            }

            resultMixture._temperature = 0.0F;
            resultMixture.UpdateNextBoilingPoints();
            resultMixture.Heat(totalEnergy / totalAmount);

            resultMixture.CheckMixture();
            resultMixture.UpdateColor();
            resultMixture.UpdateNextBoilingPoints();
            return (resultMixture, totalAmount);
        }

        public void Heat(float energyDensity)
        {
            var volumetricHeatCapacity = GetVolumetricHeatCapacity();

            if (volumetricHeatCapacity == 0.0F) return;

            var temperatureChange = energyDensity / volumetricHeatCapacity;

            if (temperatureChange == 0.0F) return;
            Molecule molecule;
            float liquidConcentration;
            float energyRequiredToFullyBoil;
            float boiled;
            if (temperatureChange > 0.0F) // If the temperature would be increasing
            {
                if (_nextHigherBoilingPoint.Item2 != null
                    && _temperature + temperatureChange >= _nextHigherBoilingPoint.Item1)
                {
                    // If a Molecule needs to boil before the temperature can change
                    temperatureChange =
                        _nextHigherBoilingPoint.Item1 -
                        _temperature; // Only increase the temperature by enough to get to the next BP
                    _temperature += temperatureChange; // Raise the Mixture to the boiling point
                    energyDensity -=
                        temperatureChange *
                        volumetricHeatCapacity; // Energy leftover once the Mixture has been raised to the boiling point
                    molecule = _nextHigherBoilingPoint.Item2;
                    liquidConcentration =
                        GetMoles(molecule) * (1.0F - _states[molecule]); // The moles per bucket of liquid Molecules
                    energyRequiredToFullyBoil =
                        liquidConcentration *
                        molecule.GetLatentHeat(); // The energy density required to boil all remaining liquid

                    if (energyDensity > energyRequiredToFullyBoil)
                    {
                        // If there is leftover energy once the Molecule has been boiled
                        _states[molecule] = 1; // Convert the Molecule fully to gas
                        UpdateNextBoilingPoints(true);
                        _boiling =
                            false; // If we're just increasing the temperature, then all Molecule are either fully gaseous or liquid
                        Heat(energyDensity - energyRequiredToFullyBoil); // Continue heating
                    }
                    else
                    {
                        // If there is no leftover energy and the Molecule is still boiling
                        boiled = energyDensity /
                                 (molecule.GetLatentHeat() *
                                  GetMoles(
                                      molecule)); // The proportion of all of the Molecule which is additionally boiled

                        if (_states.ContainsKey(molecule))
                        {
                            _states[molecule] += boiled;
                        }
                        else
                        {
                            _states[molecule] = boiled;
                        }

                        _boiling =
                            true; // Set the fact that there is a Molecule which will be not fully gaseous or liquid
                    }

                    _equilibrium = false; // Equilibrium is broken when a Molecule boils
                }
                else
                {
                    _temperature += temperatureChange;
                }
            } // If the temperature would be decreasing
            else if (_nextLowerBoilingPoint.Item2 != null &&
                     _temperature + temperatureChange < _nextLowerBoilingPoint.Item1)
            {
                // If a Molecule needs to condense before the temperature can change
                temperatureChange =
                    _nextLowerBoilingPoint.Item1 -
                    _temperature; // Only decrease the temperature by enough to get to the next condensation point
                _temperature += temperatureChange; // Decrease the Mixture to the boiling point
                energyDensity -=
                    temperatureChange *
                    volumetricHeatCapacity; // Additional energy once the Mixture has been lowered to the condensation point

                molecule = _nextLowerBoilingPoint.Item2;
                liquidConcentration = GetMoles(molecule) * _states[molecule];
                energyRequiredToFullyBoil =
                    liquidConcentration *
                    molecule.GetLatentHeat(); // The energy density which could be released when all remaining gas is condensed
                if (energyDensity < -energyRequiredToFullyBoil)
                {
                    // If there is more energy that needs to be released than the condensation can supply
                    _states[molecule] = 0; // Convert the Molecule fully to liquid
                    UpdateNextBoilingPoints(true);
                    _boiling = false; // If we're just increasing the temperature, then all Molecule are either fully gaseous or liquid
                    Heat(energyDensity + energyRequiredToFullyBoil); // Continue cooling
                }
                else
                {
                    boiled = -energyDensity / (molecule.GetLatentHeat() * GetMoles(molecule));
                    if (_states.ContainsKey(molecule))
                    {
                        _states[molecule] = _states[molecule] + 1.0F - boiled - 1.0F;
                    }
                    else
                    {
                        _states[molecule] = 1.0F - boiled;
                    }

                    _boiling = true; // Set the fact that a Molecule is currently not fully gaseous or liquid
                }

                _equilibrium = false; // Equilibrium is broken when a Molecule condenses
            }
            else
            {
                _temperature += temperatureChange;
            }

            _temperature = Math.Max(_temperature, 1.0E-4F);
        }

        public float GetVolumetricHeatCapacity()
        {
            var totalHeatCapacity = 0.0F;

            foreach (var entry in _mixtureComposition)
            {
                totalHeatCapacity += entry.Key.GetMolarHeatCapacity() * entry.Value;
            }

            return totalHeatCapacity;
        }

        public bool IsAtEquilibrium()
        {
            return _equilibrium;
        }

        public void DisturbEquilibrium()
        {
            _equilibrium = false;
        }

        public void UpdateColor()
        {
            var totalColorContribution = 0.0F;
            var totalRed = 0.0F;
            var totalGreen = 0.0F;
            var totalBlue = 0.0F;
            var totalAlpha = 64 / 255f;

            foreach (var entry in _mixtureComposition)
            {
                var color = ColorUtil.GetColor(entry.Key.GetColor());
                var colorContribution = entry.Value * color.a;
                totalColorContribution += colorContribution;
                totalRed += color.r * colorContribution;
                totalGreen += color.g * colorContribution;
                totalBlue += color.b * colorContribution;
                totalAlpha = Mathf.Max(totalAlpha, color.a);
            }

            _color = new Color(totalRed / totalColorContribution,
                totalGreen / totalColorContribution,
                totalBlue / totalColorContribution,
                totalAlpha);
        }

        [Obsolete]
        public float RecalculateVolume(float initialVolume)
        {
            if (_mixtureComposition.Count == 0)
            {
                return 0;
            }

            var initialVolumeInLiters = initialVolume / 1.0f;
            var newVolumeInLiters = 0.0f;
            Dictionary<Molecule, float> molesOfMolecules = new();

            foreach (var (molecule, value) in _mixtureComposition)
            {
                var molesOfMolecule = value * initialVolumeInLiters;
                molesOfMolecules[molecule] = molesOfMolecule;
                newVolumeInLiters += molesOfMolecule / molecule.GetPureConcentration();
            }

            foreach (var entry in molesOfMolecules)
            {
                _mixtureComposition[entry.Key] = entry.Value / newVolumeInLiters;
            }

            Dictionary<ReactionResult, float> resultsCopy = new(_reactionResults);
            foreach (var entry in resultsCopy)
            {
                _reactionResults[entry.Key] = entry.Value * initialVolumeInLiters / newVolumeInLiters;
            }

            return newVolumeInLiters * 1.0F;
        }

        private void AddToGroup(MoleculeGroup getGroup, Molecule molecule)
        {
            if (!_moleculeGroups.ContainsKey(getGroup)) _moleculeGroups.Add(getGroup, new List<Molecule>());
            _moleculeGroups[getGroup].Add(molecule);
        }

        #region Private Action

        private float CalculateReactionRate(IReactingReaction reaction, ReactionContext context)
        {
            var rate = reaction.GetRateConstant(_temperature) / TicksPerSecond;
            foreach (var molecule in reaction.GetOrders().Keys)
            {
                rate *= (float)Math.Pow(GetMoles(molecule), reaction.GetOrders()[molecule]);
            }

            //if (reaction.NeedsUV()) rate *= context.UVPower;
            return rate;
        }

        private void CheckMixture()
        {
            // Debug.LogWarning("Checking Mixture");
            foreach (var molecule in _newMolecules)
            {
                GroupDetectingProgram.Instance.CheckMolecule(molecule, out var groups);

                foreach (var group in groups)
                {
                    // Debug.Log("Group: " + group);
                    AddToGroup(group, molecule);
                }
            }

            _newMolecules.Clear();
            _possibleReaction.Clear();
            var newPossibleReactions = new CustomList<IReactingReaction>();

            //Note: Add Dynamic Reaction
            ReactionProgram.Instance.CheckForReaction(GetReactionContext(), in newPossibleReactions);

            //Note: Add Static Reaction
            foreach (var reaction in _mixtureComposition.Keys.SelectMany(possibleReactant =>
                         possibleReactant.GetReactantReactions()))
            {
                newPossibleReactions.Push(reaction);
            }

            // Debug.Log("New Possible Reactions: " + newPossibleReactions.GetList().Count);
            // Debug.Log(newPossibleReactions.GetList()
            //     .Aggregate("", (current, reaction) => current + reaction.GetId() + "\n"));

            foreach (var possibleReaction in newPossibleReactions.GetList())
            {
                if (possibleReaction.GetOrders().Keys
                    .Any(necessaryReactantOrCatalyst => GetMoles(necessaryReactantOrCatalyst) == 0))
                    continue; // Skip Reactions which require a reactant or catalyst which is not present
                _possibleReaction.AddLast(possibleReaction);
            }

            UpdateNextBoilingPoints();
        }

        private void RunReactions()
        {
            // Debug.LogWarning("Running Reactions");
            var context = GetReactionContext();
            _equilibrium = true; // Start by assuming we have reached equilibrium

            Dictionary<Molecule, float> oldContents = new(_mixtureComposition);

            // Debug.LogWarning(oldContents.Keys.Aggregate("" , (current, molecule) => current + molecule.GetFullID() + " Moles: " + oldContents[molecule] + "\n"));

            Dictionary<IReactingReaction, float> reactionRates = new(); // Rates of all Reactions
            List<IReactingReaction>
                orderedReactions = new(); // A list of Reactions in the order of their current rate, fastest first

            foreach (var possibleReaction in _possibleReaction)
            {
                if (possibleReaction.GetSolidReactantsAndCatalysts().Any(
                        necessaryReactantOrCatalyst => GetMoles(necessaryReactantOrCatalyst) <= 0))
                    continue; // Check all Reactions have the necessary Item catalysts

                reactionRates[possibleReaction] =
                    CalculateReactionRate(possibleReaction, context); // Calculate the Reaction data for this sub-tick
                orderedReactions.Add(
                    possibleReaction); // Add the Reaction to the rate-ordered list, which is currently not sorted
            }

            orderedReactions.Sort((a, b) =>
                reactionRates[b].CompareTo(reactionRates[a])); // Sort the Reactions by their rate, fastest first

            // Note: Do Reaction
            foreach (var r in orderedReactions)
            {
                // Go through each Reaction, fastest first
                var molesOfReaction =
                    reactionRates
                        [r]; // We are reacting over one tick, so moles of Reaction that take place in this time = rate of Reaction in M per sub-tick

                foreach (var reactant in r.GetReactants())
                {
                    var reactantMolarRatio = r.GetReactantMolarRatio(reactant);
                    var reactantConcentration = GetMoles(reactant);
                    if (reactantConcentration < reactantMolarRatio * molesOfReaction)
                    {
                        // Determine the limiting reagent, if there is one
                        molesOfReaction = reactantConcentration /
                                          reactantMolarRatio; // If there is a new limiting reagent, alter the moles of reaction which will take place
                    }
                }

                if (molesOfReaction <= 0.0F) // Don't bother going any further if this Reaction won't happen
                {
                    continue;
                }

                _isMixtureChecked |=
                    DoReaction(r,
                        molesOfReaction); // Increment the amount of this Reaction which has occured, add all products and remove all reactants
            }

            // Check now if we have actually reached equilibrium or if that was a false assumption at the start
            foreach (var molecule in oldContents.Keys)
            {
                if (!Mathf.Approximately(oldContents[molecule], GetMoles(molecule)))
                {
                    // If there's something that has changed concentration noticeably in this tick...
                    _equilibrium = false; // ...we cannot have reached equilibrium
                    break;
                }
            }

            // if (_equilibrium)
            // {
            //     Debug.LogError("Mixture is at equilibrium");
            //     foreach (var m in oldContents.Keys)
            //     {
            //         Debug.LogError("Molecule: " + m.GetFullID() + " Old Moles: " + oldContents[m] + " New Moles: " +
            //                        GetMoles(m));
            //     }
            // }
        }

        private bool DoReaction(IReactingReaction reaction, float molesPerLiter)
        {
            // Debug.Log("Do Reaction: " + reaction.GetId() + " Moles: " + molesPerLiter);
            var shouldRefreshPossibleReactions = false;
            foreach (var reactant in reaction.GetReactants())
            {
                AddMoles(reactant, -(molesPerLiter * reaction.GetReactantMolarRatio(reactant)),
                    out shouldRefreshPossibleReactions);
                // Debug.Log("Reactant: " + reactant.GetFullID() + " Moles: " + GetMoles(reactant));
            }

            foreach (var product in reaction.GetProducts())
            {
                AddMoles(product, molesPerLiter * reaction.GetProductMolarRatio(product),
                    out shouldRefreshPossibleReactions);
                // Debug.Log("Product: " + product.GetFullID() + " Moles: " + GetMoles(product));
            }

            Heat(-reaction.GetEnthalpyChange() * 1000.0F * molesPerLiter);
            IncrementReactionResults(reaction, molesPerLiter);
            return shouldRefreshPossibleReactions;
        }

        private void IncrementReactionResults(IReactingReaction reaction, float molesPerBucket)
        {
            if (!reaction.HasResult()) return;

            var result = reaction.GetResult();

            if (_reactionResults.ContainsKey(result))
            {
                _reactionResults[result] += molesPerBucket;
            }
            else
            {
                _reactionResults[result] = molesPerBucket;
            }
        }

        private void UpdateNextBoilingPoints(bool ignoreCurrentTemperature = false)
        {
            _nextHigherBoilingPoint = (float.MaxValue, null);
            _nextLowerBoilingPoint = (0f, null);
            foreach (var molecule in _mixtureComposition.Keys)
            {
                var bp = molecule.GetBoilingPoint();
                if (bp < _temperature || (Math.Abs(bp - _temperature) < 0.001 && !ignoreCurrentTemperature))
                {
                    if (bp > _nextLowerBoilingPoint.Item1) _nextLowerBoilingPoint = (bp, molecule);
                }

                if (bp > _temperature || (Math.Abs(bp - _temperature) < 0.001 && !ignoreCurrentTemperature))
                {
                    // If the boiling point is higher than the current temperature.
                    if (bp < _nextHigherBoilingPoint.Item1) _nextHigherBoilingPoint = (bp, molecule);
                }
            }
        }

        #endregion

        #region Context Builders

        private ReactionTickContext GetReactionTickContext()
        {
            return new ReactionTickContext();
        }

        private ReactionContext GetReactionContext()
        {
            //Note: Get full Mixture composition

            var contextComposition = new Dictionary<Molecule, float>(_mixtureComposition);

            foreach (var (molecule, value) in _solidMolecules)
            {
                if (contextComposition.ContainsKey(molecule))
                {
                    contextComposition[molecule] += value;
                }
                else
                {
                    contextComposition[molecule] = value;
                }
            }

            return new ReactionContext(_moleculeGroups, contextComposition);
        }

        #endregion

        private Dictionary<Molecule, float> GetContent()
        {
            return _mixtureComposition;
        }

        public IReadOnlyList<Molecule> GetMolecules()
        {
            if (_molecules != null) return _molecules;
            //Note: Get all Molecules in the Mixture, includes solid Molecules
            var molecules = new List<Molecule>(_mixtureComposition.Keys);
            molecules.AddRange(_solidMolecules.Keys);
            return molecules;
        }

        public bool ContainMolecule(Molecule solidMolecule)
        {
            return _mixtureComposition.ContainsKey(solidMolecule) || _solidMolecules.ContainsKey(solidMolecule);
        }
    }
}