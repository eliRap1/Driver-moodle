using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// Represents a single learning module within a course.
    /// Modules can be theory lessons, videos, quizzes, or assignments.
    /// </summary>
    [DataContract]
    public class CourseModule : Base
    {
        [DataMember]
        public int ModuleId { get; set; }

        [DataMember]
        public int CourseId { get; set; }

        [DataMember]
        public string ModuleName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int OrderIndex { get; set; }

        /// <summary>
        /// Type of content: "Video", "Text", "Quiz", "Assignment"
        /// </summary>
        [DataMember]
        public string ContentType { get; set; }

        /// <summary>
        /// URL or path to the content resource
        /// </summary>
        [DataMember]
        public string ContentUrl { get; set; }

        /// <summary>
        /// Estimated duration in minutes
        /// </summary>
        [DataMember]
        public int DurationMinutes { get; set; }

        /// <summary>
        /// Whether this module must be completed to finish the course
        /// </summary>
        [DataMember]
        public bool IsRequired { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }

        public CourseModule()
        {
            CreatedAt = DateTime.Now;
            IsRequired = true;
            ContentType = "Text";
        }
    }

    /// <summary>
    /// Collection class for course modules, used for WCF serialization
    /// </summary>
    [CollectionDataContract]
    public class CourseModuleList : List<CourseModule>
    {
        public CourseModuleList() { }

        public CourseModuleList(IEnumerable<Base> list)
            : base(list.Cast<CourseModule>().ToList()) { }
    }

}
