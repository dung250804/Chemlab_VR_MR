using com.ethnicthv.chemlab.engine.molecule;
using TMPro;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.menu.newreaction
{
    public class CatalystItemController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField reactantIdInput;
        [SerializeField] private TMP_InputField orderInput;

        private void Awake()
        {
            reactantIdInput.onSubmit.AddListener(CheckIdValid);
            orderInput.onSubmit.AddListener(CheckNumberValid);
        }

        private void CheckIdValid(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (Molecule.IsMolecule(id))
            {
                reactantIdInput.text = "";
            }
        }
        
        private void CheckNumberValid(string order)
        {
            if (string.IsNullOrEmpty(order)) return;
            if (!int.TryParse(order, out _))
            {
                orderInput.text = "1";
            }
        }
        
        public (string id, int order) GetCatalyst()
        {
            var id = reactantIdInput.text;
            var order = int.Parse(orderInput.text);
            return (id, order);
        }
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(reactantIdInput.text) && Molecule.IsMolecule(reactantIdInput.text) &&
                   !string.IsNullOrEmpty(orderInput.text);
        }
    }
}