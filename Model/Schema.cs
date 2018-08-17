using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace CMI.PS {
    public enum App {
        Common,
        Mobileclients,
        Dossierbrowser,
        Sitzungsvorbereitung,
        Zusammenarbeitdritte
    }

    public enum ConfigControlAttribute {
        Extend,
        Replace,
        Remove,
        Internal,
        Private,
        NotSet
    }

    public enum AxSupport {
        R16_1,
        R17,
        R18
    }

    public abstract class Aspect {
        public readonly ConfigControlAttribute DefaultCCA;
        public readonly string Name;
        public Aspect Parent { get; set; }

        public Aspect (string name, ConfigControlAttribute defaultCCA = ConfigControlAttribute.NotSet) {
            if (String.IsNullOrWhiteSpace (name)) {
                throw new ArgumentNullException ("name");
            }
            this.Name = name;
            this.DefaultCCA = defaultCCA;
        }

        public virtual string GetAspectPath () {
            if (this.Parent != null) {
                var parentPath = this.Parent.GetAspectPath ();
                if (!String.IsNullOrWhiteSpace (parentPath)) {
                    return String.Format ("{0}.{1}", Parent.GetAspectPath (), this.Name);
                }
            }
            return this.Name;
        }

        public virtual List<Aspect> GetParents () {
            if (this.Parent != null) {
                return this.Parent.GetParentsInteral ();
            }
            return null;
        }

        protected virtual List<Aspect> GetParentsInteral () {
            List<Aspect> parents = null;
            if (this.Parent != null) {
                parents = this.Parent.GetParents ();
            }
            if (parents == null) {
                parents = new List<Aspect> ();
            }
            parents.Add (this);
            return parents;
        }
    }

    public class ComplexAspect : Aspect {
        public readonly IDictionary<string, Aspect> Aspects =
            new Dictionary<string, Aspect> ();

        public ComplexAspect (string name, ConfigControlAttribute defaultCCA = ConfigControlAttribute.NotSet) : base (name, defaultCCA) { }

        public void AddAspect (Aspect aspect) {
            if (aspect == null) {
                throw new ArgumentNullException ("aspect");
            }
            aspect.Parent = this;
            this.Aspects.Add (aspect.Name, aspect);
        }
    }

    public class AppSection : ComplexAspect {
        public readonly App App;
        public AppSection (App app) : base (app.ToString ()) {
            this.App = app;
        }
        public override string GetAspectPath () {
            return null;
        }
    }

    public class SimpleAspect : Aspect {
        public readonly object DefaultValue;
        public readonly Type Type;
        public readonly AxSupport AxSupport;
        public readonly IList<ValidateArgumentsAttribute> ValidationAttributes = new List<ValidateArgumentsAttribute>();

        public SimpleAspect (
            string name,
            Type type,
            object defaultValue,
            ConfigControlAttribute defaultCCA = ConfigControlAttribute.NotSet,
            AxSupport axSupport = AxSupport.R16_1
        ) : base (name, defaultCCA) {
            if (type == null) {
                throw new ArgumentNullException ("type");
            }
            if (defaultValue != null && !type.IsInstanceOfType (defaultValue)) {
                throw new ArgumentException (String.Format ("{0} is not convertable to type {1}", defaultValue.GetType ().FullName, type.FullName), "defaultValue");
            }
            this.DefaultValue = defaultValue;
            this.Type = type;
            this.AxSupport = axSupport;
        }
    }
}