using NUnit.Framework;
using RazorEngine.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.RazorEngine.TestTypes.BaseTypes;

namespace Test.RazorEngine
{
    /// <summary>
    /// Check that the CompilerServicesUtility class behaves.
    /// </summary>
    [TestFixture]
    public class CompilerServicesUtilityTestFixture
    {
        /// <summary>
        /// Check that we can generate the raw type name from a nested generic class.
        /// </summary>
        [Test]
        public void CompilerServiceUtility_CheckNestedClass()
        {
            var result = CompilerServicesUtility.CSharpGetRawTypeName(typeof(HostingClass.NestedBaseClass<>));
            Assert.AreEqual("Test.RazorEngine.TestTypes.BaseTypes.HostingClass.NestedBaseClass", result);
        }

        /// <summary>
        /// Check that we can generate the raw type name from a normal generic class
        /// </summary>
        [Test]
        public void CompilerServiceUtility_CheckNormalGenericClass()
        {
            var result = CompilerServicesUtility.CSharpGetRawTypeName(typeof(AddLanguageInfo_Viewbag<>));
            Assert.AreEqual("Test.RazorEngine.TestTypes.BaseTypes.AddLanguageInfo_Viewbag", result);
        }
    }
}
