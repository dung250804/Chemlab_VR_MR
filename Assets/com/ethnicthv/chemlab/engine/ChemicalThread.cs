using System;
using com.ethnicthv.chemlab.engine.reaction;
using UnityEngine;

namespace com.ethnicthv.chemlab.engine
{
    public class ChemicalThread : MonoBehaviour
    {
        private bool _running;
        private float _executeInterval = 0.05f;
        private float _interval;

        void Awake()
        {
            ReactionProgram.Instance.RegisterReaction(new HClReaction());
            ReactionProgram.Instance.RegisterReaction(new SulfuricAcidReaction());
            ReactionProgram.Instance.RegisterReaction(new NeutralizationReaction());
            ReactionProgram.Instance.RegisterReaction(new AceticAcidReaction());
        }

        private void Start()
        {
            _executeInterval = 1/20f;
        }

        public void StartTick()
        {
            _running = true;
        }

        public void Stop()
        {
            _running = false;
        }
        
        private void Update()
        {
            if (!_running) return;
            _interval -= Time.deltaTime;
            if (!(_interval <= 0)) return;
            Tick();
            _interval = _executeInterval;
        }
        
        private void Tick()
        {
            try {
                PerformChemicalUpdates();
            } catch (Exception e) {
                Debug.LogError("Error in chemical thread: " + e.Message + "\n" + e.StackTrace);
                throw;
            }
        }

        private static void PerformChemicalUpdates()
        {
            ChemicalTickerHandler.TickAll();
        }
    }
}