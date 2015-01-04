using System.Runtime.InteropServices;
using ClassLibrary1;

namespace TestConsole2
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void OutputDebugString(string lpOutputString);
        static void Main(string[] args)
        {
            OutputDebugString("static void Main(string[] args)");

            var version = new VersionInfo();
            OutputDebugString("version = " + version.Version);
        }
    }
}
