using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.RazorEngine.TestTypes.BaseTypes
{
    /// <summary>
    /// Test class.
    /// </summary>
    public class HostingClass
    {
        /// <summary>
        /// Test class.
        /// </summary>
        public class NestedBaseClass<T> : TemplateBase<T>
        {
            /// <summary>
            /// Test class.
            /// </summary>
            public string TestProperty { get { return "mytest"; } }
        }

        /// <summary>
        /// Test class.
        /// </summary>
        public class NonGenericNestedBaseClass : TemplateBase
        {
            /// <summary>
            /// Test class.
            /// </summary>
            public string TestProperty { get { return "mytest"; } }
        }

        /// <summary>
        /// Test class.
        /// </summary>
        public class NestedClass
        {
            /// <summary>
            /// Test class.
            /// </summary>
            public string TestProperty { get; set; }
        }

        /// <summary>
        /// Test class.
        /// </summary>
        public class GenericNestedClass<T>
        {
            /// <summary>
            /// Test class.
            /// </summary>
            public T TestProperty { get; set; }
        }
    }
}
