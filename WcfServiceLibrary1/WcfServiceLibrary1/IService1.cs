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
    }
}