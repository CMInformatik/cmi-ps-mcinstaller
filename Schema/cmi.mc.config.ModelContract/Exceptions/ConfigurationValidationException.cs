using System;
using cmi.mc.config.ModelContract.Components;

namespace cmi.mc.config.ModelContract.Exceptions
{
    public class ConfigurationValidationException : InvalidConfigurationException
    {
        public ConfigurationValidationException(IAspect aspect, string jPath) : base(aspect, jPath)
        {
        }

        public ConfigurationValidationException(string message, IAspect aspect, string jPath) : base(message, aspect,
            jPath)
        {
        }

        public ConfigurationValidationException(string message, IAspect aspect, string jPath, Exception inner) : base(
            message, aspect, jPath, inner)
        {
        }
    }
}