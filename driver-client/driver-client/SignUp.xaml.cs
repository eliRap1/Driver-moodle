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
        public static int Tid = 0;
        static List<string> users = new List<string>() {"eli", "moshe", "daniel", "david", "omer", "yossi"};//list of autorized users
        private Sign sign;
        LogIn login = new LogIn();
        public SignUp(int i = 0)
        {
            InitializeComponent();
            var options = new List<RoleOption>
            {
                new RoleOption { Name = "Choose", Icon = "picture/chose.png" },
                new RoleOption { Name = "Student", Icon = "picture/student.png" },
                new RoleOption { Name = "Teacher", Icon = "picture/driver.png" }
            };
            role.ItemsSource = options;
            role.SelectedIndex = i;     
            sign = new Sign();
            sign.TeacherId = Tid;
            this.DataContext = sign;
        }
        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            page.Navigate(login);
        }


        private void signup_Click(object sender, RoutedEventArgs e)
        {
            string userN = sign.Username;
            string password = sign.Password;
            int age1 = sign.Age;
            string emailT = sign.Email;
            string phone1 = sign.Phone;
            bool isTecher = false;

            if(role.SelectedIndex == 2)// && pass.Password == "DriverT!" && users.Contains(username.Text))//check if the admin password is right and if the user is in the list of autorized users
            {
                isTecher = true;
            }
            driver.Service1Client srv = new driver.Service1Client();
            if ((Validation.GetHasError(username) || Validation.GetHasError(age) || Validation.GetHasError(pass)) || Validation.GetHasError(phone) ||
                Validation.GetHasError(email) || (isTecher && Validation.GetHasError(teacherId)))
            {
                MessageBox.Show("Change the highlighted fields");
            }
            else if(userN == null || password == null || emailT == null || phone1 == null || role.SelectedIndex == -1 || role.SelectedIndex == 0 || age1 == -1 || (!isTecher && teacherId.Text == "0"))
            {
                MessageBox.Show("Please fill all the fields correctly");
            }
            else if(password != confirmPass.Text)
            {
                MessageBox.Show("Passwords don't match");
            }
            else if(!srv.CheckUserExist(userN))
            {
                int tid = int.Parse(teacherId.Text);
                if(srv.AddUser(userN, password, emailT, phone1, isTecher, tid))
                {
                    MessageBox.Show("You are successfully registered");
                    if(tid == -1)
                    {
                        page.Navigate(new ChooseTeacher(true));
                    }
                    page.Navigate(login);
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
                notSure.Visibility = Visibility.Visible;
            }
            else
            {
                teacherId.Visibility = Visibility.Hidden;
                idTecherText.Visibility = Visibility.Hidden;
                teacher_border.Visibility = Visibility.Hidden;
                notSure.Visibility = Visibility.Hidden;
            }
        }

        private void notSure_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You are transferd to the choose teacher page");
            page.Navigate(new ChooseTeacher(true));
            this.DataContext = null;
            
        }
    }
}
