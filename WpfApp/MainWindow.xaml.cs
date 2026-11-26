using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            var pars = ConnectionToOpcServer.GetParameters();
            TextBoxMetrazh.Text = pars.Item1.ToString();
            TextBoxSpeed.Text = pars.Item2.ToString();

        }
    }
}