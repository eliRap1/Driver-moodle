using System;
using Model;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
namespace Model
{
    [CollectionDataContract]
    public class AllUsers : List<UserInfo>
    {
        public AllUsers() { }
        public AllUsers(IEnumerable<Base> List) : base(List.Cast<UserInfo>().ToList()) { }
        public UserInfo AddUser(string name, string password, string email, string phone, bool admin, int tID)
        {
            if (this.Any(x => x.Username == name))
                return null;
            if(admin)
                tID = 0;
            this.Add(new UserInfo { Username = name, Password = password, Email = email, Phone = phone, IsAdmin = admin, TeacherId = tID});

            UserInfo user = (UserInfo)this.LastOrDefault(x => x.Username == name && x.Password == password && x.Email == email && x.Phone == phone && x.IsAdmin == admin && x.TeacherId == tID);
            
            return user;
        }
        public void SetStudentId(string name,int studentId)
        {
            var student = this.FirstOrDefault(x => x.Username == name);
            if (student != null)
                student.StudentId = studentId;
        }
        public UserInfo GetUser(string name)
        {
            return this.FirstOrDefault(x => x.Username == name);
        }
        public List<UserInfo> GetAllUsers()
        {
            return this.ToList();
        }
    }
}
