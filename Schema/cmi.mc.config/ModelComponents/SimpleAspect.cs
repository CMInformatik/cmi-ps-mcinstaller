using System;
using System.Collections.Generic;
using System.Linq;
using cmi.mc.config.ModelContract;
using FluentValidation;
using PlatformNotSupportedException = cmi.mc.config.ModelContract.PlatformNotSupportedException;

namespace cmi.mc.config.ModelComponents
{
    public class SimpleAspect<T> : Aspect, ISimpleAspect
    {
        private readonly IDictionary<Platform, T> _defaultValue = new Dictionary<Platform, T>();
        private readonly IValidator<T> _validator;
        private bool? _isRequired = null;
        private bool? _isPlatformSpecific = null;

        public SimpleAspect(string name, T defaultValue, AxSupport axSupport = AxSupport.R16_1, IValidator<T> validator = null) : base(name)
        {
            _defaultValue.Add(Platform.Unspecified, defaultValue);
            AxSupport = axSupport;
            _validator = validator;
            TestValue(defaultValue);
        }

        public Type Type => typeof(T);

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

        public object GetDefaultValue(ITenant tenant = null, Platform platform = Platform.Unspecified)
        {
            return _defaultValue.ContainsKey(platform)? _defaultValue[platform] : _defaultValue[Platform.Unspecified];
        }

        /// <inheritdoc />
        public void TestValue(object value, ITenant tenant = null, Platform platform = Platform.Unspecified)
        {
            if (!IsPlatformSpecific && platform != Platform.Unspecified)
            {
                throw new PlatformNotSupportedException($"{GetAspectPath()} does not support platform specific values", platform);
            }

            if (value == null && IsRequired) throw new ArgumentNullException(nameof(value), "A value for this aspect is required");
            if (value != null && !(value is T)) throw  new ArgumentException($"{value.GetType().FullName} is not convertable to type {typeof(T).FullName}");

            var summary = _validator?.Validate(value);
            if (summary == null || summary.IsValid) return;

            var errors = summary.Errors.Select(error => new ValueValidationException(error)).ToList();
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
