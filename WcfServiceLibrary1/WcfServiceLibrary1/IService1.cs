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
        string GetStudentLessons(int id);
        [OperationContract]
        void AddLessonToStudent(int id, string less);
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
        void AddMessage(string message, int userid, string username, bool IsTeacher);
        [OperationContract]
        List<Chats> GetAllChat();
        [OperationContract]
        bool AddUser(string name, string password, string email, string phone, bool admin, int tID);
    }


}
