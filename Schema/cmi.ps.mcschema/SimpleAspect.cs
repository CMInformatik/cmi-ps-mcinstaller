using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace cmi.ps.mcschema
{
    public class SimpleAspect : Aspect
    {
        public readonly object DefaultValue;
        public readonly Type Type;
        public readonly AxSupport AxSupport;
        public readonly IList<ValidateArgumentsAttribute> ValidationAttributes = new List<ValidateArgumentsAttribute>();
        private bool? _isRequired = null;

        public bool IsRequired
        {
            get => _isRequired ?? false;
            set
            {
                if (_isRequired == null)
                {
                    _isRequired = value;
                }
                else
                {
                    throw new InvalidOperationException("You can set this property only once");
                }
            }
        }

        public SimpleAspect(
            string name,
            Type type,
            object defaultValue,
            ConfigControlAttribute defaultCca = ConfigControlAttribute.NotSet,
            AxSupport axSupport = AxSupport.R16_1
        ) : base(name, defaultCca)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (defaultValue != null && !type.IsInstanceOfType(defaultValue))
            {
                throw new ArgumentException(
                    $"{defaultValue.GetType().FullName} is not convertable to type {type.FullName}",
                    nameof(defaultValue));
            }

            DefaultValue = defaultValue;
            Type = type;
            AxSupport = axSupport;
        }

        public override IEnumerable<Aspect> Traverse()
        {
            yield return this;
        }
    }
}
