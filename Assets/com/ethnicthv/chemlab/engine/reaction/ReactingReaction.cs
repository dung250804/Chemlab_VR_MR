using System;
using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.engine.api.error.reaction;
using com.ethnicthv.chemlab.engine.api.reaction;
using com.ethnicthv.chemlab.engine.molecule;
using com.ethnicthv.chemlab.engine.util;
using UnityEngine;

namespace com.ethnicthv.chemlab.engine.reaction
{
    public class ReactingReaction : IReactingReaction
    {
        public static readonly float GasConstant = 8.3145F;
        public static readonly int PriorityDefault = 0;
        
        private static readonly Dictionary<string, ReactingReaction> Reactions = new();
        
        public static ReactingReaction Get(string reactionId) {
            return Reactions[reactionId];
        }
        
        public static IEnumerable<IReactingReaction> GetAllReactions()
        {
            return new List<IReactingReaction>(Reactions.Values.ToList());
        }
        
        public static ReactionBuilder CreateBuilder()
        {
            return new ReactionBuilder();
        }

        private Dictionary<Molecule, int> _reactants;
        private Dictionary<Molecule, int> _products;
        private Dictionary<Molecule, int> _orders;

        private List<Molecule> _solids;

        private ReactionResult _result;

        private float _preexponentialFactor;
        private float _activationEnergy;
        private float _enthalpyChange;
        private float _standardHalfCellPotential;
        private int _electrons;

        private ReactingReaction _reverseReaction;
        private string _id;

        public int CompareTo(IReactingReaction other)
        {
            return GetPriority().CompareTo(other.GetPriority());
        }

        public int GetPriority()
        {
            return PriorityDefault;
        }

        // public abstract Dictionary<Molecule, float> GetConsumedMolecules(ReactionTickContext context);
        // public abstract Dictionary<Molecule, float> GetProducedMolecules(ReactionTickContext context);

        public bool ContainsReactant(Molecule molecule)
        {
            return _reactants.Keys.Contains(molecule);
        }

        public bool ContainsProduct(Molecule molecule)
        {
            return _products.Keys.Contains(molecule);
        }


        public IReadOnlyList<Molecule> GetReactants()
        {
            return new List<Molecule>(_reactants.Keys);
        }

        public IReadOnlyList<Molecule> GetProducts()
        {
            return new List<Molecule>(_products.Keys);
        }

        public float GetPreexponentialFactor()
        {
            return _preexponentialFactor;
        }

        public float GetActivationEnergy()
        {
            return _activationEnergy;
        }

        public float GetEnthalpyChange()
        {
            return _enthalpyChange;
        }

        public bool IsConsumedSolid()
        {
            GetSolidReactantsAndCatalysts();
            return _solids.Any(molecule => _reactants.ContainsKey(molecule));
        }

        public bool IsMoleculeCatalyst(Molecule molecule)
        {
            return _orders.ContainsKey(molecule) && !_reactants.ContainsKey(molecule) && !_products.ContainsKey(molecule);
        }

        public IReadOnlyList<Molecule> GetSolidReactants()
        {
            GetSolidReactantsAndCatalysts();
            return _solids.Where(molecule => _reactants.ContainsKey(molecule)).ToList();
        }

        public IReadOnlyList<Molecule> GetSolidReactantsAndCatalysts()
        {
            return _solids ??= new List<Molecule>(_orders.Keys);
        }

        public float GetRateConstant(float temperature)
        {
            return _preexponentialFactor * (float)Math.Exp(-(_activationEnergy * 1000.0F / (8.3145F * temperature)));
        }

        public bool HasResult()
        {
            return _result != null;
        }

        public ReactionResult GetResult()
        {
            return _result;
        }

        public int GetReactantMolarRatio(Molecule reactant)
        {
            return !_reactants.ContainsKey(reactant) ? 0 : _reactants[reactant];
        }

        public int GetProductMolarRatio(Molecule product)
        {
            return !_products.ContainsKey(product) ? 0 : _products[product];
        }

        public Dictionary<Molecule, int> GetOrders()
        {
            return _orders;
        }

        public float GetStandardHalfCellPotential()
        {
            return _standardHalfCellPotential;
        }

        public int GetElectronsTransferred()
        {
            return _electrons;
        }

        public bool IsHalfReaction()
        {
            return _electrons != 0;
        }

