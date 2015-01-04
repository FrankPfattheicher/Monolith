using System.Runtime.InteropServices;

namespace TestConsole1
{
    public class StaticClass1
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void OutputDebugString(string lpOutputString);
        public StaticClass1()
        {
            // never called due to this class is referenced nowhere
            OutputDebugString("public StaticClass1()");
        } 
    }
}