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
        //private bool isAdmin;

        public bool isAdmin { get => isAdmin; set => isAdmin = value; }
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
    }
}
