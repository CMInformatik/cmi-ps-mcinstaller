using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

namespace cmi.ps.mcschema
{
    public interface IElementDependency
    {
        void Verify(PSObject data);
        void Ensure(PSObject data);
    }
}
