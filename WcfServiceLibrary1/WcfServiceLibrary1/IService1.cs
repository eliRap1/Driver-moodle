using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfServiceLibrary1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {

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
        void UpdateRating(int tid, int rating, string rewiew);
        [OperationContract]
        bool IsConfirmed(int id);
        [OperationContract]
        List<UserInfo> GetTeacherStudents(int tid);
        [OperationContract]
        void UpdateTeacherId(int sid, int tid);
        [OperationContract]
        void SetCalendars(List<string> AVailableDays, string startDate, string endDate, List<string> datesUnavailable, Calendars calendars);
        [OperationContract]
        bool AddUser(string name, string password, string email, string phone, bool admin, int tID);
    }


}
