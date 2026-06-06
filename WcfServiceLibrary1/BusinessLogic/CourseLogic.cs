using Model;
using System.Collections.Generic;
using ViewDB;

namespace BusinessLogic
{
    /// <summary>
    /// Business logic for the learning system: courses, modules, student progress,
    /// and course/module management.
    /// </summary>
    public static class CourseLogic
    {
        // ==================== COURSE/LEARNING OPERATIONS ====================

        public static List<Course> GetAllCourses()
        {
            return new CourseDB().GetActiveCourses();
        }

        public static List<CourseModule> GetCourseModules(int courseId)
        {
            return new CourseModuleDB().GetModulesForCourse(courseId);
        }

        public static List<StudentCourseProgress> GetStudentCourseProgress(int studentId)
        {
            return new CourseModuleDB().GetStudentCourseProgress(studentId);
        }

        public static bool MarkModuleComplete(int studentId, int moduleId)
        {
            return new CourseModuleDB().MarkModuleComplete(studentId, moduleId);
        }

        public static List<StudentModuleProgress> GetStudentCompletedModules(int studentId)
        {
            return new CourseModuleDB().GetStudentCompletedModules(studentId);
        }

        // ==================== COURSE MANAGEMENT OPERATIONS ====================

        public static int AddCourse(Course course)
        {
            return new CourseDB().AddCourse(course);
        }

        public static int UpdateCourse(Course course)
        {
            return new CourseDB().UpdateCourse(course);
        }

        public static int DeactivateCourse(int courseId)
        {
            return new CourseDB().DeactivateCourse(courseId);
        }

        public static int AddModule(CourseModule module)
        {
            return new CourseModuleDB().AddModule(module);
        }

        public static int UpdateModule(CourseModule module)
        {
            return new CourseModuleDB().UpdateModule(module);
        }

        public static int DeleteModule(int moduleId)
        {
            return new CourseModuleDB().DeleteModule(moduleId);
        }

        public static Course GetCourseById(int courseId)
        {
            return new CourseDB().GetCourseById(courseId);
        }
    }
}
