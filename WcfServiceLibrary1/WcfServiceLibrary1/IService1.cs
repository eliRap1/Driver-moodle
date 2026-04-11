using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ViewDB;

namespace WcfServiceLibrary1
{
    [ServiceContract]
    public interface IService1
    {
        // ==================== LESSON OPERATIONS ====================
        [OperationContract]
        void CancelLesson(int lessonId);

        [OperationContract]
        List<Lessons> GetAllStudentLessons(int id);

        [OperationContract]
        List<Lessons> GetAllTeacherLessons(int tid);

        [OperationContract]
        void AddLessonForStudent(int sid, string Date, string time);

        [OperationContract]
        void MarkLessonPaid(int id);

        // ==================== USER OPERATIONS ====================
        [OperationContract]
        int GetUserID(string username, string table);

        [OperationContract]
        UserInfo GetUserById(int id, string table);

        [OperationContract]
        AllUsers GetAllUsers();

        [OperationContract]
        AllUsers GetAllTeacher();

        [OperationContract]
        bool CheckUserPassword(string username, string password);

        [OperationContract]
        bool CheckUserAdmin(string username);

        [OperationContract]
        bool CheckUserExist(string username);

        [OperationContract]
        bool AddUser(string name, string password, string email, string phone, bool admin, int tID, int lessonPrice = 200);

        [OperationContract]
        void TeacherConfirm(int id, int tid);

        [OperationContract]
        bool IsConfirmed(int id);

        [OperationContract]
        List<UserInfo> GetTeacherStudents(int tid);

        [OperationContract]
        int GetTeacherId(int sid);

        [OperationContract]
        void UpdateTeacherId(int sid, int tid);

        // ==================== ADMIN OPERATIONS ====================
        [OperationContract]
        bool IsUserAdmin(string username);

        [OperationContract]
        void SetAdminStatus(int teacherId, bool isAdmin);

        [OperationContract]
        void UpdateStudentCredentials(int studentId, string email, string phone, int teacherId);

        [OperationContract]
        void UpdateStudentTeacher(int studentId, int newTeacherId);

        [OperationContract]
        void ResetPassword(int userId, string table, string newPassword);

        // ==================== PRICING OPERATIONS ====================
        [OperationContract]
        void UpdateLessonPrice(int teacherId, int price);

        [OperationContract]
        void SetStudentLessonPrice(int studentId, int price);

        [OperationContract]
        int GetStudentLessonPrice(int studentId);

        [OperationContract]
        void UpdatePaymentMethods(int teacherId, string paymentMethods);

        // ==================== CALENDAR OPERATIONS ====================
        [OperationContract]
        Calendars GetTeacherCalendar(int teacherId);

        [OperationContract]
        bool SetTeacherCalendar(Calendars cal, int teacherId);

        [OperationContract]
        List<Calendars> TeacherSpacialDays(int teacherId);

        [OperationContract]
        List<Calendars> GetTeacherUnavailableDates(int teacherId);

        // ==================== RATING OPERATIONS ====================
        [OperationContract]
        void UpdateRating(int tid, int rating, string rewiew);

        [OperationContract]
        List<string> GetTeacherReviews(int tid);

        // ==================== CHAT OPERATIONS ====================
        [OperationContract]
        void AddMessageGlobal(string message, int userid, string username, bool IsTeacher);

        [OperationContract]
        List<Chats> GetAllChatGlobal();

        [OperationContract]
        List<Chats> GetChatPrivate(int studentid, int teacherid);

        [OperationContract]
        void AddMessagePrivate(string message, int studentid, int teacherid, string username);

        // ==================== PAYMENT OPERATIONS ====================
        [OperationContract]
        bool CheckPaid(int id);

        [OperationContract]
        void Pay(Payment payment);

        [OperationContract]
        List<Payment> SelectPaymentByPaymentID(int id);

        [OperationContract]
        List<Payment> SelectPaymentByTeacherID(int id);

        [OperationContract]
        List<Payment> SelectPaymentByStudentID(int id);

        [OperationContract]
        decimal GetTeacherIncome(int teacherId, DateTime fromDate, DateTime toDate);

        [OperationContract]
        List<Payment> GetOutstandingPayments(int studentId);

