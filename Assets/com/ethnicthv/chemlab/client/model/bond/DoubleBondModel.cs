using com.ethnicthv.chemlab.client.api.model;
using com.ethnicthv.chemlab.client.model.util;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.model.bond
{
    public class DoubleBondModel : BondModel
    {
        private static Mesh _doubleBondMesh;

        public DoubleBondModel(Vector3 position, Quaternion rotation, float length) : base(position, rotation, length)
        {
        }

        public DoubleBondModel(float length) : base(Vector3.zero, Quaternion.identity, length)
        {
        }

        public override Mesh GetMesh()
        {
            if (_doubleBondMesh == null)
            {
                _doubleBondMesh = GenerateMesh(Length);
            }

            return _doubleBondMesh;
        }

        private static Mesh GenerateMesh(float length)
        {
            return BondModelUtil.GenerateDoubleBond(0.05f, length);
        }
    }
}