using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmi.ps.mcschema
{
    public class ElementDependencyNotFulfilled : Exception
    {
        public ElementDependencyNotFulfilled() { }
        public ElementDependencyNotFulfilled(string message) : base(message) { }
        public ElementDependencyNotFulfilled(string message, Exception inner) : base(message, inner) { }
    }
}
