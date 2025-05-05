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
        public bool AddUser(string name, string password,string email, string phone, bool admin,int tID)
        {
            AllUsers allUsers = new AllUsers();
            ViewDB.UserDB userDB = new ViewDB.UserDB();
            bool worked = false;
            UserInfo user = allUsers.AddUser(name, password, email, phone, admin,tID);
            if (user != null)
            {
                if (user.IsAdmin)
                    worked = userDB.AddUser(user);
                else
                    worked = userDB.AddStudent(user);
            }
            return worked;

        }
        //public bool AddStudent(string name, string password, string email, string phone, bool admin, int TeacherId)
        //{
        //    AllUsers allUsers = new AllUsers();
        //    ViewDB.UserDB userDB = new ViewDB.UserDB();
        //    bool worked = false;
        //    UserInfo user = allUsers.AddStudent(name, password, email, phone, admin);
        //    if (user != null)
        //        worked = userDB.AddStudent(user);
        //    return worked;
        //}
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
        public UserInfo GetUserById(int id, string table)
        {
            ViewDB.UserDB userDB = new ViewDB.UserDB();
            UserInfo user = userDB.GetUserById(id, table);
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
