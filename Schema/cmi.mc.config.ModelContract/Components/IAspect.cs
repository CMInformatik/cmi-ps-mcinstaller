using System.Collections.Generic;

namespace cmi.mc.config.ModelContract
{
    /// <summary>
    ///     An aspect of a CMI Axioma mobile client configuration.
    /// </summary>
    public interface IAspect
    {
        /// <summary>
        ///     Name of the aspect in the configuration file.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Dependencies to other parts of the configuration.
        /// </summary>
        IReadOnlyList<IAspectDependency> Dependencies { get; }

        /// <summary>
        ///     Parent aspect.
        /// </summary>
        IAspect Parent { get; set; }

        /// <summary>
        ///     The root aspect of area, which this aspect is part of.
        /// </summary>
        IAspect Root { get; }

        IAspect this[string name] { get; }

        /// <summary>
        ///     Path to this aspect in the configuration area.
        /// </summary>
        /// <returns>Path in JPath-style</returns>
        string GetAspectPath();

        /// <summary>
        ///     Traverses any child aspect inclusive this aspect.
        /// </summary>
        IEnumerable<IAspect> Traverse();
    }
}