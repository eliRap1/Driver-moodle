using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace driver_client
{
    public class Sign
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int TeacherId { get; set; }
        public bool IsTeacher { get; set; }
        public bool IsAdmin { get; set; }
        public int Id { get; set; }
        public string RatingText { get; set; }
        public string Role { get; set; }
        public int LessonPrice { get; set; } = 200;
        public string PaymentMethods { get; set; } = "Cash,Credit Card,Bank Transfer";

        /// <summary>
        /// Verifies that the currently signed-in user is an admin teacher.
        /// Should be called at the top of every admin-only Page constructor.
        /// Returns true and lets the caller proceed; returns false after showing
        /// an error and navigating back.
        /// </summary>
        public static bool RequireAdmin(Page page)
        {
            try
            {
                var sign = LogIn.sign;
                if (sign == null || !sign.IsTeacher || string.IsNullOrEmpty(sign.Username))
                {
                    MessageBox.Show("Access denied. Admin privileges required.",
                                    "Unauthorized", MessageBoxButton.OK, MessageBoxImage.Stop);
                    page?.NavigationService?.GoBack();
                    return false;
                }

                var srv = new Service1Client();
                bool ok = srv.IsUserAdmin(sign.Username);
                try { srv.Close(); } catch { srv.Abort(); }
                if (!ok)
                {
                    MessageBox.Show("Access denied. Admin privileges required.",
                                    "Unauthorized", MessageBoxButton.OK, MessageBoxImage.Stop);
                    page?.NavigationService?.GoBack();
                }
                sign.IsAdmin = ok;
                return ok;
            }
            catch
            {
                MessageBox.Show("Could not verify admin status.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}