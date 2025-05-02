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
        public UserInfo AddUser(string name, string password, string email, string phone, bool admin)
        {
            if (this.Any(x => x.Username == name))
                return null;

            this.Add(new UserInfo { Username = name, Password = password, Email = email, Phone = phone, IsAdmin = admin});
            UserInfo user = (UserInfo)this.LastOrDefault(x => x.Username == name && x.Password == password && x.Email == email && x.Phone == phone && x.IsAdmin == admin);
            
            return user;
        }
    }
}
