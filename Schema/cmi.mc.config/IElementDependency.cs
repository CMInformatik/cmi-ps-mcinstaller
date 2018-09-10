using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config
{
    public interface IElementDependency
    {
        void Verify(JContainer data);
        void Ensure(JContainer data);
    }
}
