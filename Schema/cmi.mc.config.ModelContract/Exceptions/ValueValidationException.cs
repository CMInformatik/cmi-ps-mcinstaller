using System;
using cmi.mc.config.ModelContract.Components;

namespace cmi.mc.config.ModelContract.Exceptions
{
    public class ValueValidationException : Exception
    {
        public ValueValidationException(string message, IAspect aspect, object valueTested) : base(message)
        {
            Aspect = aspect;
            ValueTested = valueTested;
        }

        public ValueValidationException(string message, IAspect aspect, object valueTested, Exception inner) : base(
            message, inner)
        {
            Aspect = aspect;
            ValueTested = valueTested;
        }

        public IAspect Aspect { get; }
        public object ValueTested { get; }
    }
}