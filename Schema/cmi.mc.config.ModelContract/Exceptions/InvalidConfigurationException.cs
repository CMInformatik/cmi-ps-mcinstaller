using System;

namespace cmi.mc.config.ModelContract
{
    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(IAspect aspect, string jPath) : base($"{aspect?.GetAspectPath()}: {jPath}")
        {
            JPath = jPath;
            Aspect = aspect;
        }

        public InvalidConfigurationException(string message, IAspect aspect, string jPath) : base(message)
        {
            JPath = jPath;
            Aspect = aspect;
        }

        public InvalidConfigurationException(string message, IAspect aspect, string jPath, Exception inner) : base(
            message, inner)
        {
            JPath = jPath;
            Aspect = aspect;
        }

        public string JPath { get; } // ToDo: Remove Newtonsoft dependency
        public IAspect Aspect { get; }
    }
}