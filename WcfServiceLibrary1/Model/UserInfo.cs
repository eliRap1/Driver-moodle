using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class UserInfo : Base
    {
        private string username;
        private string password;
        private string email;
        private string phone;
        private bool isAdmin;
        public bool IsAdmin { get => isAdmin; set => isAdmin = value; }
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public string Email { get => email; set => email = value; }
        public string Phone { get => phone; set => phone = value; }
    }
}
