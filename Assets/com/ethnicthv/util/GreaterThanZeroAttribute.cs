using System;
using System.Reflection;

namespace com.ethnicthv.util
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class GreaterThanZeroAttribute : Attribute
    {
        public string ErrorMessage { get; }

        public GreaterThanZeroAttribute(string errorMessage = "Value must be greater than zero.")
        {
            ErrorMessage = errorMessage;
        }
    }
    
    
}