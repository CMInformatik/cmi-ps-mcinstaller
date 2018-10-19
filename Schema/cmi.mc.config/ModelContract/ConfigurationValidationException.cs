using System;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config.ModelContract
{
    public class ConfigurationValidationException : InvalidConfigurationException
    {
        public ConfigurationValidationException(IAspect aspect, JToken token) : base(aspect, token){
        }

        public ConfigurationValidationException(string message, IAspect aspect, JToken token) : base(message, aspect, token)
        {
        }
        public ConfigurationValidationException(string message, IAspect aspect, Exception inner, JToken token) : base(message, aspect, inner, token)
        {
        }
    }
}
