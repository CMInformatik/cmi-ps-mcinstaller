using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cmi.mc.config.ModelContract;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config.ModelContract
{
    public class InvalidConfigurationException : Exception
    {
        public JToken Token { get; } // ToDo: Remove Newtonsoft dependency
        public IAspect Aspect { get; }

        public InvalidConfigurationException(IAspect aspect, JToken token) : base($"{aspect?.GetAspectPath()}: {token}")
        {
            Token = token;
            Aspect = aspect;
        }

        public InvalidConfigurationException(string message, IAspect aspect, JToken token) : base(message)
        {
            Token = token;
            Aspect = aspect;
        }
        public InvalidConfigurationException(string message, IAspect aspect, Exception inner, JToken token) : base(message, inner)
        {
            Token = token;
            Aspect = aspect;
        }
    }
}
