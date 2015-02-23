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
        public void CompilerServiceUtility_CheckNestedBaseClass()
        {
            var result = CompilerServicesUtility.CSharpGetRawTypeName(typeof(HostingClass.NestedBaseClass<>));
            Assert.AreEqual("Test.RazorEngine.TestTypes.BaseTypes.HostingClass.NestedBaseClass", result);
        }

        /// <summary>
        /// Check that we can generate the raw type name from a nested generic class.
        /// </summary>
        [Test]
        public void CompilerServiceUtility_CheckNestedNonGenericBaseClass()
        {
            var result = CompilerServicesUtility.CSharpGetRawTypeName(typeof(HostingClass.NonGenericNestedBaseClass));
            Assert.AreEqual("Test.RazorEngine.TestTypes.BaseTypes.HostingClass.NonGenericNestedBaseClass", result);
        }
        
        /// <summary>
        /// Check that we can generate the raw type name from a normal generic class
        /// </summary>
        [Test]
        public void CompilerServiceUtility_CheckNormalGenericBaseClass()
        {
            var result = CompilerServicesUtility.CSharpGetRawTypeName(typeof(AddLanguageInfo_Viewbag<>));
            Assert.AreEqual("Test.RazorEngine.TestTypes.BaseTypes.AddLanguageInfo_Viewbag", result);
        }

        private IEnumerable<string> IteratorHelper()
        {
            yield return "first";
            yield return "second";
        }
        /// <summary>
        /// Check that we can generate the type name from a iterator type
        /// </summary>
        [Test]
        public void CompilerServiceUtility_CheckCSharpIteratorType()
        {
            var type = IteratorHelper().GetType();
            var result = CompilerServicesUtility.ResolveCSharpTypeName(type);
            Assert.AreEqual("System.Collections.Generic.IEnumerable<System.String>", result);
        }

        /// <summary>
        /// Check that we can generate the type name from a dynamic type
        /// </summary>
        [Test]
        public void CompilerServiceUtility_CheckDynamicType()
        {
            var result = CompilerServicesUtility.ResolveCSharpTypeName(typeof(System.Dynamic.DynamicObject));
            Assert.AreEqual("dynamic", result);
        }

        /// <summary>
        /// Check that we can generate the type name from a normal class
        /// </summary>
        [Test]
        public void CompilerServiceUtility_CheckNonGenericType()
        {
            var result = CompilerServicesUtility.ResolveCSharpTypeName(typeof(System.String));
            Assert.AreEqual("System.String", result);
        }

        /// <summary>
        /// Check that we can generate the type name from a normal nested class
        /// </summary>
        [Test]
        public void CompilerServiceUtility_CheckNonGenericNestedType()
        {
            var result = CompilerServicesUtility.ResolveCSharpTypeName(typeof(HostingClass.NestedClass));
            Assert.AreEqual("Test.RazorEngine.TestTypes.BaseTypes.HostingClass.NestedClass", result);
        }

        /// <summary>
        /// Check that we can generate the type name from a generic nested class
        /// </summary>
        [Test]
        public void CompilerServiceUtility_CheckGenericNestedType()
        {
            var result = CompilerServicesUtility.ResolveCSharpTypeName(typeof(HostingClass.GenericNestedClass<string>));
            Assert.AreEqual("Test.RazorEngine.TestTypes.BaseTypes.HostingClass.GenericNestedClass<System.String>", result);
        }
    }
}
