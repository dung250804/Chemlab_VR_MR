using com.ethnicthv.chemlab.client.api.model;
using com.ethnicthv.chemlab.client.model.util;
using com.ethnicthv.chemlab.engine.api.atom;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.model
{
    public class GenericAtomModel : IAtomModel
    {
        private const float AtomRadius = 0.5f;

        private static Mesh DefaultMesh;
        
        public Vector3 Position;
        public Quaternion Rotation;
        private readonly Atom _atom;
        private readonly Vector3 _size;
        
        public GenericAtomModel ParentAtom { get; set; }
        public int RingPosition { get; set; } = -1;
        
        public GenericAtomModel(Vector3 position, Quaternion rotation, Atom atom, float radius)
        {
            Position = position;
            Rotation = rotation;
            _atom = atom;
            _size = Vector3.one * radius;
        }

        public GenericAtomModel(Atom atom)
        {
            // calculate radius based on element
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
            _atom = atom;
            _size = ElementAtomRadius.Radius.TryGetValue(atom.GetElement(), out var radius) ? Vector3.one *  radius : Vector3.one * AtomRadius * 2;
        }

        public Vector3 GetPosition()
        {
            return Position;
        }

        public Quaternion GetRotation()
        {
            return Rotation;
        }

        public Matrix4x4 GetModelMatrix()
        {
            return Matrix4x4.TRS(Position, Rotation, _size);
        }

        public Atom GetAtom()
        {
            return _atom;
        }
    }
}