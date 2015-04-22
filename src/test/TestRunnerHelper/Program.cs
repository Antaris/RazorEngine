using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRunnerHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = new Test.RazorEngine.IsolatedRazorEngineServiceTestFixture();
            t.IsolatedRazorEngineService_BadTemplate_InSandbox();
        }
    }
}