        [OperationContract]
        List<Payment> GetOverduePayments();

        // ==================== SUPPORT TICKET OPERATIONS ====================
        [OperationContract]
        int CreateSupportTicket(SupportTicket ticket);

        [OperationContract]
        List<SupportTicket> GetUserTickets(int userId);

        [OperationContract]
        List<SupportTicket> GetAllTickets();

        [OperationContract]
        SupportTicket GetTicketById(int ticketId);

        [OperationContract]
        void UpdateTicketStatus(int ticketId, string status, string assignedTo);

        [OperationContract]
        void CloseTicket(int ticketId, string resolution, string adminNotes);

        [OperationContract]
        void AddTicketMessage(TicketMessage message);

        [OperationContract]
        List<TicketMessage> GetTicketMessages(int ticketId);

        [OperationContract]
        void UpdateTicketPriority(int ticketId, string priority);

        // ==================== MIGRATION ====================
        [OperationContract]
        void MigrateAllPasswords();

        // ==================== STUDENT PRICING OPERATIONS  ====================

        [OperationContract]
        void SetStudentDiscount(int studentId, int discountPercent);

        [OperationContract]
        int GetEffectiveLessonPrice(int studentId);

        // ==================== COURSE/LEARNING OPERATIONS ====================

        /// <summary>
        /// Get all available courses
        /// </summary>
        [OperationContract]
        List<Course> GetAllCourses();

        /// <summary>
        /// Get all modules for a specific course
        /// </summary>
        [OperationContract]
        List<CourseModule> GetCourseModules(int courseId);

        /// <summary>
        /// Get a student's progress across all courses
        /// </summary>
        [OperationContract]
        List<StudentCourseProgress> GetStudentCourseProgress(int studentId);

        /// <summary>
        /// Mark a module as complete for a student
        /// </summary>
        [OperationContract]
        bool MarkModuleComplete(int studentId, int moduleId);

        /// <summary>
        /// Get all completed modules for a student
        /// </summary>
        [OperationContract]
        List<StudentModuleProgress> GetStudentCompletedModules(int studentId);

        // ==================== COURSE MANAGEMENT OPERATIONS ====================

        [OperationContract]
        int AddCourse(Course course);

        [OperationContract]
        int UpdateCourse(Course course);

        [OperationContract]
        int DeactivateCourse(int courseId);

        [OperationContract]
        int AddModule(CourseModule module);

        [OperationContract]
        int UpdateModule(CourseModule module);

        [OperationContract]
        int DeleteModule(int moduleId);

        [OperationContract]
        Course GetCourseById(int courseId);

        // ==================== NOTIFICATION OPERATIONS ====================

        /// <summary>
        /// Send a notification to a user
        /// </summary>
        [OperationContract]
        int SendNotification(Notification notification);

        /// <summary>
        /// Get all notifications for a user
        /// </summary>
        [OperationContract]
        List<Notification> GetUserNotifications(int userId, string userType);

        /// <summary>
        /// Get unread notification count
        /// </summary>
        [OperationContract]
        int GetUnreadNotificationCount(int userId, string userType);

        /// <summary>
        /// Get unread notifications
        /// </summary>
        [OperationContract]
        List<Notification> GetUnreadNotifications(int userId, string userType);

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        [OperationContract]
        void MarkNotificationAsRead(int notificationId);

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [OperationContract]
        void MarkAllNotificationsAsRead(int userId, string userType);

        /// <summary>
        /// Delete a notification
        /// </summary>
        [OperationContract]
        void DeleteNotification(int notificationId);

        /// <summary>
        /// Send a message from teacher to student
        /// </summary>
        [OperationContract]
        void SendTeacherMessage(int teacherId, string teacherName, int studentId, string title, string message);

        /// <summary>
        /// Send a message from student to teacher
        /// </summary>
        [OperationContract]
        void SendStudentMessage(int studentId, string studentName, int teacherId, string title, string message);

        /// <summary>
        /// Send lesson cancelled notification to teacher
        /// </summary>
        [OperationContract]
        void SendLessonCancelledNotification(int studentId, string studentName, int teacherId, string lessonDate, string lessonTime);
    }
}