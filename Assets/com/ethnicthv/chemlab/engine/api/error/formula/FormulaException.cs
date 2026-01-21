using com.ethnicthv.chemlab.engine.formula;

namespace com.ethnicthv.chemlab.engine.api.error.formula
{
    public abstract class FormulaException : ChemistryException
    {
        public readonly Formula Formula;

        public FormulaException(Formula formula, string message) : base("Problem with Formula '" + formula.Serialize() + "': " + message)
        {
            Formula = formula;
        }
    }

}