        public class ReactionBuilder
        {
            private readonly IOnlyPushList<IReactingReaction> _results;
            
            private readonly bool _generated;
            private readonly ReactingReaction _reaction;
            private readonly bool _declaredAsReverse;
            private bool _hasForcedPreExponentialFactor;
            private bool _hasForcedActivationEnergy;
            private bool _hasForcedEnthalpyChange;
            private bool _hasForcedHalfCellPotential;

            internal ReactionBuilder(in IOnlyPushList<IReactingReaction> results , ReactingReaction reaction, bool generated, bool declaredAsReverse)
            {
                _generated = generated;
                _reaction = reaction;
                _declaredAsReverse = declaredAsReverse;
                reaction._reactants = new Dictionary<Molecule, int>();
                reaction._products = new Dictionary<Molecule, int>();
                reaction._orders = new Dictionary<Molecule, int>();
                _results = results;
                _hasForcedPreExponentialFactor = false;
                _hasForcedActivationEnergy = false;
                _hasForcedEnthalpyChange = false;
                _hasForcedHalfCellPotential = false;
            }

            internal ReactionBuilder(in IOnlyPushList<IReactingReaction> results)
            {
                _reaction = new ReactingReaction();
                _generated = false;
                _declaredAsReverse = false;
                _reaction._reactants = new Dictionary<Molecule, int>();
                _reaction._products = new Dictionary<Molecule, int>();
                _reaction._orders = new Dictionary<Molecule, int>();
                _results = results;
                _hasForcedPreExponentialFactor = false;
                _hasForcedActivationEnergy = false;
                _hasForcedEnthalpyChange = false;
                _hasForcedHalfCellPotential = false;
            }

            internal ReactionBuilder()
            {
                _reaction = new ReactingReaction();
                _generated = false;
                _declaredAsReverse = false;
                _reaction._reactants = new Dictionary<Molecule, int>();
                _reaction._products = new Dictionary<Molecule, int>();
                _reaction._orders = new Dictionary<Molecule, int>();
                _results = null;
                _hasForcedPreExponentialFactor = false;
                _hasForcedActivationEnergy = false;
                _hasForcedEnthalpyChange = false;
                _hasForcedHalfCellPotential = false;
            }

            private void CheckNull(Molecule molecule)
            {
                if (molecule == null)
                {
                    throw E("Molecules cannot be null");
                }
            }

            public ReactionBuilder ID(String id)
            {
                _reaction._id = id;
                return this;
            }

            public ReactionBuilder AddReactant(Molecule molecule)
            {
                return AddReactant(molecule, 1);
            }

            public ReactionBuilder AddReactant(Molecule molecule, int ratio)
            {
                return AddReactant(molecule, ratio, ratio);
            }

            public ReactionBuilder AddReactant(Molecule molecule, int ratio, int order)
            {
                CheckNull(molecule);
                _reaction._reactants.Add(molecule, ratio);
                _reaction._orders.Add(molecule, order);
                return this;
            }

            public ReactionBuilder SetOrder(Molecule molecule, int order)
            {
                if (!_reaction._reactants.Keys.Contains(molecule))
                {
                    throw E("Cannot modify order of a Molecule (" + molecule.GetFullID() +
                            ") that is not a reactant.");
                }

                AddCatalyst(molecule, order);
                return this;
            }

            public ReactionBuilder AddProduct(Molecule molecule)
            {
                return AddProduct(molecule, 1);
            }

            public ReactionBuilder AddProduct(Molecule molecule, int ratio)
            {
                CheckNull(molecule);
                _reaction._products.Add(molecule, ratio);
                return this;
            }

            public ReactionBuilder AddCatalyst(Molecule molecule, int order)
            {
                CheckNull(molecule);
                _reaction._orders.Add(molecule, order);
                return this;
            }

            public ReactionBuilder PreexponentialFactor(float preexponentialFactor)
            {
                _reaction._preexponentialFactor = preexponentialFactor;
                _hasForcedPreExponentialFactor = true;
                return this;
            }

            public ReactionBuilder ActivationEnergy(float activationEnergy)
            {
                _reaction._activationEnergy = activationEnergy;
                _hasForcedActivationEnergy = true;
                return this;
            }

