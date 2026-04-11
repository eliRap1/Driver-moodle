using Model;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace ViewDB
{
    /// <summary>
    /// Database access class for StudentModuleProgress table.
    /// Tracks which modules a student has completed.
    ///
    /// TABLE SCHEMA (StudentModuleProgress):
    /// - ProgressID (AutoNumber, Primary Key)
    /// - StudentID (Integer, Foreign Key to Student)
    /// - ModuleID (Integer, Foreign Key to CourseModules)
    /// - CourseID (Integer, Foreign Key to Courses)
    /// - IsCompleted (Yes/No)
    /// - CompletedAt (Date/Time, nullable)
    /// - ProgressPercent (Integer) - 0-100
    /// </summary>
    public class StudentCourseProgressDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new StudentModuleProgress();
        }

        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    StudentModuleProgress p = (StudentModuleProgress)entity;
                    p.ProgressId = Convert.ToInt32(reader["ProgressID"]);
                    p.StudentId = Convert.ToInt32(reader["StudentID"]);
                    p.ModuleId = Convert.ToInt32(reader["ModuleID"]);
                    p.CourseId = Convert.ToInt32(reader["CourseID"]);

                    try { p.IsCompleted = bool.Parse(reader["IsCompleted"].ToString()); }
                    catch { p.IsCompleted = false; }

                    try
                    {
                        var completedAt = reader["CompletedAt"];
                        if (completedAt != null && completedAt != DBNull.Value)
                            p.CompletedAt = DateTime.Parse(completedAt.ToString());
                        else
                            p.CompletedAt = null;
                    }
                    catch { p.CompletedAt = null; }

                    try { p.ProgressPercent = Convert.ToInt32(reader["ProgressPercent"]); }
                    catch { p.ProgressPercent = 0; }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("StudentCourseProgressDB CreateModel Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Mark a module as complete for a student
        /// </summary>
        public int MarkModuleComplete(int studentId, int moduleId, int courseId)
        {
            // Check if record already exists
            var existing = GetProgressRecord(studentId, moduleId);

            if (existing != null)
            {
                // Update existing record
                string updateSql = @"UPDATE [StudentModuleProgress] SET
                    [IsCompleted] = ?,
                    [CompletedAt] = ?,
                    [ProgressPercent] = ?
                    WHERE [ProgressID] = ?";

                return SaveChanges(updateSql,
                    new OleDbParameter("@isCompleted", true),
                    new OleDbParameter("@completedAt", OleDbType.Date) { Value = DateTime.Now },
                    new OleDbParameter("@progressPercent", 100),
                    new OleDbParameter("@progressId", existing.ProgressId));
            }
            else
            {
                // Insert new record
                string insertSql = @"INSERT INTO [StudentModuleProgress]
                    ([StudentID], [ModuleID], [CourseID], [IsCompleted], [CompletedAt], [ProgressPercent])
                    VALUES (?, ?, ?, ?, ?, ?)";

                return SaveChanges(insertSql,
                    new OleDbParameter("@studentId", studentId),
                    new OleDbParameter("@moduleId", moduleId),
                    new OleDbParameter("@courseId", courseId),
                    new OleDbParameter("@isCompleted", true),
                    new OleDbParameter("@completedAt", OleDbType.Date) { Value = DateTime.Now },
                    new OleDbParameter("@progressPercent", 100));
            }
        }

        /// <summary>
        /// Update progress percentage for a module (for partial progress like video watching)
        /// </summary>
        public int UpdateModuleProgress(int studentId, int moduleId, int courseId, int progressPercent)
        {
            bool isCompleted = progressPercent >= 100;

            // Check if record already exists
            var existing = GetProgressRecord(studentId, moduleId);

            if (existing != null)
            {
                string updateSql = @"UPDATE [StudentModuleProgress] SET
                    [ProgressPercent] = ?,
                    [IsCompleted] = ?,
                    [CompletedAt] = ?
                    WHERE [ProgressID] = ?";

                return SaveChanges(updateSql,
                    new OleDbParameter("@progressPercent", progressPercent),
                    new OleDbParameter("@isCompleted", isCompleted),
                    new OleDbParameter("@completedAt", OleDbType.Date) { Value = isCompleted ? (object)DateTime.Now : DBNull.Value },
                    new OleDbParameter("@progressId", existing.ProgressId));
            }
            else
            {
                string insertSql = @"INSERT INTO [StudentModuleProgress]
                    ([StudentID], [ModuleID], [CourseID], [IsCompleted], [CompletedAt], [ProgressPercent])
                    VALUES (?, ?, ?, ?, ?, ?)";

                return SaveChanges(insertSql,
                    new OleDbParameter("@studentId", studentId),
                    new OleDbParameter("@moduleId", moduleId),
                    new OleDbParameter("@courseId", courseId),
                    new OleDbParameter("@isCompleted", isCompleted),
                    new OleDbParameter("@completedAt", OleDbType.Date) { Value = isCompleted ? (object)DateTime.Now : DBNull.Value },
                    new OleDbParameter("@progressPercent", progressPercent));
            }
        }

        /// <summary>
        /// Get progress record for a specific student and module
        /// </summary>
        public StudentModuleProgress GetProgressRecord(int studentId, int moduleId)
        {
            string sql = "SELECT * FROM [StudentModuleProgress] WHERE [StudentID] = ? AND [ModuleID] = ?";
            var list = Select(sql,
                new OleDbParameter("@studentId", studentId),
                new OleDbParameter("@moduleId", moduleId))
                .OfType<StudentModuleProgress>()
                .ToList();

            return list.Count == 1 ? list[0] : null;
        }

        /// <summary>
        /// Get all progress records for a student in a specific course
        /// </summary>
        public List<StudentModuleProgress> GetStudentProgressForCourse(int studentId, int courseId)
        {
            string sql = "SELECT * FROM [StudentModuleProgress] WHERE [StudentID] = ? AND [CourseID] = ?";
            return Select(sql,
                new OleDbParameter("@studentId", studentId),
                new OleDbParameter("@courseId", courseId))
                .OfType<StudentModuleProgress>()
                .ToList();
        }

        /// <summary>
        /// Get all progress records for a student across all courses
        /// </summary>
        public List<StudentModuleProgress> GetAllStudentProgress(int studentId)
        {
            string sql = "SELECT * FROM [StudentModuleProgress] WHERE [StudentID] = ?";
            return Select(sql, new OleDbParameter("@studentId", studentId))
                .OfType<StudentModuleProgress>()
                .ToList();
        }

        /// <summary>
        /// Get completed modules for a student in a course
        /// </summary>
        public List<StudentModuleProgress> GetCompletedModules(int studentId, int courseId)
        {
            string sql = "SELECT * FROM [StudentModuleProgress] WHERE [StudentID] = ? AND [CourseID] = ? AND [IsCompleted] = ?";
            return Select(sql,
                new OleDbParameter("@studentId", studentId),
                new OleDbParameter("@courseId", courseId),
                new OleDbParameter("@isCompleted", true))
                .OfType<StudentModuleProgress>()
                .ToList();
        }

        /// <summary>
        /// Check if a student has completed a specific module
        /// </summary>
        public bool IsModuleCompleted(int studentId, int moduleId)
        {
            string sql = "SELECT [IsCompleted] FROM [StudentModuleProgress] WHERE [StudentID] = ? AND [ModuleID] = ?";
            object result = SelectScalar(sql,
                new OleDbParameter("@studentId", studentId),
                new OleDbParameter("@moduleId", moduleId));

            if (result != null && result != DBNull.Value)
                return bool.Parse(result.ToString());

            return false;
        }

        /// <summary>
        /// Get aggregated course progress for a student
        /// </summary>
        public StudentCourseProgress GetCourseProgress(int studentId, int courseId)
        {
            CourseModuleDB moduleDb = new CourseModuleDB();
            CourseDB courseDb = new CourseDB();

            var course = courseDb.GetCourseById(courseId);
            var allModules = moduleDb.GetModulesForCourse(courseId);
            var studentProgress = GetStudentProgressForCourse(studentId, courseId);

            int totalModules = allModules.Count;
            int completedModules = studentProgress.Count(p => p.IsCompleted);

            return new StudentCourseProgress
            {
                StudentId = studentId,
                CourseId = courseId,
                CourseName = course?.CourseName ?? "Unknown Course",
                CourseDescription = course?.Description ?? "",
                TotalModules = totalModules,
                CompletedModules = completedModules,
                ProgressPercent = totalModules > 0 ? (completedModules * 100) / totalModules : 0,
                ModuleProgress = studentProgress
            };
        }

        /// <summary>
        /// Get progress summary for all courses for a student
        /// </summary>
        public List<StudentCourseProgress> GetAllCoursesProgress(int studentId)
        {
            CourseDB courseDb = new CourseDB();
            var courses = courseDb.GetActiveCourses();

            List<StudentCourseProgress> progressList = new List<StudentCourseProgress>();

            foreach (var course in courses)
            {
                progressList.Add(GetCourseProgress(studentId, course.Id));
            }

            return progressList;
        }

        /// <summary>
        /// Count completed modules for a student in a course
        /// </summary>
        public int CountCompletedModules(int studentId, int courseId)
        {
            string sql = "SELECT COUNT(*) FROM [StudentModuleProgress] WHERE [StudentID] = ? AND [CourseID] = ? AND [IsCompleted] = ?";
            object result = SelectScalar(sql,
                new OleDbParameter("@studentId", studentId),
                new OleDbParameter("@courseId", courseId),
                new OleDbParameter("@isCompleted", true));

            return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// Reset progress for a student on a specific course
        /// </summary>
        public int ResetCourseProgress(int studentId, int courseId)
        {
            string sql = "DELETE FROM [StudentModuleProgress] WHERE [StudentID] = ? AND [CourseID] = ?";
            return SaveChanges(sql,
                new OleDbParameter("@studentId", studentId),
                new OleDbParameter("@courseId", courseId));
        }

        /// <summary>
        /// Reset progress for a student on a specific module
        /// </summary>
        public int ResetModuleProgress(int studentId, int moduleId)
        {
            string sql = "DELETE FROM [StudentModuleProgress] WHERE [StudentID] = ? AND [ModuleID] = ?";
            return SaveChanges(sql,
                new OleDbParameter("@studentId", studentId),
                new OleDbParameter("@moduleId", moduleId));
        }
    }
}
