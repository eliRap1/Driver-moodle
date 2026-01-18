using BusinessLogic;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using ViewDB;
using Model.Helpers;

namespace WcfServiceLibrary1
{
    public class Service1 : IService1
    {
        private ViewDB.UserDB userDB = new ViewDB.UserDB();
        private AllUsers allUsers = new AllUsers();
        private ChatDB chatDB = new ChatDB();
        private LessonsDB lessonsDB = new LessonsDB();
        private ViewDB.CalendarDB calendarDB = new ViewDB.CalendarDB();

        public bool AddUser(string name, string password, string email, string phone, bool admin, int tID)
        {
            // Validate all inputs before processing
            if (!SecurityHelper.IsSafeString(name, 50) ||
                !SecurityHelper.IsSafeString(email, 100) ||
                string.IsNullOrEmpty(password))
            {
                return false;
            }

            bool worked = false;
            UserInfo user = new UserInfo
            {
                Username = name,
                Password = password, // Will be hashed in UserDB.AddUser/AddStudent
                Email = email,
                Phone = phone,
                IsAdmin = admin,
                TeacherId = tID
            };

            // Check if user already exists
            if (CheckUserExist(name))
            {
                return false;
            }

            if (admin)
            {
                worked = userDB.AddUser(user);
            }
            else
            {
                worked = userDB.AddStudent(user);
                if (worked)
                {
                    int sid = userDB.GetUserID(name, "Student");
                    allUsers.SetStudentId(name, sid);
                }
            }

            return worked;
        }

        public void CancelLesson(int lessonId)
        {
            lessonsDB.CancelLesson(lessonId);
        }

        public void MarkLessonPaid(int id)
        {
            lessonsDB.MarkLessonPaid(id);
        }

        public void AddLessonForStudent(int sid, string Date, string time)
        {
            lessonsDB.AddLessonForStudent(sid, Date, time);
        }

        public List<Lessons> GetAllStudentLessons(int id)
        {
            return lessonsDB.GetAllStudentLessons(id);
        }

        public List<Lessons> GetAllTeacherLessons(int tid)
        {
            return lessonsDB.GetAllTeacherLessons(tid);
        }

        public void UpdateRating(int tid, int rating, string rewiew)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

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

            if (!SecurityHelper.IsSafeString(username, 50))
                return false;

            allUsers = userDB.GetAllStudents();
            var allAdmins = userDB.GetAllTeacher();
            
            return allUsers.Any(x => x.Username == username) || 
                   allAdmins.Any(x => x.Username == username);
        }

        /// <summary>
        /// SECURE: Now uses hash verification instead of plain text comparison
        /// </summary>
        public bool CheckUserPassword(string username, string password)
        {
            if (!SecurityHelper.IsSafeString(username, 50) || 
                string.IsNullOrEmpty(password))
            {
                return false;
            }

            // Verify password using hash comparison
            return userDB.VerifyUserPassword(username, password);
        }

        public UserInfo GetUserById(int id, string table)
        {
            // Validation is done in UserDB
            return userDB.GetUserById(id, table);
        }

        public bool CheckUserAdmin(string username)
        {
            if (!SecurityHelper.IsSafeString(username, 50))
                return false;

            var allAdmins = userDB.GetAllTeacher();
            return allAdmins.Any(x => x.Username == username);
        }

        public AllUsers GetAllUsers()
        {
            return userDB.GetAllStudents();
        }

        public int GetUserID(string username, string table)
        {
            return userDB.GetUserID(username, table);
        }

        public bool SetTeacherCalendar(Calendars cal, int teacherId)
        {
            return calendarDB.SetTeacherCalendar(cal, teacherId);
        }

        public Calendars GetTeacherCalendar(int teacherId)
        {
            return calendarDB.GetTeacherCalendar(teacherId);
        }

        public int GetTeacherId(int studentId)
        {
            return userDB.GetTeacherId(studentId);
        }

        public List<Chats> GetAllChatGlobal()
        {
            return chatDB.GetAllChatGlobal();
        }

        public void AddMessageGlobal(string message, int userid, string username, bool IsTeacher)
        {
            chatDB.AddMessageGlobal(message, userid, username, IsTeacher);
        }

        public List<Chats> GetChatPrivate(int studentid, int teacherid)
        {
            return chatDB.GetChatPrivate(studentid, teacherid);
        }

        public void AddMessagePrivate(string message, int studentid, int teacherid, string username)
        {
            chatDB.AddMessagePrivate(message, studentid, teacherid, username);
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
            return calendarDB.GetTeacherUnavailableDates(teacherId);
        }

        public List<Calendars> TeacherSpacialDays(int teacherId)
        {
            return calendarDB.TeacherSpacialDays(teacherId);
        }
        public void MigrateAllPasswords()
        {
            userDB.MigrateAllPasswords();
        }
    }
}