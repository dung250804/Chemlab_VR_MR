using com.ethnicthv.chemlab.engine.molecule.group;
using com.ethnicthv.chemlab.engine.molecule.group.detector;
using com.ethnicthv.chemlab.engine.reaction;
using UnityEngine;

namespace com.ethnicthv.chemlab.engine
{
    public class ChemLabEngine : MonoBehaviour
    {
        public static ChemLabEngine Instance { get; private set; }
        
        [SerializeField] private ChemicalThread chemicalThread;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            
            Setup();
        }
        
        private void Start()
        {
            
            chemicalThread.StartTick();
        }

        private void OnDestroy()
        {
            chemicalThread.Stop();
        }

        private void Setup()
        {
            var temp = StaticReactions.SodiumDissolution;
            // Register detectors
            GroupDetectingProgram.Instance.RegisterDetector(new AlcoholGroupDetector());
            GroupDetectingProgram.Instance.RegisterDetector(new OrganicAcidDetector());
            
            //
        }
    }
}
