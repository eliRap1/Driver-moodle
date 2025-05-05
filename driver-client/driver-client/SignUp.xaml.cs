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
    public class RoleOption
    {
        public string Name { get; set; }
        public string Icon { get; set; }
    }
    public partial class SignUp : Page
    {
        static List<string> users = new List<string>() {"eli", "moshe", "daniel", "david", "omer", "yossi"};//list of autorized users
        private Sign sign;

        public SignUp()
        {
            InitializeComponent();
            var options = new List<RoleOption>
            {
                new RoleOption { Name = "Choose", Icon = "picture/chose.png" },
                new RoleOption { Name = "Student", Icon = "picture/student.png" },
                new RoleOption { Name = "Teacher", Icon = "picture/driver.png" }
            };
            role.ItemsSource = options;
            role.SelectedIndex = 0;
            sign = new Sign();
            this.DataContext = sign;
        }

        private void signup_Click(object sender, RoutedEventArgs e)
        {
            string userN = sign.Username;
            string password = sign.Password;
            int age1 = sign.Age;
            string emailT = sign.Email;
            string phone1 = sign.Phone;
            bool isTecher = false;
            //username_border.BorderThickness = new Thickness(0);
            //pass_border.BorderThickness = new Thickness(0);
            //age_border.BorderThickness = new Thickness(0);
            //email_border.BorderThickness = new Thickness(0);
            if(role.SelectedIndex == 2)// && pass.Password == "DriverT!" && users.Contains(username.Text))//check if the admin password is right and if the user is in the list of autorized users
            {
                isTecher = true;
            }
            driver.Service1Client srv = new driver.Service1Client();
            if ((Validation.GetHasError(username) || Validation.GetHasError(age) || Validation.GetHasError(pass)) || Validation.GetHasError(phone) ||
                Validation.GetHasError(email) || Validation.GetHasError(teacherId))
            {
                MessageBox.Show("Change the highlighted fields");
            }
            else if(userN == null || password == null || emailT == null || phone1 == null || role.SelectedIndex == -1 || role.SelectedIndex == 0)
            {
                MessageBox.Show("Please fill all the fields correctly");
            }
            else if(!srv.CheckUserExist(userN))
            {
                if(srv.AddUser(userN, password, emailT, phone1, isTecher, int.Parse(teacherId.Text)))
                {
                    MessageBox.Show("You are successfully registered");
                }
                else
                {
                    MessageBox.Show("Error!");
                }
            }
            else
            {
                MessageBox.Show("Username already exist");
            }

        }

        private void role_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (role.SelectedIndex == 1)
            {
                teacherId.Visibility = Visibility.Visible;
                idTecherText.Visibility = Visibility.Visible;
                teacher_border.Visibility = Visibility.Visible;
            }
            else
            {
                teacherId.Visibility = Visibility.Hidden;
                idTecherText.Visibility = Visibility.Hidden;
                teacher_border.Visibility = Visibility.Hidden;

            }
        }
    }
}
