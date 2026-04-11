using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// Tracks a student's progress on a specific module
    /// </summary>
    [DataContract]
    public class StudentModuleProgress : Base
    {
        [DataMember]
        public int ProgressId { get; set; }

        [DataMember]
        public int StudentId { get; set; }

        [DataMember]
        public int ModuleId { get; set; }

        [DataMember]
        public int CourseId { get; set; }

        [DataMember]
        public bool IsCompleted { get; set; }

        [DataMember]
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Progress percentage for this module (0-100)
        /// </summary>
        [DataMember]
        public int ProgressPercent { get; set; }

        public StudentModuleProgress()
        {
            IsCompleted = false;
            ProgressPercent = 0;
        }
    }

    /// <summary>
    /// Aggregated progress for a student on an entire course
    /// </summary>
    [DataContract]
    public class StudentCourseProgress
    {
        [DataMember]
        public int StudentId { get; set; }

        [DataMember]
        public int CourseId { get; set; }

        [DataMember]
        public string CourseName { get; set; }

        [DataMember]
        public string CourseDescription { get; set; }

        [DataMember]
        public int TotalModules { get; set; }

        [DataMember]
        public int CompletedModules { get; set; }

        /// <summary>
        /// Overall course completion percentage
        /// </summary>
        [DataMember]
        public int ProgressPercent { get; set; }

        [DataMember]
        public List<StudentModuleProgress> ModuleProgress { get; set; }

        public StudentCourseProgress()
        {
            ModuleProgress = new List<StudentModuleProgress>();
        }
    }
}
