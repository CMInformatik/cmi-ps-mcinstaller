using System;

namespace cmi.mc.config.AspectDependencies
{
    public class AspectDependencyNotFulfilled : Exception
    {
        public AspectDependencyNotFulfilled() { }
        public AspectDependencyNotFulfilled(string message) : base(message) { }
        public AspectDependencyNotFulfilled(string message, Exception inner) : base(message, inner) { }
    }
}
