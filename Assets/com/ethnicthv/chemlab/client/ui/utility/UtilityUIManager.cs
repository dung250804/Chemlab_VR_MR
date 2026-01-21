using com.ethnicthv.chemlab.client.api.ui.utility;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.utility
{
    public class UtilityUIManager : MonoBehaviour
    {
        public IPouringPanelController PouringPanelController => pouringPanelController;
        public INamingPanelController NamingPanelController => namingPanelController;
        public IAddMoleculePanelController AddMoleculePanelController => addMoleculePanelController;
        
        [SerializeField] private PouringPanelController pouringPanelController;
        [SerializeField] private NamingPanelController namingPanelController;
        [SerializeField] private AddMoleculePanelController addMoleculePanelController;
    }
}