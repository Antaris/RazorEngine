using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine;
using NUnit.Framework;

namespace Test.RazorEngine
{
    /// <summary>
    /// Various general tests.
    /// </summary>
    [TestFixture]
    public class VariousTestsFixture
    {
        /// <summary>
        /// Test if we can call GetTypes on the RazorEngine assembly.
        /// This will make sure all SecurityCritical attributes are valid.
        /// </summary>
        [Test]
        public void AssemblyIsScannable()
        {
            typeof(Engine).Assembly.GetTypes();
        }
    }
}
