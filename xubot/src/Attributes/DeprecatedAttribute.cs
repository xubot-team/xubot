﻿using System;
using System.Collections.Generic;
using System.Text;

namespace xubot.src.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DeprecatedAttribute : Attribute
    {
        public DeprecatedAttribute() {}
    }
}
