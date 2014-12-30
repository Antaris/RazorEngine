using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImpromptuInterface
{
    /// <summary>
    /// Attribute on Inteface to stop proxy from recursively
    /// proxying other interfaces
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Method |
                          System.AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public class NonRecursiveInterfaceAttribute : Attribute
    {
    }
}
