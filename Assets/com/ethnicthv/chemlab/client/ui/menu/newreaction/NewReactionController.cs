using System;
using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.engine.molecule;
using com.ethnicthv.chemlab.engine.reaction;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.ethnicthv.chemlab.client.ui.menu.newreaction
{
    public class NewReactionController : MonoBehaviour
    {
        [Header("Main")] [SerializeField] private TMP_InputField idInput;

        [Header("Properties")] [SerializeField]
        private TMP_InputField preexponentialFactorInputField;

        [SerializeField] private TMP_InputField activationEnergyInputField;
        [SerializeField] private TMP_InputField enthalpyInputField;
        [SerializeField] private TMP_InputField standardHalfCellPotentialInputField;
        [SerializeField] private Toggle isReversibleToggle;

        [Header("View Control")] [SerializeField]
        private GameObject mainView;
        [SerializeField] private GameObject reactantEditorView;
        [SerializeField] private GameObject productEditorView;
        [SerializeField] private GameObject catalystEditorView;
        private int _curViewIndex;
        
        [Header("Main View")]
        [SerializeField]
        private GameObject compoundNamePrefab;

        [SerializeField] private GameObject plusPrefab;
        [SerializeField] private Transform leftReactantContainer;
        [SerializeField] private Transform rightProductContainer;
        [SerializeField] private Transform catalystContainer;
        [SerializeField] private GameObject catalystMainObject;

        [Header("Reactant Editor")] [SerializeField]
        private Transform reactantContainer;

        [SerializeField] private ReactantItemController reactantPrefab;
        [SerializeField] private List<ReactantItemController> reactants;
        [SerializeField] private ToggleGroup reactantToggleGroup;

        [Header("Product Editor")] [SerializeField]
        private Transform productContainer;

        [SerializeField] private ProductItemController productPrefab;
        [SerializeField] private List<ProductItemController> products;
        [SerializeField] private ToggleGroup productToggleGroup;

        [Header("Catalyst Editor")] [SerializeField]
        private Transform catalystItemContainer;

        [SerializeField] private CatalystItemController catalystPrefab;
        [SerializeField] private List<CatalystItemController> catalysts;
        [SerializeField] private ToggleGroup catalystToggleGroup;

        private void Awake()
        {
            OpenView(0);
        }

        private void SetupReactionDisplay()
        {
            var r = reactants.Select(r => r.GetReactant()).ToList();
            var p = products.Select(p => p.GetProduct()).ToList();
            var c = catalysts.Select(c => c.GetCatalyst()).ToList();

            //Note: Clear the containers
            foreach (Transform child in leftReactantContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in rightProductContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in catalystContainer)
            {
                Destroy(child.gameObject);
            }

            //Note: Setup the reactants
            for (var i = 0; i < r.Count; i++)
            {
                var (id, number, _) = r[i];
                var molecule = Molecule.GetMolecule(id);
                var compoundName = Instantiate(compoundNamePrefab, leftReactantContainer);

                compoundName.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    (number != 1 ? number + " " : "") +
                    molecule.GetSerlializedMolecularFormula(true, true);
                compoundName.gameObject.SetActive(true);
                if (i < r.Count - 1)
                {
                    Instantiate(plusPrefab, leftReactantContainer).gameObject.SetActive(true);
                }
            }

            //Note: Setup the products
            for (var i = 0; i < products.Count; i++)
            {
                var (id, number) = p[i];
                var molecule = Molecule.GetMolecule(id);
                var compoundName = Instantiate(compoundNamePrefab, rightProductContainer);

                compoundName.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    (number != 1 ? number + " " : "") +
                    molecule.GetSerlializedMolecularFormula(true, true);
                compoundName.gameObject.SetActive(true);
                if (i < products.Count - 1)
                {
                    Instantiate(plusPrefab, rightProductContainer).gameObject.SetActive(true);
                }
            }

            //Note: Setup the catalyst

            if (c.Count == 0)
            {
                catalystMainObject.SetActive(false);
                return;
            }

            catalystMainObject.SetActive(true);

            foreach (var (id, _) in c)
            {
                var compoundName = Instantiate(compoundNamePrefab, catalystContainer);

                var mo = Molecule.GetMolecule(id);
                
                compoundName.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    mo.GetSerlializedMolecularFormula(true, true);
                compoundName.gameObject.SetActive(true);
            }
        }

        private void OpenView(int viewIndex)
        {
            if (viewIndex == _curViewIndex) return;
            _curViewIndex = viewIndex;
            mainView.SetActive(viewIndex == 0);
            reactantEditorView.SetActive(viewIndex == 1);
            productEditorView.SetActive(viewIndex == 2);
            catalystEditorView.SetActive(viewIndex == 3);
            
            if (viewIndex == 0)
            {
                SetupReactionDisplay();
            }
        }

        public void OpenReactantEditor()
        {
            if (_curViewIndex != 0) return;
            OpenView(1);
        }

        public void AddReactant()
        {
            var reactantItem = Instantiate(reactantPrefab, reactantContainer);
            reactantItem.gameObject.SetActive(true);
            reactants.Add(reactantItem);
        }

        public void RemoveReactant()
        {
            if (!reactantToggleGroup.AnyTogglesOn()) return;
            var selectedToggle = reactantToggleGroup.GetFirstActiveToggle();
            var itemController = selectedToggle.GetComponent<ReactantItemController>();
            Destroy(selectedToggle.gameObject);
            reactants.Remove(itemController);
        }

        public void SubmitReactant()
        {
            if (reactants.Any(r => !r.IsValid()))
            {
                return;
            }

            OpenView(0);
        }

        public void OpenProductEditor()
        {
            if (_curViewIndex != 0) return;
            OpenView(2);
        }

        public void AddProduct()
        {
            var productItem = Instantiate(productPrefab, productContainer);
            productItem.gameObject.SetActive(true);
            products.Add(productItem);
        }

        public void RemoveProduct()
        {
            if (!productToggleGroup.AnyTogglesOn()) return;
            var selectedToggle = productToggleGroup.GetFirstActiveToggle();
            var itemController = selectedToggle.GetComponent<ProductItemController>();
            Destroy(selectedToggle.gameObject);
            products.Remove(itemController);
        }

        public void SubmitProduct()
        {
            if (products.Any(r => !r.IsValid()))
            {
                return;
            }

            OpenView(0);
        }

        public void OpenCatalystEditor()
        {
            if (_curViewIndex != 0) return;
            OpenView(3);
        }

        public void AddCatalyst()
        {
            var catalystItem = Instantiate(catalystPrefab, catalystItemContainer);
            catalystItem.gameObject.SetActive(true);
            catalysts.Add(catalystItem);
        }

        public void RemoveCatalyst()
        {
            if (!catalystToggleGroup.AnyTogglesOn()) return;
            var selectedToggle = catalystToggleGroup.GetFirstActiveToggle();
            var itemController = selectedToggle.GetComponent<CatalystItemController>();
            Destroy(selectedToggle.gameObject);
            catalysts.Remove(itemController);
        }

        public void SubmitCatalyst()
        {
            if (catalysts.Any(r => !r.IsValid()))
            {
                return;
            }

            OpenView(0);
        }

        public void CreateReaction()
        {
            if (_curViewIndex != 0) return;
            if (string.IsNullOrEmpty(idInput.text))
            {
                Debug.Log("Reaction ID is empty");
                return;
            }

            if (reactants.Count == 0)
            {
                Debug.Log("No reactants");
                return;
            }

            if (products.Count == 0)
            {
                Debug.Log("No products");
                return;
            }

            var id = idInput.text;

            var builder = ReactingReaction.CreateBuilder().ID(id);

            foreach (var reactant in reactants)
            {
                var (reactantId, ratio, order) = reactant.GetReactant();
                var molecule = Molecule.GetMolecule(reactantId);
                builder.AddReactant(molecule, ratio, order);
            }

            foreach (var product in products)
            {
                var (productId, ratio) = product.GetProduct();
                var molecule = Molecule.GetMolecule(productId);
                builder.AddProduct(molecule, ratio);
            }

            foreach (var catalyst in catalysts)
            {
                var (catalystId, order) = catalyst.GetCatalyst();
                var molecule = Molecule.GetMolecule(catalystId);
                builder.AddCatalyst(molecule, order);
            }

            if (float.TryParse(preexponentialFactorInputField.text, out var preexponentialFactor))
            {
                builder.PreexponentialFactor(preexponentialFactor);
            }

            if (float.TryParse(activationEnergyInputField.text, out var activationEnergy))
            {
                builder.ActivationEnergy(activationEnergy);
            }

            if (float.TryParse(standardHalfCellPotentialInputField.text, out var standardHalfCellPotential))
            {
                builder.StandardHalfCellPotential(standardHalfCellPotential);
            }

            if (float.TryParse(enthalpyInputField.text, out var enthalpy))
            {
                builder.EnthalpyChange(enthalpy);
            }

            if (isReversibleToggle.isOn) 
            {
                builder.Reversible();
            }
            
            builder.Build();
            
            //Note: reset the view
            idInput.text = "";
            preexponentialFactorInputField.text = "";
            activationEnergyInputField.text = "";
            standardHalfCellPotentialInputField.text = "";
            enthalpyInputField.text = "";
            isReversibleToggle.isOn = false;

            foreach (var reactant in reactants)
            {
                Destroy(reactant.gameObject);
            }

            foreach (var product in products)
            {
                Destroy(product.gameObject);
            }

            foreach (var catalyst in catalysts)
            {
                Destroy(catalyst.gameObject);
            }

            reactants.Clear();
            products.Clear();
            catalysts.Clear();
            
            SetupReactionDisplay();
        }
    }
}