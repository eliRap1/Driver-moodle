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
        static public Sign sign = new Sign();
        public LogIn()
        {
            InitializeComponent();
            this.DataContext = sign;
            //driver.Service1Client srv = new driver.Service1Client();
            //
        }

        private void signIn_Click(object sender, RoutedEventArgs e)
        {
            string password = pass.Password;
            string user = username.Text;

            driver.Service1Client srv = new driver.Service1Client();
            if (srv.CheckUserPassword(user, password))
            {
                sign.Username = user;
                sign.Password = password;
                
                if(srv.CheckUserAdmin(user))
                {
                    sign.IsTeacher = true;
                    sign.Id = srv.GetUserID(user, "Teacher");
                    page.Navigate(new TeacherUI());
                }
                else
                {
                    sign.IsTeacher = false;
                    sign.Id = srv.GetUserID(user, "Student");
                    page.Navigate(new StudentUI());
                }
            }
            else
             {
                MessageBox.Show("Wrong password or username");
            }

        }
        private void signUp_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new RoleSelection());
        }
    }
}
