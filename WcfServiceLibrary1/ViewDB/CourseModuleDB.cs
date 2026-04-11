using Model;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace ViewDB
{
    /// <summary>
    /// Database access class for CourseModules table.
    /// Provides CRUD operations for individual learning modules within courses.
    ///
    /// TABLE SCHEMA (CourseModules):
    /// - ModuleID (AutoNumber, Primary Key)
    /// - CourseID (Integer, Foreign Key to Courses)
    /// - ModuleName (Text, 150)
    /// - Description (Memo)
    /// - OrderIndex (Integer) - sequence within the course
    /// - ContentType (Text, 50) - "Video", "Text", "Quiz", "Assignment"
    /// - ContentUrl (Text, 255) - URL or path to content
    /// - DurationMinutes (Integer)
    /// - IsRequired (Yes/No)
    /// - CreatedAt (Date/Time)
    /// </summary>
    public class CourseModuleDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new CourseModule();
        }

        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    CourseModule m = (CourseModule)entity;
                    m.ModuleId = Convert.ToInt32(reader["ModuleID"]);
                    m.CourseId = Convert.ToInt32(reader["CourseID"]);
                    m.ModuleName = reader["ModuleName"].ToString();

                    try { m.Description = reader["Description"].ToString(); }
                    catch { m.Description = ""; }

                    try { m.OrderIndex = Convert.ToInt32(reader["OrderIndex"]); }
                    catch { m.OrderIndex = 0; }

                    try { m.ContentType = reader["ContentType"].ToString(); }
                    catch { m.ContentType = "Text"; }

                    try { m.ContentUrl = reader["ContentUrl"].ToString(); }
                    catch { m.ContentUrl = ""; }

                    try { m.DurationMinutes = Convert.ToInt32(reader["DurationMinutes"]); }
                    catch { m.DurationMinutes = 0; }

                    try { m.IsRequired = bool.Parse(reader["IsRequired"].ToString()); }
                    catch { m.IsRequired = true; }

                    try { m.CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()); }
                    catch { m.CreatedAt = DateTime.Now; }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("CourseModuleDB CreateModel Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Get all modules for a specific course, ordered by sequence
        /// </summary>
        public List<CourseModule> GetModulesForCourse(int courseId)
        {
            string sql = "SELECT * FROM [CourseModules] WHERE [CourseID] = ? ORDER BY [OrderIndex]";
            return Select(sql, new OleDbParameter("@courseId", courseId))
                .OfType<CourseModule>()
                .ToList();
        }

        /// <summary>
        /// Get a specific module by ID
        /// </summary>
        public CourseModule GetModuleById(int moduleId)
        {
            string sql = "SELECT * FROM [CourseModules] WHERE [ModuleID] = ?";
            var list = Select(sql, new OleDbParameter("@moduleId", moduleId))
                .OfType<CourseModule>()
                .ToList();

            return list.Count == 1 ? list[0] : null;
        }

        /// <summary>
        /// Get all modules of a specific type for a course
        /// </summary>
        public List<CourseModule> GetModulesByType(int courseId, string contentType)
        {
            string sql = "SELECT * FROM [CourseModules] WHERE [CourseID] = ? AND [ContentType] = ? ORDER BY [OrderIndex]";
            return Select(sql,
                new OleDbParameter("@courseId", courseId),
                new OleDbParameter("@contentType", contentType))
                .OfType<CourseModule>()
                .ToList();
        }

        /// <summary>
        /// Get only required modules for a course
        /// </summary>
        public List<CourseModule> GetRequiredModules(int courseId)
        {
            string sql = "SELECT * FROM [CourseModules] WHERE [CourseID] = ? AND [IsRequired] = ? ORDER BY [OrderIndex]";
            return Select(sql,
                new OleDbParameter("@courseId", courseId),
                new OleDbParameter("@isRequired", true))
                .OfType<CourseModule>()
                .ToList();
        }

        /// <summary>
        /// Add a new module to a course
        /// </summary>
        public int AddModule(CourseModule module)
        {
            // Get next order index if not specified
            if (module.OrderIndex == 0)
            {
                module.OrderIndex = GetNextOrderIndex(module.CourseId);
            }

            string sql = @"INSERT INTO [CourseModules]
                ([CourseID], [ModuleName], [Description], [OrderIndex], [ContentType],
                 [ContentUrl], [DurationMinutes], [IsRequired], [CreatedAt])
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";

            var parameters = new[]
            {
                new OleDbParameter("@courseId", OleDbType.Integer) { Value = module.CourseId },
                new OleDbParameter("@moduleName", OleDbType.VarWChar) { Value = module.ModuleName },
                new OleDbParameter("@description", OleDbType.LongVarWChar) { Value = module.Description ?? "" },
                new OleDbParameter("@orderIndex", OleDbType.Integer) { Value = module.OrderIndex },
                new OleDbParameter("@contentType", OleDbType.VarWChar) { Value = module.ContentType ?? "Text" },
                new OleDbParameter("@contentUrl", OleDbType.VarWChar) { Value = module.ContentUrl ?? "" },
                new OleDbParameter("@durationMinutes", OleDbType.Integer) { Value = module.DurationMinutes },
                new OleDbParameter("@isRequired", OleDbType.Boolean) { Value = module.IsRequired },
                new OleDbParameter("@createdAt", OleDbType.Date) { Value = DateTime.Now }
            };

            return SaveChanges(sql, parameters);
        }

        /// <summary>
        /// Update an existing module
        /// </summary>
        public int UpdateModule(CourseModule module)
        {
            string sql = @"UPDATE [CourseModules] SET
                [ModuleName] = ?,
                [Description] = ?,
                [OrderIndex] = ?,
                [ContentType] = ?,
                [ContentUrl] = ?,
                [DurationMinutes] = ?,
                [IsRequired] = ?
                WHERE [ModuleID] = ?";

            var parameters = new[]
            {
                new OleDbParameter("@moduleName", OleDbType.VarWChar) { Value = module.ModuleName },
                new OleDbParameter("@description", OleDbType.LongVarWChar) { Value = module.Description ?? "" },
                new OleDbParameter("@orderIndex", OleDbType.Integer) { Value = module.OrderIndex },
                new OleDbParameter("@contentType", OleDbType.VarWChar) { Value = module.ContentType ?? "Text" },
                new OleDbParameter("@contentUrl", OleDbType.VarWChar) { Value = module.ContentUrl ?? "" },
                new OleDbParameter("@durationMinutes", OleDbType.Integer) { Value = module.DurationMinutes },
                new OleDbParameter("@isRequired", OleDbType.Boolean) { Value = module.IsRequired },
                new OleDbParameter("@moduleId", OleDbType.Integer) { Value = module.ModuleId }
            };

            return SaveChanges(sql, parameters);
        }

        /// <summary>
        /// Delete a module
        /// </summary>
        public int DeleteModule(int moduleId)
        {
            string sql = "DELETE FROM [CourseModules] WHERE [ModuleID] = ?";
            return SaveChanges(sql, new OleDbParameter("@moduleId", moduleId));
        }

        /// <summary>
        /// Update the order of a module within a course
        /// </summary>
        public void UpdateModuleOrder(int moduleId, int newOrderIndex)
        {
            string sql = "UPDATE [CourseModules] SET [OrderIndex] = ? WHERE [ModuleID] = ?";
            SaveChanges(sql,
                new OleDbParameter("@orderIndex", newOrderIndex),
                new OleDbParameter("@moduleId", moduleId));
        }

        /// <summary>
        /// Get the next available order index for a course
        /// </summary>
        private int GetNextOrderIndex(int courseId)
        {
            string sql = "SELECT MAX([OrderIndex]) FROM [CourseModules] WHERE [CourseID] = ?";
            object result = SelectScalar(sql, new OleDbParameter("@courseId", courseId));
            return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) + 1 : 1;
        }

        /// <summary>
        /// Get total duration of all modules in a course (in minutes)
        /// </summary>
        public int GetCourseTotalDuration(int courseId)
        {
            string sql = "SELECT SUM([DurationMinutes]) FROM [CourseModules] WHERE [CourseID] = ?";
            object result = SelectScalar(sql, new OleDbParameter("@courseId", courseId));
            return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// Get count of modules in a course
        /// </summary>
        public int GetModuleCount(int courseId)
        {
            string sql = "SELECT COUNT(*) FROM [CourseModules] WHERE [CourseID] = ?";
            object result = SelectScalar(sql, new OleDbParameter("@courseId", courseId));
            return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
        }

        // ==================== STUDENT PROGRESS OPERATIONS ====================

        /// <summary>
        /// Get student's progress for all courses
        /// </summary>
        public List<StudentCourseProgress> GetStudentCourseProgress(int studentId)
        {
            List<StudentCourseProgress> progressList = new List<StudentCourseProgress>();

            try
            {
                // Get all active courses
                CourseDB courseDB = new CourseDB();
                var courses = courseDB.GetActiveCourses();

                foreach (var course in courses)
                {
                    var modules = GetModulesForCourse(course.Id);
                    var completedModules = GetStudentCompletedModulesForCourse(studentId, course.Id);

                    var courseProgress = new StudentCourseProgress
                    {
                        StudentId = studentId,
                        CourseId = course.Id,
                        CourseName = course.CourseName,
                        CourseDescription = course.Description ?? "",
                        TotalModules = modules.Count,
                        CompletedModules = completedModules.Count,
                        ProgressPercent = modules.Count > 0
                            ? (int)((double)completedModules.Count / modules.Count * 100)
                            : 0
                    };

                    // Add module-level progress
                    foreach (var module in modules)
                    {
                        var moduleProgress = completedModules.FirstOrDefault(m => m.ModuleId == module.ModuleId);
                        if (moduleProgress != null)
                        {
                            courseProgress.ModuleProgress.Add(moduleProgress);
                        }
                        else
                        {
                            courseProgress.ModuleProgress.Add(new StudentModuleProgress
                            {
                                StudentId = studentId,
                                ModuleId = module.ModuleId,
                                CourseId = course.Id,
                                IsCompleted = false,
                                ProgressPercent = 0
                            });
                        }
                    }

                    progressList.Add(courseProgress);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetStudentCourseProgress Error: " + ex.Message);
            }

            return progressList;
        }

        /// <summary>
        /// Get completed modules for a student in a specific course
        /// </summary>
        private List<StudentModuleProgress> GetStudentCompletedModulesForCourse(int studentId, int courseId)
        {
            string sql = "SELECT * FROM [StudentModuleProgress] WHERE [StudentID] = ? AND [CourseID] = ? AND [IsCompleted] = True";
            return SelectProgress(sql,
                new OleDbParameter("@studentId", studentId),
                new OleDbParameter("@courseId", courseId));
        }

        /// <summary>
        /// Mark a module as complete for a student
        /// </summary>
        public bool MarkModuleComplete(int studentId, int moduleId)
        {
            try
            {
                // First get the courseId for this module
                var module = GetModuleById(moduleId);
                if (module == null)
                {
                    System.Diagnostics.Debug.WriteLine("MarkModuleComplete: Module not found");
                    return false;
                }

                int courseId = module.CourseId;

                // Check if progress record exists
                string checkSql = "SELECT * FROM [StudentModuleProgress] WHERE [StudentID] = ? AND [ModuleID] = ?";
                var existingProgress = SelectProgress(checkSql,
                    new OleDbParameter("@studentId", studentId),
                    new OleDbParameter("@moduleId", moduleId));

                if (existingProgress.Count > 0)
                {
                    // Update existing record
                    string updateSql = "UPDATE [StudentModuleProgress] SET [IsCompleted] = ?, [CompletedAt] = ?, [ProgressPercent] = ? WHERE [StudentID] = ? AND [ModuleID] = ?";
                    SaveChanges(updateSql,
                        new OleDbParameter("@isCompleted", true),
                        new OleDbParameter("@completedAt", DateTime.Now),
                        new OleDbParameter("@progressPercent", 100),
                        new OleDbParameter("@studentId", studentId),
                        new OleDbParameter("@moduleId", moduleId));
                }
                else
                {
                    // Insert new record
                    string insertSql = "INSERT INTO [StudentModuleProgress] ([StudentID], [ModuleID], [CourseID], [IsCompleted], [CompletedAt], [ProgressPercent]) VALUES (?, ?, ?, ?, ?, ?)";
                    SaveChanges(insertSql,
                        new OleDbParameter("@studentId", studentId),
                        new OleDbParameter("@moduleId", moduleId),
                        new OleDbParameter("@courseId", courseId),
                        new OleDbParameter("@isCompleted", true),
                        new OleDbParameter("@completedAt", DateTime.Now),
                        new OleDbParameter("@progressPercent", 100));
                }

                System.Diagnostics.Debug.WriteLine($"MarkModuleComplete: StudentId={studentId}, ModuleId={moduleId} marked complete");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MarkModuleComplete Error: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get all completed modules for a student across all courses
        /// </summary>
        public List<StudentModuleProgress> GetStudentCompletedModules(int studentId)
        {
            string sql = "SELECT * FROM [StudentModuleProgress] WHERE [StudentID] = ? AND [IsCompleted] = True";
            return SelectProgress(sql, new OleDbParameter("@studentId", studentId));
        }

        /// <summary>
        /// Helper method to select StudentModuleProgress records
        /// </summary>
        private List<StudentModuleProgress> SelectProgress(string sqlCommandTxt, params OleDbParameter[] parameters)
        {
            List<StudentModuleProgress> list = new List<StudentModuleProgress>();
            try
            {
                connection.Open();
                command.CommandText = sqlCommandTxt;
                command.Parameters.Clear();

                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    StudentModuleProgress progress = new StudentModuleProgress();
                    try { progress.ProgressId = (int)reader["ProgressID"]; }
                    catch { progress.ProgressId = 0; }

                    progress.StudentId = (int)reader["StudentID"];
                    progress.ModuleId = (int)reader["ModuleID"];
                    progress.CourseId = (int)reader["CourseID"];

                    try { progress.IsCompleted = bool.Parse(reader["IsCompleted"].ToString()); }
                    catch { progress.IsCompleted = false; }

                    try
                    {
                        if (reader["CompletedAt"] != DBNull.Value)
                            progress.CompletedAt = DateTime.Parse(reader["CompletedAt"].ToString());
                    }
                    catch { progress.CompletedAt = null; }

                    try { progress.ProgressPercent = (int)reader["ProgressPercent"]; }
                    catch { progress.ProgressPercent = 0; }

                    list.Add(progress);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SelectProgress Error: " + ex.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
            return list;
        }
    }
}
