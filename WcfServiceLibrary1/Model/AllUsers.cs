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
        public bool AddUser(string name, string password)
        {
            if (this.Any(x => x.Username == name))
                return false;

            this.Add(new UserInfo { Username = name, Password = password });
            UserInfo user = (UserInfo)this.LastOrDefault(x => x.Username == name && x.Password == password);
            
            return true;
        }
    }
}
