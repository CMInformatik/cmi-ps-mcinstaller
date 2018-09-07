﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cmi.ps.mcschema
{
    public class SimpleAspectDependency : IElementDependency
    {
        private readonly App _app;
        private readonly SimpleAspect _aspect;
        private readonly string _aspectPath;
        private readonly object _value;

        public SimpleAspectDependency(App app, string aspectPath, object value)
        {
            Aspect.ThrowIfInvalidAspectPath(aspectPath);
            this._app = app;
            this._value = value;
            this._aspectPath = aspectPath;
        }


        public void Verify(PSObject data)
        {
            throw new NotImplementedException();
        }

        public void Ensure(PSObject data)
        {
            throw new NotImplementedException();
        }
    }
}
