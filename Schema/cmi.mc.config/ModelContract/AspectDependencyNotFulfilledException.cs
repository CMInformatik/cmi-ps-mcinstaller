using System;

namespace cmi.mc.config.ModelContract
{
    public class AspectDependencyNotFulfilledException : Exception
    {
        public AspectDependencyNotFulfilledException() { }
        public AspectDependencyNotFulfilledException(string message) : base(message) { }
        public AspectDependencyNotFulfilledException(string message, Exception inner) : base(message, inner) { }
    }
}
