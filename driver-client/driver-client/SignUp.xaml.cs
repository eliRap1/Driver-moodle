using System;
using System.Collections.Generic;
using System.Data.OleDb;
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
    /// Interaction logic for SignUp.xaml
    /// </summary>
    public partial class SignUp : Page
    {
        static List<string> users = new List<string>() {"eli", "moshe", "daniel", "david", "omer", "yossi"};//list of autorized users
        private Sign sign;

        public SignUp()
        {
            InitializeComponent();
            role.Items.Add("Student");
            role.Items.Add("Teacher");
            sign = new Sign();
            this.DataContext = sign;
        }

        private void signup_Click(object sender, RoutedEventArgs e)
        {
            string userN = sign.Username;
            string password = sign.Password;
            int age1 = sign.Age;
            string emailT = sign.Username;
            string phone1 = sign.Phone;
            bool isTecher = false;
            username_border.BorderThickness = new Thickness(0);
            pass_border.BorderThickness = new Thickness(0);
            age_border.BorderThickness = new Thickness(0);
            email_border.BorderThickness = new Thickness(0);
            if(role.SelectedItem == "Teacher")// && pass.Password == "DriverT!" && users.Contains(username.Text))//check if the admin password is right and if the user is in the list of autorized users
            {
                isTecher = true;
            }
            driver.Service1Client srv = new driver.Service1Client();
            if ((Validation.GetHasError(username) || Validation.GetHasError(age) || Validation.GetHasError(pass)) || Validation.GetHasError(phone) ||
                Validation.GetHasError(email) || role.SelectedIndex == -1)
            {
                MessageBox.Show("username and password must be atleast 3 letters, email and phone number must be real, if you didnt select a role do it");
            }
            else if(!srv.CheckUserExist(userN))
            {
                if(srv.AddUser(userN, password,emailT, phone1, isTecher))
                {
                    MessageBox.Show("You are successfully registered");

                }
                else
                {
                    MessageBox.Show("Error!!");
                }
                
            }


        }
    }
}
