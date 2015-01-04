using System.Runtime.InteropServices;
using System.Windows;

namespace TestWpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void OutputDebugString(string lpOutputString);
        public MainWindow()
        {
            OutputDebugString("public MainWindow()");
            InitializeComponent();
        }
    }
}
