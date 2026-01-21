using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.client.game;
using com.ethnicthv.chemlab.engine.mixture;
using com.ethnicthv.chemlab.engine.molecule;
using TMPro;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.utility
{
    public class AddMoleculeItemController : MonoBehaviour
    {
        private Molecule _molecule;
        private IMixtureContainer _mixtureContainer;
        private ISolidContainer _solidContainer;
        
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TMP_InputField volumeInput;
        
        public void Set(Molecule molecule, string bottleName, IMixtureContainer mixtureContainer, ISolidContainer solidContainer)
        {
            _molecule = molecule;
            _mixtureContainer = mixtureContainer;
            _solidContainer = solidContainer;
            nameText.text = bottleName;
        }

        public void OnClick()
        {
            if (!_molecule.IsSolid())
            {
                if (!_mixtureContainer.IsEmpty()) return;
                var mixture = Mixture.Pure(_molecule);
                _mixtureContainer.SetMixture(mixture);
                _mixtureContainer.SetVolume(float.Parse(volumeInput.text));
            }
            else
            {
                if (!_solidContainer.IsSolidEmpty()) return;
                _solidContainer.AddSolidMolecule(_molecule, float.Parse(volumeInput.text));
            }
        }
    }
}