            public ReactionBuilder EnthalpyChange(float enthalpyChange)
            {
                _reaction._enthalpyChange = enthalpyChange;
                _hasForcedEnthalpyChange = true;
                return this;
            }

            public ReactionBuilder StandardHalfCellPotential(float standardHalfCellPotential)
            {
                if (_hasForcedHalfCellPotential)
                {
                    throw E("Cannot set half-cell potential more than once.");
                }

                _reaction._standardHalfCellPotential = standardHalfCellPotential;
                _hasForcedHalfCellPotential = true;
                return this;
            }

            public ReactionBuilder WithResult(float moles,
                Func<float, ReactingReaction, ReactionResult> reactionResultFactory)
            {
                if (_reaction._result != null)
                {
                    throw E(
                        "Reaction already has a Reaction Result. Use a CombinedReactionResult to have multiple.");
                }

                _reaction._result = reactionResultFactory(moles, _reaction);
                return this;
            }

            public ReactingReaction Acid(Molecule acid, Molecule conjugateBase, float pKa)
            {
                if (conjugateBase.GetCharge() + 1 != acid.GetCharge())
                {
                    throw E("Acids must not violate the conservation of charge: " + acid.GetFullID() + " -> " +
                            conjugateBase.GetFullID());
                }

                var id = acid.GetFullID();
                var dissociationReaction = AddReactant(acid).ID(id + ".dissociation")
                    .AddCatalyst(Molecules.Water, 1).AddProduct(Molecules.Proton)
                    .AddProduct(conjugateBase).ActivationEnergy(2.477721F)
                    .PreexponentialFactor(0.5F * (float)Math.Pow(10.0D, -pKa)).Build();

                var neutralizationReaction = new ReactionBuilder(_results);
                neutralizationReaction.ID(id + ".neutralization").AddReactant(acid)
                    .AddReactant(Molecules.Hydroxide).AddProduct(conjugateBase)
                    .AddProduct(Molecules.Water).ActivationEnergy(2.477721F)
                    .PreexponentialFactor(0.5F * (float)Math.Pow(10.0D, -pKa)).Build();

                var associationReaction = new ReactionBuilder(_results);
                associationReaction.ID(id[1] + ".association").AddReactant(conjugateBase)
                    .AddReactant(Molecules.Proton).AddProduct(acid).ActivationEnergy(2.477721F)
                    .PreexponentialFactor(1.0F).Build();
                return dissociationReaction;
            }

            public ReactionBuilder Reversible()
            {
                return ReverseReaction(_ => { });
            }

            public ReactionBuilder ReverseReaction(
                Action<ReactionBuilder> reverseReactionModifier)
            {
                if (_generated)
                {
                    throw E("Generated Reactions cannot be reversible. Add another Generic Reaction instead.");
                }

                var reverseBuilder = new ReactionBuilder(_results, new ReactingReaction(), false, true);

                foreach (var rateFactor in _reaction._reactants)
                {
                    reverseBuilder.AddProduct(rateFactor.Key, rateFactor.Value);
                }

                foreach (var rateFactor in _reaction._products)
                {
                    reverseBuilder.AddReactant(rateFactor.Key, rateFactor.Value);
                }

                foreach (var rateFactor in _reaction._orders
                             .Where(rateFactor =>
                                 !_reaction._reactants.Keys.Contains(rateFactor.Key)))
                {
                    reverseBuilder.AddCatalyst(rateFactor.Key, rateFactor.Value);
                }

                _reaction._reverseReaction = reverseBuilder._reaction;
                reverseBuilder._reaction._reverseReaction = _reaction;
                reverseBuilder.ID(_reaction._id + ".reverse");
                if (_hasForcedEnthalpyChange)
                {
                    reverseBuilder.EnthalpyChange(-_reaction._enthalpyChange);
                    if (_hasForcedActivationEnergy)
                    {
                        reverseBuilder.ActivationEnergy(_reaction._activationEnergy -
                                                        _reaction._enthalpyChange);
                    }
                }

                if (_hasForcedHalfCellPotential)
                {
                    reverseBuilder.StandardHalfCellPotential(-_reaction._standardHalfCellPotential);
                }

                reverseReactionModifier.Invoke(reverseBuilder);
                if (!Mathf.Approximately(_reaction._enthalpyChange, -reverseBuilder._reaction._enthalpyChange))
                {
                    if (_hasForcedEnthalpyChange)
                    {
                        throw E("The enthalpy change of a reverse reaction must be the negative of the forward");
                    }

                    EnthalpyChange(_reaction._activationEnergy - reverseBuilder._reaction._activationEnergy);
                    reverseBuilder.EnthalpyChange(-_reaction._enthalpyChange);
                }

                if (!Mathf.Approximately(_reaction._activationEnergy - _reaction._enthalpyChange, 
                        reverseBuilder._reaction._activationEnergy))
                {
                    if (!reverseBuilder._hasForcedActivationEnergy)
                    {
                        reverseBuilder.ActivationEnergy(_reaction._activationEnergy -
                                                        _reaction._enthalpyChange);
                    }
                    else
                    {
                        if (_hasForcedActivationEnergy)
                        {
                            throw E(
                                "Activation energies and enthalpy changes for reversible Reactions must obey Hess' Law");
                        }

                        ActivationEnergy(reverseBuilder._reaction._activationEnergy +
                                         _reaction._enthalpyChange);
                    }
                }

                reverseBuilder.Build();
                return this;
            }

