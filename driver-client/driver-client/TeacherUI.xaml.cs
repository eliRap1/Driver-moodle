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
    /// Interaction logic for TeacherUI.xaml
    /// </summary>
    public partial class TeacherUI : Page
    {
        public TeacherUI()
        {
            InitializeComponent();
            teacherName.Text = LogIn.sign.Username;
        }

        private void Students_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new AllStudents(LogIn.sign.Id));
        }

        private void Calendar_Click(object sender, RoutedEventArgs e)
        {
            CalendarTeacher calendar = new CalendarTeacher(LogIn.sign);
            page.Navigate(calendar);
        }

        private void TestSchedule_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Chat_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new Chat());
        }
    }
}
