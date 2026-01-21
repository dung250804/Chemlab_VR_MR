using com.ethnicthv.chemlab.engine.formula;

namespace com.ethnicthv.chemlab.engine.api.error.formula
{
    public class FormulaModificationException : FormulaException
    {
        public FormulaModificationException(Formula formula, string message) : base(formula ,message) { }
    }
}