using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmi.mc.config
{
    public enum App
    {
        Common,
        Mobileclients,
        Dossierbrowser,
        Sitzungsvorbereitung,
        Zusammenarbeitdritte
    }

    public enum ConfigControlAttribute
    {
        Extend,
        Replace,
        Remove,
        Internal,
        Private,
        NotSet
    }

    public enum AxSupport
    {
        R16_1,
        R17,
        R18
    }
}
