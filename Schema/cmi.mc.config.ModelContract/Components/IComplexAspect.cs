using System.Collections.Generic;

namespace cmi.mc.config.ModelContract
{
    /// <summary>
    ///     A intermediate part of a CMI Axioma mobile clients configuration.
    /// </summary>
    public interface IComplexAspect : IAspect
    {
        /// <summary>
        ///     Default <seealso cref="ConfigControlAttribute" /> for this aspect.
        /// </summary>
        ConfigControlAttribute DefaultCca { get; }

        /// <summary>
        ///     Childs of this aspect.
        /// </summary>
        IReadOnlyDictionary<string, IAspect> Aspects { get; }
    }
}