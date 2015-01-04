using System.Runtime.InteropServices;

namespace TestConsole1
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void OutputDebugString(string lpOutputString);

        public Program()
        {
            // never called because Program is not static and no instance is created
            OutputDebugString("public Program()");
        }
        static void Main(string[] args)
        {
            OutputDebugString("static void Main(string[] args)");
        }
    }
}
