using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace com.ethnicthv.chemlab.client.core.renderer
{
    [Serializable]
    public class ChemicalCompoundVolume : VolumeComponent
    {
        [Space(10)]
        public ClampedFloatParameter bondRadius = new(0.1f, 0.001f, 0.5f);
    }
}