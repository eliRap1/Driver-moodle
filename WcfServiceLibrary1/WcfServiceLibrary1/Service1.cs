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
        private ViewDB.UserDB userDB = null;// new ViewDB.UserDB();
        private AllUsers allUsers = null;//new AllUsers();
        private AllUsers allAdmins = null;//new AllUsers();
        private ChatDB chatDB = null;//new ChatDB();
        private LessonsDB lessonsDB = null;//new LessonsDB();
        private ViewDB.CalnderDB calnderDB = null;//new ViewDB.CalnderDB();
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
            return new ViewDB.UserDB().SetTeacherCalendar(cal, teacherId);
        }
        public Calendars GetTeacherCalendar(int teacherId)
        {
            return new ViewDB.CalnderDB().GetTeacherCalendar(teacherId);
        }
        public int GetTeacherId(int studentId)
        {
            return new ViewDB.UserDB().GetTeacherId(studentId);
        }
        public List<Chats> GetAllChat()
        {
            return new ChatDB().GetAllChat();
        }
        public void AddMessage(string message, int userid, string username, bool IsTeacher)
        {
            new ChatDB().AddMessage(message, userid, username, IsTeacher);
        }
    }
}
