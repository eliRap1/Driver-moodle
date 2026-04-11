using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [DataContract]
    [KnownType(typeof(Course))]
    [KnownType(typeof(CourseModule))]
    [KnownType(typeof(Notification))]
    [KnownType(typeof(StudentModuleProgress))]
    public abstract class Base
    {
        [DataMember]
        public int Id { get; set; }
    }
}