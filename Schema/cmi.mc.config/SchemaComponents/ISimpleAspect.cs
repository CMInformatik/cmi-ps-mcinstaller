using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace cmi.mc.config.SchemaComponents
{
    public interface ISimpleAspect : IAspect
    {
        bool IsRequired { get; set; }
        Type Type { get; }
        AxSupport AxSupport { get; }
        IReadOnlyList<ValidateArgumentsAttribute> ValidationAttributes { get; }

        /// <summary>
        /// Tests if the given value is valid for this aspect.
        /// Throws expection when the value is not valid.
        /// Some aspects can accept null values, some won't.
        /// </summary>
        /// <param name="value">The value to test</param>
        void TestValue(object value);

        /// <summary>
        /// Returns the default value for this aspect.
        /// </summary>
        /// <param name="tenant">The default value for a specific tenant. Can be null.</param>
        /// <returns>The default value.</returns>
        object GetDefaultValue(ITenant tenant = null);

        void AddValidationAttribute(ValidateArgumentsAttribute validator);

    }
}