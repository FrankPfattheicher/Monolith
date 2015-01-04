using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace TestWpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void OutputDebugString(string lpOutputString);
        public App()
        {
            OutputDebugString("public App()");
            //AppDomain.CurrentDomain.AssemblyResolve += Monolith.MonolithAssemblyResolver.CurrentDomainOnAssemblyResolve;
        }
    }
}
