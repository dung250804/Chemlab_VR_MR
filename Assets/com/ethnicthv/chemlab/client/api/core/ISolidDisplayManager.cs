using System;
using com.ethnicthv.chemlab.client.core.game;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.api.core
{
    public interface ISolidDisplayManager
    {
        public SolidDisplay GetSolidDisplay(string key);
    }
}