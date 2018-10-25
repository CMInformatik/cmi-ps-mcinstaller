using System;

namespace cmi.mc.config.ModelContract.Exceptions
{
    //ToDo: Add more informations/properties to exception
    //ToDo: Add doc.
    public class AspectDependencyNotFulfilledException : Exception
    {
        public App App { get; }

        public AspectDependencyNotFulfilledException(App app) : this(app, $"Dependency not fulfilled.")
        {
        }

        public AspectDependencyNotFulfilledException(App app, string message) : this(app, message, null)
        {
        }

        public AspectDependencyNotFulfilledException( App app, string message, Exception inner) : base(message, inner)
        {
            App = app;
        }
    }
}