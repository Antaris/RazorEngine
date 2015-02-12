using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.RazorEngine.TestTypes.BaseTypes
{
    public class HostingClass 
    {
        public class NestedBaseClass<T> : TemplateBase<T>
        {
            public string TestProperty { get { return "mytest"; } }
        }
    }
}
