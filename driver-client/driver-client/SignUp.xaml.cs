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
    public class RoleOption
    {
        public string Name { get; set; }
        public string Icon { get; set; }
    }

    public partial class SignUp : Page
    {
        public static int Tid = 0;
        static List<string> users = new List<string>() { "eli", "moshe", "daniel", "david", "omer", "yossi" };
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
            sign.LessonPrice = 200; // Default lesson price
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
            bool isTeacher = false;

            if (role.SelectedIndex == 2)
            {
                isTeacher = true;
            }

            driver.Service1Client srv = new driver.Service1Client();

            // Basic validation
            if ((Validation.GetHasError(username) || Validation.GetHasError(age) || Validation.GetHasError(pass)) ||
                Validation.GetHasError(phone) || Validation.GetHasError(email))
            {
                MessageBox.Show("Change the highlighted fields");
                return;
            }

            // Teacher-specific validation
            if (isTeacher && Validation.GetHasError(lessonPrice))
            {
                MessageBox.Show("Please enter a valid lesson price");
                return;
            }

            // Student-specific validation
            if (!isTeacher && Validation.GetHasError(teacherId))
            {
                MessageBox.Show("Please enter a valid teacher ID");
                return;
            }

            if (userN == null || password == null || emailT == null || phone1 == null ||
                role.SelectedIndex == -1 || role.SelectedIndex == 0 || age1 == -1 ||
                (!isTeacher && teacherId.Text == "0"))
            {
                MessageBox.Show("Please fill all the fields correctly");
                return;
            }

            if (password != confirmPass.Text)
            {
                MessageBox.Show("Passwords don't match");
                return;
            }

            if (srv.CheckUserExist(userN))
            {
                MessageBox.Show("Username already exists");
                return;
            }

            // Get lesson price for teachers
            int lessonPriceValue = 200;
            if (isTeacher)
            {
                if (!int.TryParse(lessonPrice.Text, out lessonPriceValue) || lessonPriceValue < 0)
                {
                    lessonPriceValue = 200;
                }
            }

            // Get teacher ID for students
            int tid = 0;
            if (!isTeacher)
            {
                int.TryParse(teacherId.Text, out tid);
            }

            // Register user
            if (srv.AddUser(userN, password, emailT, phone1, isTeacher, tid, lessonPriceValue))
            {
                MessageBox.Show("You are successfully registered");

                // If teacher, save payment methods
                if (isTeacher)
                {
                    try
                    {
                        int newTeacherId = srv.GetUserID(userN, "Teacher");
                        string paymentMethods = GetSelectedPaymentMethods();
                        srv.UpdatePaymentMethods(newTeacherId, paymentMethods);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating payment methods: {ex.Message}");
                    }
                }

                // Navigate
                if (tid == -1)
                {
                    page.Navigate(new ChooseTeacher(true));
                }
                else
                {
                    page.Navigate(login);
                }
            }
            else
            {
                MessageBox.Show("Error!");
            }
        }

        private string GetSelectedPaymentMethods()
        {
            List<string> methods = new List<string>();

            if (chkCash.IsChecked == true) methods.Add("Cash");
            if (chkCreditCard.IsChecked == true) methods.Add("Credit Card");
            if (chkBankTransfer.IsChecked == true) methods.Add("Bank Transfer");
            if (chkBit.IsChecked == true) methods.Add("Bit");
            if (chkPaybox.IsChecked == true) methods.Add("Paybox");

            return methods.Count > 0 ? string.Join(",", methods) : "Cash";
        }

        private void role_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (role.SelectedIndex == 1) // Student
            {
                // Show student fields
                teacherId.Visibility = Visibility.Visible;
                idTecherText.Visibility = Visibility.Visible;
                teacher_border.Visibility = Visibility.Visible;

                // Hide teacher fields
                lessonPriceText.Visibility = Visibility.Hidden;
                lessonPrice_border.Visibility = Visibility.Hidden;
                paymentMethodsText.Visibility = Visibility.Hidden;
                paymentMethods_border.Visibility = Visibility.Hidden;
            }
            else if (role.SelectedIndex == 2) // Teacher
            {
                // Hide student fields
                teacherId.Visibility = Visibility.Hidden;
                idTecherText.Visibility = Visibility.Hidden;
                teacher_border.Visibility = Visibility.Hidden;

                // Show teacher fields
                lessonPriceText.Visibility = Visibility.Visible;
                lessonPrice_border.Visibility = Visibility.Visible;
                paymentMethodsText.Visibility = Visibility.Visible;
                paymentMethods_border.Visibility = Visibility.Visible;
            }
            else // Choose (not selected)
            {
                // Hide all role-specific fields
                teacherId.Visibility = Visibility.Hidden;
                idTecherText.Visibility = Visibility.Hidden;
                teacher_border.Visibility = Visibility.Hidden;
                lessonPriceText.Visibility = Visibility.Hidden;
                lessonPrice_border.Visibility = Visibility.Hidden;
                paymentMethodsText.Visibility = Visibility.Hidden;
                paymentMethods_border.Visibility = Visibility.Hidden;
            }
        }

        private void notSure_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You are transferred to the choose teacher page");
            page.Navigate(new ChooseTeacher(true));
            this.DataContext = null;
        }
    }
}