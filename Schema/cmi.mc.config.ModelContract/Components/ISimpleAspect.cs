using System;

namespace cmi.mc.config.ModelContract
{
    /// <summary>
    ///     A leaf aspect of a CMI Axioma mobile client configuration.
    /// </summary>
    public interface ISimpleAspect : IAspect
    {
        /// <summary>
        ///     Specifies whether the aspect is required in any specification.
        /// </summary>
        bool IsRequired { get; set; } //ToDo: Remove setter

        /// <summary>
        ///     Value type of this aspect.
        /// </summary>
        Type Type { get; }

        /// <summary>
        ///     Minimal CMI Axioma version requirements for this aspect.
        /// </summary>
        AxSupport AxSupport { get; }

        /// <summary>
        ///     Specifies whether the aspect is <seealso cref="Platform" />-specific.
        /// </summary>
        bool IsPlatformSpecific { get; set; } //ToDo: Remove setter

        /// <summary>
        ///     Tests if the given value is valid for this aspect.
        ///     Throws a exception when the value is not valid.
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <param name="tenant">Allows to test the value for a specific tenant</param>
        /// <param name="platform">Allows to test the value for a specific platform</param>
        /// <exception cref="AggregateException">When several validation errors occures</exception>
        /// <exception cref="ValueValidationException">When the provided value does not pass a validation test</exception>
        void TestValue(object value, ITenant tenant = null, Platform platform = Platform.Unspecified);

        /// <summary>
        ///     Returns the default value for this aspect.
        /// </summary>
        /// <param name="tenant">The default value for a specific tenant. Can be null.</param>
        /// <param name="platform">The default for a specific platform.</param>
        /// <returns>The default value.</returns>
        object GetDefaultValue(ITenant tenant = null, Platform platform = Platform.Unspecified);
    }
}