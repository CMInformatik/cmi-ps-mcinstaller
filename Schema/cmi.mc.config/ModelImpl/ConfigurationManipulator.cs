using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cmi.mc.config.ModelContract.Exceptions;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config.ModelImpl
{
    public abstract class ConfigurationManipulator
    {
        protected JProperty Configuration { get; private set; }
        protected ISchema Schema { get; }

        internal ConfigurationManipulator(JProperty configuration, ISchema schema)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            if (configuration.Type != JTokenType.Property)
            {
                throw new InvalidConfigurationException($"{configuration.Path} is not a JSON property.", null, Configuration.Path);
            }
        }

        protected void RevertChangesOnFailure(Action action)
        {
            Debug.Assert(action != null);
            var beforeChanges = (JProperty)Configuration.DeepClone();
            try
            {
                action.Invoke();
            }
            catch (Exception)
            {
                Configuration.Replace(beforeChanges);
                Configuration = beforeChanges;
                throw;
            }
        }
    }
}
