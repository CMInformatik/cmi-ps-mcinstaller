using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace cmi.mc.config.ModelContract
{
    public class ValueValidationException : Exception
    {
        public ValidationFailure Error { get; } // ToDo: Remove FulentValidation Dependency
        public IAspect Aspect { get; }

        public ValueValidationException(IAspect aspect, ValidationFailure error): base($"{aspect?.GetAspectPath()}: {error?.ErrorMessage}")
        {
            Error = error;
            Aspect = aspect;
        }

        public ValueValidationException(string message, IAspect aspect, ValidationFailure error) : base(message)
        {
            Error = error;
            Aspect = aspect;
        }
        public ValueValidationException(string message, IAspect aspect, Exception inner, ValidationFailure error) : base(message, inner)
        {
            Error = error;
            Aspect = aspect;
        }
    }
}
