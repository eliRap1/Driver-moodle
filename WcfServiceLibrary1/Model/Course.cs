using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// Represents a learning course in the driving school system.
    /// A course contains multiple modules (lessons, videos, quizzes).
    /// </summary>
    [DataContract]
    public class Course : Base
    {
        [DataMember]
        public string CourseName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int DisplayOrder { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public int TotalModules { get; set; }

        public Course()
        {
            IsActive = true;
            CreatedDate = DateTime.Now;
            DisplayOrder = 0;
        }
    }

    /// <summary>
    /// Collection class for courses, used for WCF serialization
    /// </summary>
    [CollectionDataContract]
    public class CourseList : List<Course>
    {
        public CourseList() { }

        public CourseList(IEnumerable<Base> list)
            : base(list.Cast<Course>().ToList()) { }
    }
}
