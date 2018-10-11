using System;
using System.Collections.Generic;

namespace cmi.mc.config.ModelContract
{
    public interface ISimpleAspect : IAspect
    {
        bool IsRequired { get; set; }
        Type Type { get; }
        AxSupport AxSupport { get; }
        bool IsPlatformSpecific { get; set; }

        /// <summary>
        /// Tests if the given value is valid for this aspect.
        /// Throws exception when the value is not valid.
        /// Some aspects can accept null values, some won't.
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <param name="tenant">Allows to test the value for a specific tenant</param>
        /// <param name="platform">Allows to test the value for a specific platform</param>
        void TestValue(object value, ITenant tenant = null, Platform platform = Platform.Unspecified);

        /// <summary>
        /// Returns the default value for this aspect.
        /// </summary>
        /// <param name="tenant">The default value for a specific tenant. Can be null.</param>
        /// <param name="platform">The default for a specific platform.</param>
        /// <returns>The default value.</returns>
        object GetDefaultValue(ITenant tenant = null, Platform platform = Platform.Unspecified);

    }
}