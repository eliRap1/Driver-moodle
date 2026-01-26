using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace driver_client
{
    public partial class RoleSelection : Page
    {
        public RoleSelection()
        {
            InitializeComponent();
        }

        private void StudentCard_Click(object sender, MouseButtonEventArgs e)
        {
            // Students must first choose a teacher before signing up
            page.Navigate(new ChooseTeacher(chooseTeacher: true));
        }

        private void TeacherCard_Click(object sender, MouseButtonEventArgs e)
        {
            // Navigate to signup as teacher (isTeacher = true)
            page.Navigate(new SignUp(isTeacher: true));
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new LogIn());
        }
    }
}
