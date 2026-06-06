using BusinessLogic;
using Model;
using System;
using System.Collections.Generic;
using ViewDB;

namespace WcfServiceLibrary1
{
    /// <summary>
    /// WCF service facade. This class only exposes the operations defined in
    /// IService1 to clients; every method delegates to the matching class in the
    /// BusinessLogic layer, which holds the validation, orchestration and rules.
    /// Layering: Service1 (WCF) -> BusinessLogic -> ViewDB (data access) -> Model.
    /// </summary>
    public class Service1 : IService1
    {
        // ==================== USER OPERATIONS ====================

        public bool AddUser(string name, string password, string email, string phone, bool admin, int tID, int lessonPrice = 200)
        {
            return UserLogic.AddUser(name, password, email, phone, admin, tID, lessonPrice);
        }

        public bool CheckUserExist(string username)
        {
            return UserLogic.CheckUserExist(username);
        }

        public bool CheckUserPassword(string username, string password)
        {
            return UserLogic.CheckUserPassword(username, password);
        }

        public UserInfo GetUserById(int id, string table)
        {
            return UserLogic.GetUserById(id, table);
        }

        public bool CheckUserAdmin(string username)
        {
            return UserLogic.CheckUserAdmin(username);
        }

        public bool IsUserAdmin(string username)
        {
            return UserLogic.IsUserAdmin(username);
        }

        public AllUsers GetAllUsers()
        {
            return UserLogic.GetAllUsers();
        }

        public AllUsers GetAllTeacher()
        {
            return UserLogic.GetAllTeacher();
        }

        public int GetUserID(string username, string table)
        {
            return UserLogic.GetUserID(username, table);
        }

        public void TeacherConfirm(int id, int tID)
        {
            UserLogic.TeacherConfirm(id, tID);
        }

        public List<UserInfo> GetTeacherStudents(int tid)
        {
            return UserLogic.GetTeacherStudents(tid);
        }

        public bool IsConfirmed(int id)
        {
            return UserLogic.IsConfirmed(id);
        }

        public int GetTeacherId(int studentId)
        {
            return UserLogic.GetTeacherId(studentId);
        }

        public void UpdateTeacherId(int sid, int tid)
        {
            UserLogic.UpdateTeacherId(sid, tid);
        }

        // ==================== ADMIN OPERATIONS ====================

        public void SetAdminStatus(int teacherId, bool isAdmin)
        {
            UserLogic.SetAdminStatus(teacherId, isAdmin);
        }

        public void ResetPassword(int userId, string table, string newPassword)
        {
            UserLogic.ResetPassword(userId, table, newPassword);
        }

        // ==================== PRICING OPERATIONS ====================

        public void UpdateLessonPrice(int teacherId, int price)
        {
            UserLogic.UpdateLessonPrice(teacherId, price);
        }

        public int GetStudentLessonPrice(int studentId)
        {
            return UserLogic.GetStudentLessonPrice(studentId);
        }

        public void UpdatePaymentMethods(int teacherId, string paymentMethods)
        {
            UserLogic.UpdatePaymentMethods(teacherId, paymentMethods);
        }

        // ==================== LESSON OPERATIONS ====================

        public void CancelLesson(int lessonId)
        {
            LessonLogic.CancelLesson(lessonId);
        }

        public void MarkLessonPaid(int id)
        {
            LessonLogic.MarkLessonPaid(id);
        }

        public void AddLessonForStudent(int sid, string Date, string time)
        {
            LessonLogic.AddLessonForStudent(sid, Date, time);
        }

        public List<Lessons> GetAllStudentLessons(int id)
        {
            return LessonLogic.GetAllStudentLessons(id);
        }

        public List<Lessons> GetAllTeacherLessons(int tid)
        {
            return LessonLogic.GetAllTeacherLessons(tid);
        }

        // ==================== RATING OPERATIONS ====================

        public void UpdateRating(int tid, int rating, string rewiew)
        {
            UserLogic.UpdateRating(tid, rating, rewiew);
        }

        public List<string> GetTeacherReviews(int tid)
        {
            return UserLogic.GetTeacherReviews(tid);
        }

