using System;
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

    }
}
