using cmi.mc.config.ModelContract.Exceptions;

namespace cmi.mc.config.ModelContract.Components
{
    /// <summary>
    ///     Dependency of an aspect to other parts of the configuration.
    /// </summary>
    public interface IAspectDependency
    {
        /// <summary>
        ///     Verifies that the dependency is fulfilled.
        ///     Throws exception if the dependency is not fulfilled.
        /// </summary>
        /// <param name="tenant">Verifies the dependency tenant specific.</param>
        /// <param name="app">Verifies the dependency app specific.</param>
        /// <exception cref="AspectDependencyNotFulfilledException">When the dependency is not fulfilled.</exception>
        void Verify(ITenant tenant, App app);

        /// <summary>
        ///     Ensures that the dependency is fulfilled.
        ///     This will change foreign configuration parts, which may lead to other side effects.
        /// </summary>
        /// <param name="tenant">Verifies the dependency tenant specific.</param>
        /// <param name="app">Verifies the dependency app specific.</param>
        void Ensure(ITenant tenant, App app);
    }
}