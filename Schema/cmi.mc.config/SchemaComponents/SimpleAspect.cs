using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace cmi.mc.config.SchemaComponents
{
    public class SimpleAspect : Aspect, ISimpleAspect
    {
        private readonly object _defaultValue;
        private readonly List<ValidateArgumentsAttribute> _validationAttributes = new List<ValidateArgumentsAttribute>();
        private bool? _isRequired = null;

        public SimpleAspect(string name, Type type, object defaultValue, AxSupport axSupport = AxSupport.R16_1) : base(name)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (defaultValue != null && !type.IsInstanceOfType(defaultValue))
            {
                throw new ArgumentException(
                    $"{defaultValue.GetType().FullName} is not convertable to type {type.FullName}",
                    nameof(defaultValue));
            }

            _defaultValue = defaultValue;
            Type = type;
            AxSupport = axSupport;
        }

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
        public Type Type { get; }
        public AxSupport AxSupport { get; }
        public IReadOnlyList<ValidateArgumentsAttribute> ValidationAttributes => _validationAttributes;

        public void AddValidationAttribute(ValidateArgumentsAttribute validator)
        {
            if(validator == null) throw new ArgumentNullException(nameof(validator));
            _validationAttributes.Add(validator);
        }

        public object GetDefaultValue(ITenant tenant = null) => _defaultValue;

        /// <inheritdoc />
        public void TestValue(object value, ITenant tenant = null)
        {
            if (value == null && !IsRequired) return;
            if (value == null) throw new ArgumentNullException(nameof(value), "A value for this aspect is required");
            if (!Type.IsInstanceOfType(value)) throw  new ArgumentException($"{value.GetType().FullName} is not convertable to type {Type.FullName}");
            foreach (var validator in _validationAttributes)
            {
                var valMethod = validator.GetType()
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name == "Validate");
               object[] param = {value, null};
               valMethod.Invoke(validator, param); // throws when not fulfilled
            }
        }

        public override IEnumerable<IAspect> Traverse()
        {
            yield return this;
        }
    }
}
