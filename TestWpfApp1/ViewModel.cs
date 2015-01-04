using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace TestWpfApp1
{
    public class ViewModel
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void OutputDebugString(string lpOutputString);
        public ViewModel()
        {
            OutputDebugString("public ViewModel()");

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                OutputDebugString("AssemblyResolve " + args.Name);
                return null;
            };
        }
         
    }
}