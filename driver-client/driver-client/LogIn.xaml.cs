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
    /// Interaction logic for SignIn.xaml
    /// </summary>
    public partial class LogIn : Page
    {
        SignUp signup = new SignUp();
        //private Sign sign = new Sign();
        public LogIn()
        {
            InitializeComponent();
            //this.DataContext = sign;
        }

        private void signIn_Click(object sender, RoutedEventArgs e)
        {
            string password = pass.Password;
            string user = username.Text;

            driver.Service1Client srv = new driver.Service1Client();
            if (srv.CheckUserPassword(user, password))
            {
                if(srv.CheckUserAdmin(user))
                {
                    //page.Navigate(new Admin());
                }
                else
                {
                    //page.Navigate(new Home());
                }
            }
            else
             {
                MessageBox.Show("Wrong password or username");
            }

        }
        private void signUp_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(signup);
        }
    }
}
