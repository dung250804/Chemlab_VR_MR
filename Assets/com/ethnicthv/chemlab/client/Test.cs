using com.ethnicthv.chemlab.client.core.renderer.render;
using com.ethnicthv.chemlab.client.model;
using com.ethnicthv.chemlab.client.model.bond;
using com.ethnicthv.chemlab.engine.api.atom;
using com.ethnicthv.chemlab.engine.api.element;
using UnityEngine;

namespace com.ethnicthv.chemlab.client
{
    [ExecuteAlways]
    public class Test : MonoBehaviour
    {
        private SingleBondModel _singleBondModel;
        private GenericAtomModel _atomModel;
        
        private BondRenderer _bondRenderer;
        private GenericAtomRenderer _atomRenderer;
    }
}