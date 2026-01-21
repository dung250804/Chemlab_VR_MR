using com.ethnicthv.chemlab.engine.molecule;
using TMPro;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.menu.newreaction
{
    public class ReactantItemController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField reactantIdInput;
        [SerializeField] private TMP_InputField ratioInput;
        [SerializeField] private TMP_InputField orderInput;

        private void Awake()
        {
            reactantIdInput.onSubmit.AddListener(CheckIdValid);
            ratioInput.onSubmit.AddListener(CheckNumberValid);
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
        
        private void CheckNumberValid(string ratio)
        {
            if (string.IsNullOrEmpty(ratio)) return;
            if (!int.TryParse(ratio, out _))
            {
                ratioInput.text = "1";
            }
        }
        
        public (string id, int ratio, int order) GetReactant()
        {
            var id = reactantIdInput.text;
            var ratio = int.Parse(ratioInput.text);
            var order = int.Parse(orderInput.text);
            return (id, ratio, order);
        }
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(reactantIdInput.text) && Molecule.IsMolecule(reactantIdInput.text) &&
                   !string.IsNullOrEmpty(ratioInput.text) && 
                   !string.IsNullOrEmpty(orderInput.text);
        }
    }
}