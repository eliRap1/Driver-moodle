using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace driver_client
{
    public partial class SignUp : Page
    {
        public static int Tid = 0;
        private Sign sign;
        private bool _isTeacher;
        private string _password = "";
        LogIn login = new LogIn();

        public SignUp(bool isTeacher)
        {
            InitializeComponent();
            _isTeacher = isTeacher;

            sign = new Sign();
            sign.TeacherId = Tid;
            sign.LessonPrice = 200; // Default lesson price
            this.DataContext = sign;

            SetupRoleBadge();
            SetupFieldsVisibility();
        }

        private void SetupRoleBadge()
        {
            if (_isTeacher)
            {
                // Green badge for teacher
                roleBadge.Background = new LinearGradientBrush(
                    Color.FromRgb(0x43, 0xA0, 0x47),
                    Color.FromRgb(0x2E, 0x7D, 0x32),
                    45);
                roleText.Text = "Teacher";
                try
                {
                    roleIcon.Source = new BitmapImage(new Uri("/picture/driver.png", UriKind.Relative));
                }
                catch { }
            }
            else
            {
                // Blue badge for student
                roleBadge.Background = new LinearGradientBrush(
                    Color.FromRgb(0x1E, 0x88, 0xE5),
                    Color.FromRgb(0x15, 0x65, 0xC0),
                    45);
                roleText.Text = "Student";
                try
                {
                    roleIcon.Source = new BitmapImage(new Uri("/picture/student.png", UriKind.Relative));
                }
                catch { }
            }
        }

        private void SetupFieldsVisibility()
        {
            if (_isTeacher)
            {
                // Show teacher fields
                lessonPriceText.Visibility = Visibility.Visible;
                lessonPrice_border.Visibility = Visibility.Visible;
                paymentMethodsText.Visibility = Visibility.Visible;
                paymentMethods_border.Visibility = Visibility.Visible;

                // Hide student fields
                idTecherText.Visibility = Visibility.Collapsed;
                teacher_border.Visibility = Visibility.Collapsed;
            }
            else
            {
                // For students, teacher was already selected in ChooseTeacher page
                // Hide the teacher ID field since Tid is already set
                idTecherText.Visibility = Visibility.Collapsed;
                teacher_border.Visibility = Visibility.Collapsed;

                // Hide teacher fields
                lessonPriceText.Visibility = Visibility.Collapsed;
                lessonPrice_border.Visibility = Visibility.Collapsed;
                paymentMethodsText.Visibility = Visibility.Collapsed;
                paymentMethods_border.Visibility = Visibility.Collapsed;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            if (_isTeacher)
            {
                // Teachers go back to role selection
                page.Navigate(new RoleSelection());
            }
            else
            {
                // Students go back to choose teacher
                page.Navigate(new ChooseTeacher(chooseTeacher: true));
            }
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            page.Navigate(login);
        }

        private void Pass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _password = pass.Password;
        }

        private void signup_Click(object sender, RoutedEventArgs e)
        {
            string userN = sign.Username;
            string password = _password;
            int age1 = sign.Age;
            string emailT = sign.Email;
            string phone1 = sign.Phone;

            driver.Service1Client srv = new driver.Service1Client();

            // Basic validation
            if (Validation.GetHasError(username) || Validation.GetHasError(age) ||
                Validation.GetHasError(phone) || Validation.GetHasError(email))
            {
                MessageBox.Show("Please fix the highlighted fields");
                return;
            }

            // Password validation
            if (string.IsNullOrEmpty(password) || password.Length < 3)
            {
                MessageBox.Show("Password must be at least 3 characters");
                return;
            }

            // Teacher-specific validation
            if (_isTeacher && Validation.GetHasError(lessonPrice))
            {
                MessageBox.Show("Please enter a valid lesson price");
                return;
            }

            if (string.IsNullOrWhiteSpace(userN) || string.IsNullOrWhiteSpace(emailT) ||
                string.IsNullOrWhiteSpace(phone1) || age1 <= 0)
            {
                MessageBox.Show("Please fill all the fields correctly");
                return;
            }

            // For students, verify teacher was selected (from ChooseTeacher page)
            if (!_isTeacher && Tid <= 0)
            {
                MessageBox.Show("No teacher selected. Please go back and choose a teacher.");
                return;
            }

            if (password != confirmPass.Password)
            {
                MessageBox.Show("Passwords don't match");
                return;
            }

            try
            {
                if (srv.CheckUserExist(userN))
                {
                    MessageBox.Show("Username already exists");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to server: " + ex.Message);
                return;
            }

            // Get lesson price for teachers
            int lessonPriceValue = 200;
            if (_isTeacher)
            {
                if (!int.TryParse(lessonPrice.Text, out lessonPriceValue) || lessonPriceValue < 0)
                {
                    lessonPriceValue = 200;
                }
            }

            // Get teacher ID for students (already set from ChooseTeacher page)
            int tid = 0;
            if (!_isTeacher)
            {
                tid = Tid; // Use the static Tid set by ChooseTeacher
            }

            // Register user
            try
            {
                bool success = srv.AddUser(userN, password, emailT, phone1, _isTeacher, tid, lessonPriceValue);

                if (success)
                {
                    MessageBox.Show("You are successfully registered!");

                    // If teacher, save payment methods
                    if (_isTeacher)
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
                    MessageBox.Show("Registration failed. Please check your information and try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during registration: " + ex.Message);
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex}");
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

        private void notSure_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You are transferred to the choose teacher page");
            page.Navigate(new ChooseTeacher(true));
            this.DataContext = null;
        }
    }
}
