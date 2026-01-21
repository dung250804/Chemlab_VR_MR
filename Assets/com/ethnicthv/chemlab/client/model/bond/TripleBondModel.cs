using com.ethnicthv.chemlab.client.api.model;
using com.ethnicthv.chemlab.client.model.util;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.model.bond
{
    public class TripleBondModel : BondModel
    {
        private static Mesh _mesh;
        
        public TripleBondModel(Vector3 position, Quaternion rotation, float length) : base(position, rotation, length)
        {
        }
        
        public TripleBondModel(float length) : base(Vector3.zero, Quaternion.identity, length)
        {
        }
        
        public override Mesh GetMesh()
        {
            if (_mesh == null)
            {
                _mesh = GenerateMesh(Length);
            }
            return _mesh;
        }
        
        private static Mesh GenerateMesh(float length)
        {
            return BondModelUtil.GenerateTripleBond(0.05f, length);
        }
    }
}