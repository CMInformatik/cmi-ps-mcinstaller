using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmi.mc.config
{
    public class AppSection : Element
    {
        public readonly IDictionary<string, Aspect> Aspects = new Dictionary<string, Aspect>();
        public readonly App App;

        public AppSection(App app) : base(app.ToString(), ConfigControlAttribute.NotSet)
        {
            this.App = app;
        }

        public void AddAspect(Aspect aspect)
        {
            if (aspect == null)
            {
                throw new ArgumentNullException(nameof(aspect));
            }
            this.Aspects.Add(aspect.Name, aspect);
        }

        public IEnumerable<Aspect> Traverse() => Aspects.Values.SelectMany(item => item.Traverse());
    }
}
