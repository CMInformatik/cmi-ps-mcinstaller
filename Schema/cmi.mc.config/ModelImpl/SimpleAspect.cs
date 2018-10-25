using System;
using System.Collections.Generic;
using System.Linq;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelContract.Components;
using cmi.mc.config.ModelContract.Exceptions;
using FluentValidation;

namespace cmi.mc.config.ModelImpl
{
    internal class SimpleAspect<T> : Aspect, ISimpleAspect
    {
        private class ValidationContext
        {
            public object Value { get; }
            public Platform Platform { get; }
            public ITenant Tenant { get; }

            public ValidationContext(object value, Platform platform, ITenant tenant)
            {
                Value = value;
                Platform = platform;
                Tenant = tenant;
            }
        }

        private class InternalValidator : AbstractValidator<ValidationContext>
        {
            public InternalValidator(ISimpleAspect aspect)
            {
                if(aspect == null) throw new ArgumentNullException(nameof(aspect));

                RuleFor(c => c.Value).Must(o => o == null || (o is T))
                    .WithMessage(v => $"{v?.GetType().FullName} is not convertable to type {typeof(T).FullName}");
                RuleFor(c => c.Value).NotNull()
                    .WithMessage($"The aspect '{aspect.GetAspectPath()}' does not accept null values");
                RuleFor(c => c.Platform).Must(p => aspect.IsPlatformSpecific || p == Platform.Unspecified)
                    .WithMessage($"'{aspect.GetAspectPath()}' does not support platform specific values");
            }
        }

        private readonly IDictionary<Platform, T> _defaultValue = new Dictionary<Platform, T>();
        private readonly IValidator<T> _validator;
        private readonly InternalValidator _internalValidator;
        private bool? _isRequired = null;
        private bool? _isPlatformSpecific = null;

        public SimpleAspect(string name, T defaultValue, AxSupport axSupport = AxSupport.R16_1, IValidator<T> validator = null) : base(name)
        {
            _defaultValue.Add(Platform.Unspecified, defaultValue);
            AxSupport = axSupport;
            _validator = validator;
            _internalValidator = new InternalValidator(this);
            TestValue(defaultValue);
        }

        /// <inheritdoc />
        public Type Type => typeof(T);

        /// <inheritdoc />
        public bool IsPlatformSpecific
        {
            get => _isPlatformSpecific ?? false;
            set
            {
                if (value)
                {
                    _isPlatformSpecific = true;
                }
                else
                {
                    throw new InvalidOperationException("You can not set this property to false");
                }
            }
        }

        /// <inheritdoc />
        public bool IsRequired
        {
            get => _isRequired ?? false;
            set
            {
                if (value)
                {
                    _isRequired = true;
                }
                else
                {
                    throw new InvalidOperationException("You can not set this property to false");
                }
            }
        }

        /// <inheritdoc />
        public AxSupport AxSupport { get; }

        public void SetDefaultValue(Platform platform, T value)
        {
            if (_defaultValue.ContainsKey(platform))
            {
                throw new InvalidOperationException($"The default value for {platform} is already set.");
            }      
            IsPlatformSpecific = true;
            TestValue(value, null, platform);
            _defaultValue.Add(platform, value);
        }

        /// <inheritdoc />
        public object GetDefaultValue(ITenant tenant = null, Platform platform = Platform.Unspecified)
        {
            return _defaultValue.ContainsKey(platform)? _defaultValue[platform] : _defaultValue[Platform.Unspecified];
        }

        /// <inheritdoc />
        public void TestValue(object value, ITenant tenant = null, Platform platform = Platform.Unspecified)
        {
            Validate(_internalValidator, new ValidationContext(value, platform, tenant));
            Validate(_validator, value);
        }

        private void Validate(IValidator validator, object value)
        {
            var summary = validator?.Validate(value);
            if (summary == null || summary.IsValid) return;

            var errors = summary.Errors.Select(error => new ValueValidationException(error.ErrorMessage, this, value)).ToList();
            if (!errors.Any()) return;
            if (errors.Count == 1)
            {
                throw errors.First();
            }
            throw new AggregateException(errors);
        }

        public override IEnumerable<IAspect> Traverse()
        {
            yield return this;
        }
    }
}
