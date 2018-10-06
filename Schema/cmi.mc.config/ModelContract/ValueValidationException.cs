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
        public ValidationFailure Error { get; }

        public ValueValidationException(ValidationFailure error): base($"{error?.PropertyName}: {error?.ErrorMessage}")
        {
            Error = error;
        }

        public ValueValidationException(string message, ValidationFailure error) : base(message)
        {
            Error = error;
        }
        public ValueValidationException(string message, Exception inner, ValidationFailure error) : base(message, inner)
        {
            Error = error;
        }
    }
}