        // ==================== CALENDAR OPERATIONS ====================

        public bool SetTeacherCalendar(Calendars cal, int teacherId)
        {
            return CalendarLogic.SetTeacherCalendar(cal, teacherId);
        }

        public Calendars GetTeacherCalendar(int teacherId)
        {
            return CalendarLogic.GetTeacherCalendar(teacherId);
        }

        public List<Calendars> GetTeacherUnavailableDates(int teacherId)
        {
            return CalendarLogic.GetTeacherUnavailableDates(teacherId);
        }

        public List<Calendars> TeacherSpacialDays(int teacherId)
        {
            return CalendarLogic.TeacherSpacialDays(teacherId);
        }

        // ==================== CHAT OPERATIONS ====================

        public List<Chats> GetAllChatGlobal()
        {
            return ChatLogic.GetAllChatGlobal();
        }

        public void AddMessageGlobal(string message, int userid, string username, bool IsTeacher)
        {
            ChatLogic.AddMessageGlobal(message, userid, username, IsTeacher);
        }

        public List<Chats> GetChatPrivate(int studentid, int teacherid)
        {
            return ChatLogic.GetChatPrivate(studentid, teacherid);
        }

        public void AddMessagePrivate(string message, int studentid, int teacherid, string username)
        {
            ChatLogic.AddMessagePrivate(message, studentid, teacherid, username);
        }

        // ==================== PAYMENT OPERATIONS ====================

        public List<Payment> SelectPaymentByStudentID(int id)
        {
            return PaymentLogic.SelectPaymentByStudentID(id);
        }

        public List<Payment> SelectPaymentByTeacherID(int id)
        {
            return PaymentLogic.SelectPaymentByTeacherID(id);
        }

        public List<Payment> SelectPaymentByPaymentID(int id)
        {
            return PaymentLogic.SelectPaymentByPaymentID(id);
        }

        public void Pay(Payment payment)
        {
            PaymentLogic.Pay(payment);
        }

        public bool CheckPaid(int id)
        {
            return PaymentLogic.CheckPaid(id);
        }

        public decimal GetTeacherIncome(int teacherId, DateTime fromDate, DateTime toDate)
        {
            return PaymentLogic.GetTeacherIncome(teacherId, fromDate, toDate);
        }

        public List<Payment> GetOutstandingPayments(int studentId)
        {
            return PaymentLogic.GetOutstandingPayments(studentId);
        }

        public List<Payment> GetOverduePayments()
        {
            return PaymentLogic.GetOverduePayments();
        }

        // ==================== SUPPORT TICKET OPERATIONS ====================

        public int CreateSupportTicket(SupportTicket ticket)
        {
            return SupportTicketLogic.CreateSupportTicket(ticket);
        }

        public List<SupportTicket> GetUserTickets(int userId)
        {
            return SupportTicketLogic.GetUserTickets(userId);
        }

        public List<SupportTicket> GetAllTickets()
        {
            return SupportTicketLogic.GetAllTickets();
        }

        public SupportTicket GetTicketById(int ticketId)
        {
            return SupportTicketLogic.GetTicketById(ticketId);
        }

        public void UpdateTicketStatus(int ticketId, string status, string assignedTo)
        {
            SupportTicketLogic.UpdateTicketStatus(ticketId, status, assignedTo);
        }

        public void CloseTicket(int ticketId, string resolution, string adminNotes)
        {
            SupportTicketLogic.CloseTicket(ticketId, resolution, adminNotes);
        }

        public void AddTicketMessage(TicketMessage message)
        {
            SupportTicketLogic.AddTicketMessage(message);
        }

        public List<TicketMessage> GetTicketMessages(int ticketId)
        {
            return SupportTicketLogic.GetTicketMessages(ticketId);
        }

        public void UpdateTicketPriority(int ticketId, string priority)
        {
            SupportTicketLogic.UpdateTicketPriority(ticketId, priority);
        }

        // ==================== MIGRATION ====================

        public void MigrateAllPasswords()
        {
            UserLogic.MigrateAllPasswords();
        }

        // ==================== STUDENT PRICING OPERATIONS ====================

