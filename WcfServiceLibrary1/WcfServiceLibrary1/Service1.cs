using BusinessLogic;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using ViewDB;
using WcfServiceLibrary1.ContractTypes;

namespace WcfServiceLibrary1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class Service1 : IService1
    {
        private ViewDB.UserDB userDB = null;// new ViewDB.UserDB();
        private AllUsers allUsers = null;//new AllUsers();
        private AllUsers allAdmins = null;//new AllUsers();
        private ChatDB chatDB = null;//new ChatDB();
        private LessonsDB lessonsDB = null;//new LessonsDB();
        private ViewDB.CalendarDB calnderDB = null;//new ViewDB.CalnderDB();
        public bool AddUser(string name, string password, string email, string phone, bool admin, int tID)
        {
            bool worked = false;


            //TEST. should be created and fill in client by binding
            UserInfo user = new UserInfo();
            user.Username = name;
            user.Password = password;
            user.Email = email;
            //worked = new ViewDB.UserDB().AddUser(user);

            user = allUsers.AddUser(name, password, email, phone, admin, tID);
            if (user != null)
            {
                if (user.IsAdmin)
                    worked = new ViewDB.UserDB().AddUser(user);
                else
                {
                    worked = new ViewDB.UserDB().AddStudent(user);

                }

            }
            return worked;

        }
        //public List<Lessons> GetAllTeacherLessonsForDate(int tid, string date)
        //{
        //    return new LessonsDB().GetAllTeacherLessonsForDate(tid, date);
        //}
        public void MarkLessonPaid(int id)
        {
            new LessonsDB().MarkLessonPaid(id);
        }
        public void AddLessonForStudent(int sid, string Date, string time)
        {
            new LessonsDB().AddLessonForStudent(sid, Date, time);
        }
        public List<Lessons> GetAllStudentLessons(int id)
        {
            return new LessonsDB().GetAllStudentLessons(id);
        }
        public List<Lessons> GetAllTeacherLessons(int tid)
        {
            return new LessonsDB().GetAllTeacherLessons(tid);
        }
    public void UpdateRating(int tid, int rating, string rewiew)
        {
            new ViewDB.UserDB().UpdateRating(tid, rating, rewiew);
        }
        public List<string> GetTeacherReviews(int tid)
        {
            return new ViewDB.UserDB().GetTeacherReviews(tid);
        }
        public void UpdateTeacherId(int sid, int tid)
        {
            new ViewDB.UserDB().UpdateTeacherId(sid, tid);
        }
        public AllUsers GetAllTeacher()
        {
            return new ViewDB.UserDB().GetAllTeacher();
        }
        public void TeacherConfirm(int id, int tID)
        {
            new ViewDB.UserDB().TeacherConfirm(id, tID);
        }
        public List<UserInfo> GetTeacherStudents(int tid)
        {
            return new ViewDB.UserDB().GetTeacherStudents(tid);
        }
        public bool IsConfirmed(int id)
        {
            return new ViewDB.UserDB().IsConfirmed(id);
        }
        public bool CheckUserExist(string username)
        {
            allUsers = new ViewDB.UserDB().GetAllStudents();
            allAdmins = new ViewDB.UserDB().GetAllTeacher();
            return allUsers.Any(x => x.Username == username) || allAdmins.Any(x => x.Username == username);
        }
        public bool CheckUserPassword(string username, string password)
        {
            allUsers = new ViewDB.UserDB().GetAllStudents();
            allAdmins = new ViewDB.UserDB().GetAllTeacher();
            return allUsers.Any(x => x.Username == username && x.Password == password) || allAdmins.Any(x => x.Username == username && x.Password == password);
        }
        public UserInfo GetUserById(int id, string table)
        {
            UserInfo user = new ViewDB.UserDB().GetUserById(id, table);
            return user;
        }
        public bool CheckUserAdmin(string username)
        {
            allAdmins = new ViewDB.UserDB().GetAllTeacher();
            return allAdmins.Any(x => x.Username == username);
        }
        public AllUsers GetAllUsers()
        {
            AllUsers allUsers = new ViewDB.UserDB().GetAllStudents();
            return allUsers;
        }
        public int GetUserID(string username, string table)
        {
            return new ViewDB.UserDB().GetUserID(username, table);
        }
        public bool SetTeacherCalendar(Calendars cal, int teacherId)
        {
            return new ViewDB.CalendarDB().SetTeacherCalendar(cal, teacherId);
        }
        public Calendars GetTeacherCalendar(int teacherId)
        {
            return new ViewDB.CalendarDB().GetTeacherCalendar(teacherId);
        }
        public int GetTeacherId(int studentId)
        {
            return new ViewDB.UserDB().GetTeacherId(studentId);
        }
        public List<Chats> GetAllChatGlobal()
        {
            return new ChatDB().GetAllChatGlobal();
        }
        public void AddMessageGlobal(string message, int userid, string username, bool IsTeacher)
        {
            new ChatDB().AddMessageGlobal(message, userid, username, IsTeacher);
        }
        public List<Chats> GetChatPrivate(int studentid, int teacherid)
        {
            return new ChatDB().GetChatPrivate(studentid, teacherid);
        }
        public void AddMessagePrivate(string message, int studentid, int teacherid,string username)
        {
            new ChatDB().AddMessagePrivate(message, studentid, teacherid, username);
        }

        public List<Payment> SelectPaymentByStudentID(int id)
        {
            return new PaymentDB().SelectPaymentByStudentID(id);
        }
        public List<Payment> SelectPaymentByTeacherID(int id)
        {
            return new PaymentDB().SelectPaymentByTeacherID(id);
        }
        public List<Payment> SelectPaymentByPaymentID(int id)
        {
            return new PaymentDB().SelectPaymentByPaymentID(id);
        }
        public void Pay(Payment payment)
        {
            new PaymentDB().Pay(payment);
        }
        public bool CheckPaid(int id)
        {
            return new PaymentDB().CheckPaid(id);
        }
        public List<Calendars> GetTeacherUnavailableDates(int teacherId)
        {
            return new ViewDB.CalendarDB().GetTeacherUnavailableDates(teacherId);
        }
        public List<Calendars> TeacherSpacialDays(int teacherId)
        {
            return new ViewDB.CalendarDB().TeacherSpacialDays(teacherId);
        }
    }
}
