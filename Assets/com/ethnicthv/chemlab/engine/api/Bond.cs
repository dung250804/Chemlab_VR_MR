using System;
using com.ethnicthv.chemlab.engine.api.atom;

namespace com.ethnicthv.chemlab.engine.api
{
    public class Bond
    {
        private readonly Atom _srcAtom;
        private readonly Atom _dstAtom;
        private BondType _bondType;
        
        public Bond(Atom srcAtom, Atom dstAtom, BondType bondType)
        {
            _srcAtom = srcAtom;
            _dstAtom = dstAtom;
            _bondType = bondType;
        }
        
        public Atom GetSourceAtom()
        {
            return _srcAtom;
        }
        
        public Atom GetDestinationAtom()
        {
            return _dstAtom;
        }
        
        public BondType GetBondType()
        {
            return _bondType;
        }
        
        public void SetBondType(BondType bondType)
        {
            _bondType = bondType;
        }

        public override bool Equals(object obj)
        {
            if (obj is Bond bond)
            {
                return bond._srcAtom == _srcAtom && bond._dstAtom == _dstAtom && bond._bondType == _bondType;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_srcAtom, _dstAtom);
        }

        public enum BondType
        {
            Single = 1,
            Double = 2,
            Triple = 3,
            Aromatic = 4,
        }
    }
}