using System;

namespace cmi.mc.config.ModelContract
{
    //ToDo: Add more informations/properties to exception
    //ToDo: Add doc.
    public class AspectDependencyNotFulfilledException : Exception
    {
        public AspectDependencyNotFulfilledException()
        {
        }

        public AspectDependencyNotFulfilledException(string message) : base(message)
        {
        }

        public AspectDependencyNotFulfilledException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}