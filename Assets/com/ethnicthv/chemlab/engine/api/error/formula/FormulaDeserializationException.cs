using System;

namespace com.ethnicthv.chemlab.engine.api.error.formula
{
    public class FormulaDeserializationException : Exception
    {
        public FormulaDeserializationException(string message) : base(message)
        {
        }
    }
}