            public ReactingReaction Build()
            {
                if (_reaction._id == null && !_generated)
                {
                    throw E("Reaction is missing an ID.");
                }

                var chargeDecrease = _reaction._reactants.Sum(product =>
                    product.Key.GetCharge() * product.Value);

                chargeDecrease = _reaction._products.Aggregate(chargeDecrease,
                    (current, product) =>
                        current - product.Key.GetCharge() * product.Value);

                if (chargeDecrease != 0 && chargeDecrease < 0 != _declaredAsReverse)
                {
                    throw E("Reactions must conserve charge or be reduction half-Reactions.");
                }

                if (chargeDecrease == 0)
                {
                    if (_hasForcedHalfCellPotential)
                    {
                        throw E("A half-cell potential is specified but electrons are not transferred.");
                    }
                }
                else
                {
                    if (!_hasForcedHalfCellPotential)
                    {
                        throw E("Half-Reactions must specify a half-cell potential.");
                    }

                    if (_reaction._reverseReaction == null)
                    {
                        throw E("Half-Reactions must be reversible.");
                    }

                    _reaction._electrons = chargeDecrease;
                }

                if (!_hasForcedActivationEnergy)
                {
                    _reaction._activationEnergy = 25.0F;
                }

                if (!_hasForcedPreExponentialFactor || _reaction._preexponentialFactor <= 0.0F)
                {
                    _reaction._preexponentialFactor = 10000.0F;
                }

                if (!_hasForcedEnthalpyChange)
                {
                    _reaction._enthalpyChange = 0.0F;
                }

                if (!_generated)
                {
                    Debug.Log("Built reaction: " + ReactionString());
                    foreach (var reactant in _reaction._reactants.Keys) {
                        reactant.AddReactantReaction(_reaction);
                    };
                    foreach (var product in _reaction._products.Keys) {
                        product.AddProductReaction(_reaction);
                    };
                    Reactions[_reaction.GetId()] = _reaction;
                }

                _results?.Push(_reaction);

                return _reaction;
            }

            public bool HasReactant(Molecule reactant)
            {
                return _reaction._reactants.ContainsKey(reactant);
            }

            private ReactionConstructionException E(string message)
            {
                var id = _reaction._id ?? ReactionString();
                return new ReactionConstructionException("Problem generating reaction (" + id + "): " + message);
            }

            private string ReactionString()
            {
                var reactionString = _reaction._reactants.Keys.Aggregate("",
                    (current, product) =>
                        current + product.GetSerlializedMolecularFormula(false) + " + ");

                if (_reaction._reactants.Count > 0)
                {
                    reactionString = reactionString[..^3];
                }

                reactionString += " => ";

                reactionString = _reaction._products.Keys.Aggregate(reactionString, 
                    (current, product) => 
                        current + (product.GetSerlializedMolecularFormula(false) + " + "));

                if (_reaction._products.Count > 0)
                {
                    reactionString = reactionString[..^3];
                }

                return reactionString;
            }
        }

        public string GetId()
        {
            return _id;
        }

        public static ReactionBuilder GeneratedReactionBuilder(in IOnlyPushList<IReactingReaction> results)
        {
            return new ReactionBuilder(results , new ReactingReaction(), true, false);
        }
    }
}