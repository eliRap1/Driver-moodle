using Model;
using BusinessLogic;
using ViewDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfServiceLibrary1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class Service1 : IService1
    {
        public bool AddUser(string name, string password)
        {
            AllUsers allUsers = new AllUsers();
            ViewDB.UserDB userDB = new ViewDB.UserDB();
            bool worked = false;
            if (allUsers.AddUser(name, password))
                worked = userDB.AddStudent(new UserInfo { Username = name, Password = password });
            return worked;

        }
        public bool CheckUserExist(string username)
        {
            AllUsers allUsers = new AllUsers();
            return allUsers.Any(x => x.Username == username);
        }
        public bool CheckUserPassword(string username, string password)
        {
            AllUsers allUsers = new AllUsers();
            return allUsers.Any(x => x.Username == username && x.Password == password);
        }
        public UserInfo GetUserById(int id)
        {
            ViewDB.UserDB userDB = new ViewDB.UserDB();
            UserInfo user = userDB.GetUserById(id);
            return user;
        }

        public AllUsers GetAllUsers()
        {
            ViewDB.UserDB userDB = new ViewDB.UserDB();
            AllUsers allUsers = userDB.GetAllStudents();
            return allUsers;
        }


    }
}
