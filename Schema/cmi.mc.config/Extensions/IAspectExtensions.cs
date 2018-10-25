using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelContract.Components;
using cmi.mc.config.ModelContract.Exceptions;

namespace cmi.mc.config.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IAspect"/> or derivations of this interface.
    /// </summary>
    internal static class IAspectExtensions
    {
        /// <summary>
        /// Executes dependency tests for all dependencies attached to an <see cref="IAspect"/>.
        /// If no exception is thrown, all dependencies are fulfilled.
        /// </summary>
        /// <param name="ensureDependencies">If a dependency is not fulfilled, try to change the configuration to fulfill the dependency.</param>
        /// <exception cref="AspectDependencyNotFulfilledException">When a dependency is not fulfilled.</exception>
        /// <exception cref="AggregateException">When several dependencies are not fulfilled.</exception>
        public static void TestDependencies(this IAspect aspect, ITenant tenant, App app, bool ensureDependencies = false)
        {
            Debug.Assert(aspect != null);
            Debug.Assert(tenant != null);        

            if(!aspect.Dependencies.Any()) return;
            Console.WriteLine($"Test dependencies for {aspect.GetAspectPath()}");

            var exceptions = new List<Exception>();
            foreach (var dep in aspect.Dependencies)
            {
                try
                {
                    if (ensureDependencies)
                    {
                        dep.Ensure(tenant, app);
                    }
                    else
                    {
                        dep.Verify(tenant, app);
                    }
                }
                catch(AspectDependencyNotFulfilledException e)
                {
                    exceptions.Add(e);
                }
            }
            if (!exceptions.Any()) return;
            if (exceptions.Count == 1) throw exceptions.First();
            throw new AggregateException(exceptions);
        }
    }
}
