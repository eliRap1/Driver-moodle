using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [CollectionDataContract]
    public class StudentsList:List<Student>
    {
        public StudentsList() { }
        public StudentsList(IEnumerable<Base> List): base(List.Cast<Student>().ToList()) { }
      
    }
}
