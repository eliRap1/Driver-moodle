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
    /// Interaction logic for SignUp.xaml
    /// </summary>
    public partial class SignUp : Page
    {
        static List<string> users = new List<string>() {"eli", "moshe", "daniel", "david", "omer", "yossi"};//list of autorized users

        public SignUp()
        {
            InitializeComponent();
            role.Items.Add("Student");
            role.Items.Add("Admin");
        }

        private void signup_Click(object sender, RoutedEventArgs e)
        {
            string user = this.username.Text;
            string password = this.pass.Password;
            if(role.SelectedItem == "Admin" && pass.Password == "DriverT!" && users.Contains(username.Text))//check if the admin password is right and if the user is in the list of autorized users
            {
                
            }
        }
    }
}
