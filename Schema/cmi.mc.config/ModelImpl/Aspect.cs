using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using cmi.mc.config.ModelContract;

namespace cmi.mc.config.ModelImpl
{
    internal abstract class Aspect : IAspect
    {
        public IAspect Parent { get; set; }
        public IAspect Root => Parent == null ? this : Parent.Root;
        public string Name { get; }
        protected readonly List<IAspectDependency> DependenciesInteral = new List<IAspectDependency>();
        public IReadOnlyList<IAspectDependency> Dependencies => DependenciesInteral;

        protected Aspect(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if(McSymbols.ReservedWords.Contains(name)) throw new ArgumentException($"{name} is a reserved word and can not be used as name.", nameof(name));
            Name = name;
        }

        public IAspect AddDependency(IAspectDependency dependency)
        {
            if (dependency == null) throw new ArgumentNullException(nameof(dependency));
            DependenciesInteral.Add(dependency);
            return this;
        }

        public IAspect AddDependency(params IAspectDependency[] dependency)
        {
            var exceptions = new List<Exception>();
            if (dependency == null) return this;
            foreach (var d in dependency)
            {
                try
                {
                    AddDependency(d);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }
            return exceptions.Any() ? throw new AggregateException(exceptions) : this;
        }

        /// <inheritdoc/>
        public virtual string GetAspectPath()
        {
            if (this.Parent == null) return this.Name;
            var parentPath = this.Parent.GetAspectPath();
            return !string.IsNullOrWhiteSpace(parentPath) ? $"{parentPath}.{this.Name}" : this.Name;
        }

        public static bool IsValidAspectPath(string aspectPath)
        {
            if (string.IsNullOrWhiteSpace(aspectPath)) return true;
            return Regex.IsMatch(aspectPath, "^[A-Za-z0-9]+(\\.[A-Za-z0-9]+)*$", RegexOptions.Singleline);
        }

        public static void ThrowIfInvalidAspectPath(string aspectPath)
        {   
            if (!IsValidAspectPath(aspectPath)) throw new ArgumentException("Not a valid aspect path", nameof(aspectPath));
        }

        public override string ToString() => GetAspectPath();

        /// <inheritdoc/>
        public abstract IEnumerable<IAspect> Traverse();

        /// <inheritdoc/>
        public virtual IAspect this[string name] => name.Equals(this.Name) ? this : null;
    }
}
