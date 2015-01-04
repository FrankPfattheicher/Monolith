using System.Runtime.InteropServices;
using System.Windows;
using IctBaden.Framework;

namespace TestWpfApp1
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

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var t = new PassiveTimer(1000);
            lblLoadResult.Content = "Loaded successfully.";
        }
    }
}
