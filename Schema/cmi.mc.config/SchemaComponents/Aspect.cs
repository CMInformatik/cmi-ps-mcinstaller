using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace cmi.mc.config.SchemaComponents
{
    public abstract class Aspect : Element
    {
        public Aspect Parent { get; set; }
        public Aspect Root => Parent == null ? this : Parent.Root;

        protected Aspect(string name) : base(name) { }

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

        protected virtual List<Aspect> GetParentsInternal()
        {
            List<Aspect> parents = null;
            if (this.Parent != null)
            {
                parents = this.Parent.GetParents();
            }
            if (parents == null)
            {
                parents = new List<Aspect>();
            }
            parents.Add(this);
            return parents;
        }

        public virtual List<Aspect> GetParents() => Parent?.GetParentsInternal();
        public override string ToString() => GetAspectPath();
        public abstract IEnumerable<Aspect> Traverse();
    }
}
