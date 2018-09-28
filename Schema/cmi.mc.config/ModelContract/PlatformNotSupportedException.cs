using System;

namespace cmi.mc.config.ModelContract
{
    public class PlatformNotSupportedException : Exception
    {
        public Platform Platform { get; }

        public PlatformNotSupportedException(Platform platform) : base($"The platform {platform.ToConfigurationName()} is not supported in the current context")
        {
            Platform = platform;
        }
        public PlatformNotSupportedException(string message, Platform platform) : base(message)
        {
            Platform = platform;
        }
        public PlatformNotSupportedException(string message, Exception inner, Platform platform) : base(message, inner)
        {
            Platform = platform;
        }
    }
}
