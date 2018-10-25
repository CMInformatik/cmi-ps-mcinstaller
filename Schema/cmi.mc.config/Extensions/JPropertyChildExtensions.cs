using System;
using System.Linq;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelContract.Components;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="JProperty"/> class to simplify access to child content of the property.
    /// </summary>
    internal static class JPropertyChildExtensions
    {
        /// <summary>
        /// Determines if the <see cref="JProperty"/> contains a child <see cref="JProperty"/>
        /// with the name of the given <see cref="IAspect"/>.
        /// </summary>
        /// <param name="prop">The property to examine.</param>
        /// <param name="aspect">The aspect from which to get the name of the property.</param>
        /// <returns>True if the <param name="prop" /> contains the child property, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">If a parameter is null.</exception>
        public static bool HasChildProperty(this JProperty prop, IAspect aspect)
        {
            if (aspect == null) throw new ArgumentNullException(nameof(aspect));
            return HasChildProperty(prop, aspect.Name);
        }

        /// <summary>
        /// Determines if the <see cref="JProperty"/> contains a child <see cref="JProperty"/>
        /// with the name of the given <see cref="Enum"/> value.
        /// The <see cref="InConfigurationName"/> will be used to retrieve the <see cref="Enum"/> value.
        /// </summary>
        /// <param name="prop">The property to examine.</param>
        /// <param name="en">The enum from to get the configuration name.</param>
        /// <returns>True if the <param name="prop" /> contains the child property, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">If a parameter is null.</exception>
        public static bool HasChildProperty(this JProperty prop, Enum en)
        {
            if (en == null) throw new ArgumentNullException(nameof(en));
            return HasChildProperty(prop, en.ToConfigurationName());
        }

        /// <summary>
        /// Determines if the <see cref="JProperty"/> contains a child with the given name.
        /// </summary>
        /// <param name="prop">The property to examine.</param>
        /// <param name="name">Name of the property.</param>
        /// <returns>True if the <param name="prop" /> contains the child property, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">If a parameter is null.</exception>
        public static bool HasChildProperty(this JProperty prop, string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return prop.Value.Children<JProperty>().Any(p => p.Name.Equals(name));
        }

        /// <summary>
        /// Returns the child <see cref="JProperty"/> of a given <see cref="JProperty"/>.
        /// </summary>
        /// <param name="prop">The property to examine.</param>
        /// <param name="aspect">The aspect from which to get the name of the child property.</param>
        /// <returns>The <see cref="JProperty"/> or null when no such child is present.</returns>
        /// <exception cref="ArgumentNullException">If a parameter is null.</exception>
        public static JProperty GetChildProperty(this JProperty prop, IAspect aspect)
        {
            if (aspect == null) throw new ArgumentNullException(nameof(aspect));
            return GetChildProperty(prop, aspect.Name);
        }

        /// <summary>
        /// Returns the child <see cref="JProperty"/> of a given <see cref="JProperty"/>.
        /// The <see cref="InConfigurationName"/> will be used to retrieve the <see cref="Enum"/> value.
        /// </summary>
        /// <param name="prop">The property to examine.</param>
        /// <param name="en">The enum from to get the configuration name.</param>
        /// <returns>The <see cref="JProperty"/> or null when no such child is present.</returns>
        /// <exception cref="ArgumentNullException">If a parameter is null.</exception>
        public static JProperty GetChildProperty(this JProperty prop, Enum en)
        {
            if (en == null) throw new ArgumentNullException(nameof(en));
            return GetChildProperty(prop, en.ToConfigurationName());
        }

        /// <summary>
        /// Returns the child <see cref="JProperty"/> of a given <see cref="JProperty"/>.
        /// </summary>
        /// <param name="prop">The property to examine.</param>
        /// <param name="name">Name of the property.</param>
        /// <returns>The <see cref="JProperty"/> or null when no such child is present.</returns>
        /// <exception cref="ArgumentNullException">If a parameter is null.</exception>
        public static JProperty GetChildProperty(this JProperty prop, string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return prop.Value.Children<JProperty>().SingleOrDefault(p => p.Name.Equals(name));
        }
    }
}
