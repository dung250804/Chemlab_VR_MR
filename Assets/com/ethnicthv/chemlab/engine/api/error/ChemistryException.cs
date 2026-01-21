using System;

namespace com.ethnicthv.chemlab.engine.api.error
{
    public abstract class ChemistryException : Exception
    {
        public ChemistryException(string message) : base(message) { }
        
        public ChemistryException(string message, Exception exception) : base(message, exception) {}
    }
}