        public void SetStudentLessonPrice(int studentId, int price)
        {
            UserLogic.SetStudentLessonPrice(studentId, price);
        }

        public void SetStudentDiscount(int studentId, int discountPercent)
        {
            UserLogic.SetStudentDiscount(studentId, discountPercent);
        }

        public int GetEffectiveLessonPrice(int studentId)
        {
            return UserLogic.GetEffectiveLessonPrice(studentId);
        }

        public void UpdateStudentCredentials(int studentId, string email, string phone, int teacherId)
        {
            UserLogic.UpdateStudentCredentials(studentId, email, phone, teacherId);
        }

        public void UpdateStudentTeacher(int studentId, int newTeacherId)
        {
            UserLogic.UpdateStudentTeacher(studentId, newTeacherId);
        }

        // ==================== COURSE/LEARNING OPERATIONS ====================

        public List<Course> GetAllCourses()
        {
            return CourseLogic.GetAllCourses();
        }

        public List<CourseModule> GetCourseModules(int courseId)
        {
            return CourseLogic.GetCourseModules(courseId);
        }

        public List<StudentCourseProgress> GetStudentCourseProgress(int studentId)
        {
            return CourseLogic.GetStudentCourseProgress(studentId);
        }

        public bool MarkModuleComplete(int studentId, int moduleId)
        {
            return CourseLogic.MarkModuleComplete(studentId, moduleId);
        }

        public List<StudentModuleProgress> GetStudentCompletedModules(int studentId)
        {
            return CourseLogic.GetStudentCompletedModules(studentId);
        }

        // ==================== COURSE MANAGEMENT OPERATIONS ====================

        public int AddCourse(Course course)
        {
            return CourseLogic.AddCourse(course);
        }

        public int UpdateCourse(Course course)
        {
            return CourseLogic.UpdateCourse(course);
        }

        public int DeactivateCourse(int courseId)
        {
            return CourseLogic.DeactivateCourse(courseId);
        }

        public int AddModule(CourseModule module)
        {
            return CourseLogic.AddModule(module);
        }

        public int UpdateModule(CourseModule module)
        {
            return CourseLogic.UpdateModule(module);
        }

        public int DeleteModule(int moduleId)
        {
            return CourseLogic.DeleteModule(moduleId);
        }

        public Course GetCourseById(int courseId)
        {
            return CourseLogic.GetCourseById(courseId);
        }

        // ==================== NOTIFICATION OPERATIONS ====================

        public int SendNotification(Notification notification)
        {
            return NotificationLogic.SendNotification(notification);
        }

        public List<Notification> GetUserNotifications(int userId, string userType)
        {
            return NotificationLogic.GetUserNotifications(userId, userType);
        }

        public int GetUnreadNotificationCount(int userId, string userType)
        {
            return NotificationLogic.GetUnreadNotificationCount(userId, userType);
        }

        public List<Notification> GetUnreadNotifications(int userId, string userType)
        {
            return NotificationLogic.GetUnreadNotifications(userId, userType);
        }

        public void MarkNotificationAsRead(int notificationId)
        {
            NotificationLogic.MarkNotificationAsRead(notificationId);
        }

        public void MarkAllNotificationsAsRead(int userId, string userType)
        {
            NotificationLogic.MarkAllNotificationsAsRead(userId, userType);
        }

        public void DeleteNotification(int notificationId)
        {
            NotificationLogic.DeleteNotification(notificationId);
        }

        public void SendTeacherMessage(int teacherId, string teacherName, int studentId, string title, string message)
        {
            NotificationLogic.SendTeacherMessage(teacherId, teacherName, studentId, title, message);
        }

        public void SendStudentMessage(int studentId, string studentName, int teacherId, string title, string message)
        {
            NotificationLogic.SendStudentMessage(studentId, studentName, teacherId, title, message);
        }

        public void SendLessonCancelledNotification(int studentId, string studentName, int teacherId, string lessonDate, string lessonTime)
        {
            NotificationLogic.SendLessonCancelledNotification(studentId, studentName, teacherId, lessonDate, lessonTime);
        }
    }
}
