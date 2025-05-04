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
        UserInfo GetUserById(int id,string table);
        [OperationContract]
        AllUsers GetAllUsers();
        [OperationContract]
        bool CheckUserPassword(string username, string password);
        [OperationContract]
        bool CheckUserExist(string username);

        [OperationContract]
        bool AddUser(string name, string password, string email, string phone, bool admin,string teacherId = "");
    }


}
