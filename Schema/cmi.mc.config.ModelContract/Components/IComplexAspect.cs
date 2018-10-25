using System.Collections.Generic;

namespace cmi.mc.config.ModelContract.Components
{
    /// <summary>
    ///     A intermediate part of a mobile clients configuration.
    ///     This should be a non-leaf node in a tree structure.
    /// </summary>
    public interface IComplexAspect : IAspect
    {
        /// <summary>
        ///     Default <seealso cref="ConfigControlAttribute" /> for this aspect.
        /// </summary>
        ConfigControlAttribute DefaultCca { get; }

        /// <summary>
        ///     Childs aspects of this aspect.
        /// </summary>
        IReadOnlyDictionary<string, IAspect> Aspects { get; }
    }
}