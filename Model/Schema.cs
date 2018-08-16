using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CMI.PS
{
    public enum App
    {
        Common,
        Mobileclients,
        Dossierbrowser,
        Sitzungsvorbereitung,
        Zusammenarbeitdritte
    };

    public enum ConfigurationControlAttribute
    {
        Extend,
        Replace,
        Remove,
        Internal,
        Private,
        NotSet
    };

    public abstract class Aspect
    {
        public readonly ConfigurationControlAttribute DefaultCCA;
        public readonly string Name;
        public Aspect Parent {get; set;}

        public Aspect(string name, ConfigurationControlAttribute defaultCCA = ConfigurationControlAttribute.NotSet)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }
            this.Name = name;
            this.DefaultCCA = defaultCCA;
        }

        public virtual string GetAspectPath (){
            if(this.Parent != null){
                var parentPath = this.Parent.GetAspectPath();
                if(!String.IsNullOrWhiteSpace(parentPath)){
                    return String.Format("{0}.{1}", Parent.GetAspectPath(), this.Name);
                }
            }
            return this.Name;
        }
    }

    public class ComplexAspect : Aspect
    {
        public readonly Dictionary<string, Aspect> Aspects =
            new Dictionary<string, Aspect>();

        public ComplexAspect(string name) : base(name)
        {
        }

        public void AddAspect(Aspect aspect)
        {
            if (aspect == null)
            {
                throw new ArgumentNullException("aspect");
            }
            aspect.Parent = this;
            this.Aspects.Add(aspect.Name, aspect);
        }
    }

    public class AppSection : ComplexAspect
    {
        public readonly App App;
        public AppSection(App app) : base(app.ToString())
        {
            this.App = app;
        }
        public override string GetAspectPath(){
            return null;
        }
    }


    public class SimpleAspect : Aspect
    {
        public readonly object DefaultValue;
        public readonly Type Type;

        public SimpleAspect(string name, Type type, object defaultValue) : base(name)
        {
            if(type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!(defaultValue == null) && !(type.IsInstanceOfType(defaultValue)))
            {
                throw new ArgumentException(String.Format("{0} not convertable to type {1}", defaultValue.GetType().FullName, type.FullName),"defaultValue");
            }
            this.DefaultValue = defaultValue;
            this.Type = type;
        }
    }
}