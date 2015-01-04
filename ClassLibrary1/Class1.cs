using System;
using System.Runtime.InteropServices;

namespace ClassLibrary1
{
    public class Class1
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void OutputDebugString(string lpOutputString);
        public Class1()
        {
            OutputDebugString("public ClassLibrary1.Class1()");
        }

        public string Version { get { return "1.00"; } }
    }
}
