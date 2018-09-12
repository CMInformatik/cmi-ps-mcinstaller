﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace cmi.mc.config.SchemaComponents
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
            AxSupport axSupport = AxSupport.R16_1
        ) : base(name)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
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

        public void TestValue(object value)
        {
            if (value == null && !IsRequired) return;
            if (value == null) throw new ArgumentNullException(nameof(value), "A value for this aspect is required");
            if (!Type.IsInstanceOfType(value)) throw  new ArgumentException($"{value.GetType().FullName} is not convertable to type {Type.FullName}");
            foreach (var validator in ValidationAttributes)
            {
                var valMethod = validator.GetType()
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name == "Validate");
               object[] param = {value, null};
               valMethod.Invoke(validator, param); // throws when not fulfilled
            }
        }

        public override IEnumerable<Aspect> Traverse()
        {
            yield return this;
        }
    }
}