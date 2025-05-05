using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace driver_client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LogIn login = new LogIn();
        SignUp signup = new SignUp();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void signUp_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(signup);
            //welcomePanel.Visibility = Visibility.Collapsed;

        }

        private void signIn_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(login);
            //welcomePanel.Visibility = Visibility.Collapsed;

        }
    }
}
