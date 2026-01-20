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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        void CancelLesson(int lessonId);
        [OperationContract]
        int GetUserID(string username, string table);
        [OperationContract]
        UserInfo GetUserById(int id,string table);
        [OperationContract]
        AllUsers GetAllUsers();
        [OperationContract]
        bool CheckUserPassword(string username, string password);
        [OperationContract]
        bool CheckUserAdmin(string username);
        [OperationContract]
        bool CheckUserExist(string username);
        [OperationContract]
        Calendars GetTeacherCalendar(int teacherId);
        [OperationContract]
        bool SetTeacherCalendar(Calendars cal, int teacherId);
        [OperationContract]
        void TeacherConfirm(int id, int tid);
        [OperationContract]
        AllUsers GetAllTeacher();
        [OperationContract]
        List<string> GetTeacherReviews(int tid);
        [OperationContract]
        List<Lessons> GetAllStudentLessons(int id);
        [OperationContract]
        List<Lessons> GetAllTeacherLessons(int tid);
        [OperationContract]
        void AddLessonForStudent(int sid, string Date, string time);
        [OperationContract]
        void UpdateRating(int tid, int rating, string rewiew);
        [OperationContract]
        bool IsConfirmed(int id);
        [OperationContract]
        List<UserInfo> GetTeacherStudents(int tid);
        [OperationContract]
        int GetTeacherId(int sid);
        [OperationContract]
        void UpdateTeacherId(int sid, int tid);
        [OperationContract]
        void MarkLessonPaid(int id);
        [OperationContract]
        void AddMessageGlobal(string message, int userid, string username, bool IsTeacher);
        [OperationContract]
        List<Chats> GetAllChatGlobal();
        [OperationContract]
        List<Chats> GetChatPrivate(int studentid, int teacherid);
        [OperationContract]
        void AddMessagePrivate(string message, int studentid, int teacherid, string username);
        [OperationContract]
        bool CheckPaid(int id);
        [OperationContract]
        void Pay(Payment payment);
        [OperationContract]
        List<Payment> SelectPaymentByPaymentID(int id);
        [OperationContract]
        List<Payment> SelectPaymentByTeacherID(int id);
        [OperationContract]
        List<Calendars> TeacherSpacialDays(int teacherId);
        [OperationContract]
        List<Calendars> GetTeacherUnavailableDates(int teacherId);
        [OperationContract]
        bool AddUser(string name, string password, string email, string phone, bool admin, int tID);
        [OperationContract]
        void MigrateAllPasswords();
        [OperationContract]
        List<Payment> SelectPaymentByStudentID(int id);

        // Support Tickets
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

        // Enhanced Payments
        [OperationContract]
        decimal GetTeacherIncome(int teacherId, DateTime fromDate, DateTime toDate);

        [OperationContract]
        List<Payment> GetOutstandingPayments(int studentId);

        [OperationContract]
        List<Payment> GetOverduePayments();

    }


}
