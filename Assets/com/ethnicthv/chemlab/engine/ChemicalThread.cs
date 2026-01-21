using System;
using UnityEngine;

namespace com.ethnicthv.chemlab.engine
{
    public class ChemicalThread : MonoBehaviour
    {
        private bool _running;
        private float _executeInterval = 0.05f;
        private float _interval;

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