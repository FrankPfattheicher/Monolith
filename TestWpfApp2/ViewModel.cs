using System.Runtime.InteropServices;
using IctBaden.Presentation;

namespace TestWpfApp2
{
    public class ViewModel : ActiveViewModel
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void OutputDebugString(string lpOutputString);
        public ViewModel()
        {
            OutputDebugString("public ViewModel()");
        }
         
    }
}