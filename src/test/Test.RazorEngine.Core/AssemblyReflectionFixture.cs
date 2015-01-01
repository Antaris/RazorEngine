using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine;
using NUnit.Framework;

namespace Test.RazorEngine
{
    [TestFixture]
    public class AssemblyReflectionFixture
    {
        [Test]
        public void AssemblyIsScannable()
        {
            typeof(Engine).Assembly.GetTypes();
        }
    }
}
