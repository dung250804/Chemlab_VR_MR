using com.ethnicthv.chemlab.client.core.game;
using com.ethnicthv.chemlab.client.ui.menu.allcompound;
using com.ethnicthv.chemlab.engine.api.reaction;
using com.ethnicthv.chemlab.engine.molecule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.ethnicthv.chemlab.client.ui.menu.allreaction
{
    [RequireComponent(typeof(Toggle))]
    public class AllReactionItemController : MonoBehaviour
    {
        private IReactingReaction _reaction;
        private Toggle _button;
        
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private AllReactionViewController allReactionViewController;
        
        private void Awake()
        {
            _button = GetComponent<Toggle>();
        }

        private void OnEnable()
        {
            _button.onValueChanged.AddListener(OnClick);
            if (_button.isOn && _reaction != null)
                allReactionViewController.SetupView(_reaction);
        }
        
        private void OnDisable()
        {
            _button.onValueChanged.RemoveListener(OnClick);
        }

        public void SetReaction(IReactingReaction reaction)
        {
            _reaction = reaction;
            
            nameText.text = _reaction.GetId();
        }

        private void OnClick(bool value)
        {
            if (!value)
                return;
            
            allReactionViewController.SetupView(_reaction);
        }
    }
}