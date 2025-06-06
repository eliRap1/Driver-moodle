using Model;
using BusinessLogic;
using ViewDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WcfServiceLibrary1.ContractTypes;

namespace WcfServiceLibrary1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class Service1 : IService1
    {
        private ViewDB.UserDB userDB = new ViewDB.UserDB();
        private AllUsers allUsers = new AllUsers();
        private AllUsers allAdmins = new AllUsers();
        public bool AddUser(string name, string password,string email, string phone, bool admin,int tID)
        {
            bool worked = false;
            UserInfo user = allUsers.AddUser(name, password, email, phone, admin,tID);
            if (user != null)
            {
                if (user.IsAdmin)
                    worked = userDB.AddUser(user);
                else
                {
                    worked = userDB.AddStudent(user);

                }
                    
            }
            return worked;

        }
        public void UpdateRating(int tid, int rating, string rewiew)
        {
            userDB.UpdateRating(tid, rating, rewiew);
        }
        public List<string> GetTeacherReviews(int tid)
        {
            return userDB.GetTeacherReviews(tid);
        }
        public void UpdateTeacherId(int sid, int tid)
        {
            userDB.UpdateTeacherId(sid, tid);
        }
        public AllUsers GetAllTeacher()
        {
            return userDB.GetAllTeacher();
        }
        public void TeacherConfirm(int id, int tID)
        {
            userDB.TeacherConfirm(id, tID);
        }
        public List<UserInfo> GetTeacherStudents(int tid)
        {
            return userDB.GetTeacherStudents(tid);
        }
        public bool IsConfirmed(int id)
        {
            return userDB.IsConfirmed(id);
        }
        public bool CheckUserExist(string username)
        {
            allUsers = userDB.GetAllStudents();
            allAdmins = userDB.GetAllTeacher();
            return allUsers.Any(x => x.Username == username) || allAdmins.Any(x => x.Username == username);
        }
        public bool CheckUserPassword(string username, string password)
        {
            allUsers = userDB.GetAllStudents();
            allAdmins = userDB.GetAllTeacher();
            return allUsers.Any(x => x.Username == username && x.Password == password) || allAdmins.Any(x => x.Username == username && x.Password == password);
        }
        public UserInfo GetUserById(int id, string table)
        {
            UserInfo user = userDB.GetUserById(id, table);
            return user;
        }
        public bool CheckUserAdmin(string username)
        {
            allAdmins = userDB.GetAllTeacher();
            return allAdmins.Any(x => x.Username == username);
        }
        public AllUsers GetAllUsers()
        {
            AllUsers allUsers = userDB.GetAllStudents();
            return allUsers;
        }
        public int GetUserID(string username, string table)
        {
            return userDB.GetUserID(username, table);
        }
        public bool SetTeacherCalendar(Calendars cal, int teacherId)
        {
            return userDB.SetTeacherCalendar(cal, teacherId);
        }
        public Calendars GetTeacherCalendar(int teacherId)
        {
            return userDB.GetTeacherCalendar(teacherId);
        }
        public void SetCalendars(List<string> AVailableDays, string startDate, string endDate, List<string> datesUnavailable, Calendars calendars)
        {
            calendars.SetCalendars(AVailableDays, startDate, endDate, datesUnavailable);
        }
    }
}
