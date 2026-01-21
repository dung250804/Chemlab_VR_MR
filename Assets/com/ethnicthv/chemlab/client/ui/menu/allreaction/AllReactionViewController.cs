using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.engine.api.reaction;
using com.ethnicthv.chemlab.engine.molecule;
using TMPro;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.menu.allreaction
{
    public class AllReactionViewController : MonoBehaviour
    {
        private IReactingReaction _reaction;

        [SerializeField] private TextMeshProUGUI nameText;

        [Header("Reaction Display")] [SerializeField]
        private GameObject compoundNamePrefab;

        [SerializeField] private GameObject plusPrefab;
        [SerializeField] private Transform leftReactantContainer;
        [SerializeField] private Transform rightProductContainer;
        [SerializeField] private Transform catalystContainer;
        [SerializeField] private GameObject catalystMainObject;

        public void SetupView(IReactingReaction reaction)
        {
            _reaction = reaction;

            nameText.text = GetReactionName(reaction);

            SetupReactionDisplay();
        }

        private static string GetReactionName(IReactingReaction reaction)
        {
            return reaction.GetId()
                .Replace('_', ' ')
                .Replace('.', ' ')
                .Split(' ')
                .Aggregate("", 
                    (current, s) => current + s[0].ToString().ToUpper() + s[1..] + ' ');
        }

        private void SetupReactionDisplay()
        {
            var reactants = _reaction.GetReactants();
            var products = _reaction.GetProducts();
            var catalyst = new List<Molecule>();

            foreach (var (molecule, _) in _reaction.GetOrders())
            {
                if (reactants.Contains(molecule))
                    continue;
                if (products.Contains(molecule))
                    continue;
                catalyst.Add(molecule);
            }

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
            for (var i = 0; i < reactants.Count; i++)
            {
                var molecule = reactants[i];
                var compoundName = Instantiate(compoundNamePrefab, leftReactantContainer);

                var number = _reaction.GetReactantMolarRatio(molecule);

                compoundName.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    (number != 1 ? number + " " : "") +
                    molecule.GetSerlializedMolecularFormula(true, true);
                compoundName.gameObject.SetActive(true);
                if (i < reactants.Count - 1)
                {
                    Instantiate(plusPrefab, leftReactantContainer).gameObject.SetActive(true);
                }
            }

            //Note: Setup the products
            for (var i = 0; i < products.Count; i++)
            {
                var molecule = products[i];
                var compoundName = Instantiate(compoundNamePrefab, rightProductContainer);

                var number = _reaction.GetProductMolarRatio(molecule);

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

            if (catalyst.Count == 0)
            {
                catalystMainObject.SetActive(false);
                return;
            }

            catalystMainObject.SetActive(true);

            foreach (var mo in catalyst)
            {
                var compoundName = Instantiate(compoundNamePrefab, catalystContainer);

                compoundName.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    mo.GetSerlializedMolecularFormula(true, true);
                compoundName.gameObject.SetActive(true);
            }
        }
    }
}