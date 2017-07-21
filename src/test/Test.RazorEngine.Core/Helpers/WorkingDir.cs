using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.RazorEngine.Helpers
{
    public sealed class WorkingDir : IDisposable
    {
        private readonly string dir;

        public WorkingDir()
        {
            dir = Path.GetTempFileName();
            File.Delete(dir);
            Directory.CreateDirectory(dir);
        }

        public static WorkingDir Create()
        {
            return new WorkingDir();
        }

        public string GetDir() => dir;

        public string GetTempFile()
        {
            return Path.Combine(GetDir(), Path.GetRandomFileName());
        }

        public void Dispose()
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
