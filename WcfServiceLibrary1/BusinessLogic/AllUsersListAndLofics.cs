using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewDB;

namespace BusinessLogic
{
    public class AllUsersListAndLofics
    {
        public static void AddStudent(UserInfo user)
        {
            new UserDB().AddStudent(user);
        }

        public static AllUsers GetAllStudents()
        {
            return new UserDB().GetAllStudents();
        }

    }
}
