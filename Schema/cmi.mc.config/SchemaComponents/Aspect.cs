using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using cmi.mc.config.AspectDependencies;

namespace cmi.mc.config.SchemaComponents
{
    public abstract class Aspect : IAspect
    {
        public IAspect Parent { get; set; }
        public IAspect Root => Parent == null ? this : Parent.Root;
        public string Name { get; }
        protected readonly List<IAspectDependency> DependenciesInteral = new List<IAspectDependency>();
        public IReadOnlyList<IAspectDependency> Dependencies => DependenciesInteral;

        protected Aspect(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
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

        public virtual string GetAspectPath()
        {
            if (this.Parent == null) return this.Name;
            var parentPath = this.Parent.GetAspectPath();
            return !string.IsNullOrWhiteSpace(parentPath) ? $"{parentPath}.{this.Name}" : this.Name;
        }

        public static bool IsValidAspectPath(string aspectPath)
        {
            return Regex.IsMatch(aspectPath, "^[A-Za-z0-9]+(\\.[A-Za-z0-9]+)*$", RegexOptions.Singleline);
        }

        public static void ThrowIfInvalidAspectPath(string aspectPath)
        {
            if (String.IsNullOrWhiteSpace(aspectPath)) throw new ArgumentNullException(nameof(aspectPath));
            if (!IsValidAspectPath(aspectPath)) throw new ArgumentException("Not a valid aspect path", nameof(aspectPath));
        }

        public virtual List<IAspect> GetParents()
        {
            List<IAspect> parents = new List<IAspect>();
            IAspect parent = Parent;
            while (parent != null)
            {
                parents.Add(parent);
                parent = parent.Parent;
            }
            return parents;
        }

        public override string ToString() => GetAspectPath();
        public abstract IEnumerable<IAspect> Traverse();
    }
}
