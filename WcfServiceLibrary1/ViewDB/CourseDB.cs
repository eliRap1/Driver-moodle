using Model;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace ViewDB
{
    /// <summary>
    /// Database access class for Courses table.
    /// Provides CRUD operations for learning courses.
    ///
    /// TABLE SCHEMA (Courses):
    /// - CourseID (AutoNumber, Primary Key)
    /// - CourseName (Text, 100)
    /// - Description (Memo)
    /// - DisplayOrder (Integer)
    /// - IsActive (Yes/No)
    /// - CreatedDate (Date/Time)
    /// </summary>
    public class CourseDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new Course();
        }

        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    Course c = (Course)entity;
                    c.CourseName = reader["CourseName"].ToString();

                    try { c.Description = reader["Description"].ToString(); }
                    catch { c.Description = ""; }

                    try { c.DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]); }
                    catch { c.DisplayOrder = 0; }

                    try { c.IsActive = bool.Parse(reader["IsActive"].ToString()); }
                    catch { c.IsActive = true; }

                    try { c.CreatedDate = DateTime.Parse(reader["CreatedDate"].ToString()); }
                    catch { c.CreatedDate = DateTime.Now; }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("CourseDB CreateModel Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Get all courses ordered by DisplayOrder
        /// </summary>
        public List<Course> GetAllCourses()
        {
            string sql = "SELECT * FROM [Courses] ORDER BY [DisplayOrder]";
            return Select(sql).OfType<Course>().ToList();
        }

        /// <summary>
        /// Get only active courses
        /// </summary>
        public List<Course> GetActiveCourses()
        {
            string sql = "SELECT * FROM [Courses] WHERE [IsActive] = ? ORDER BY [DisplayOrder]";
            return Select(sql, new OleDbParameter("@isActive", true))
                .OfType<Course>()
                .ToList();
        }

        /// <summary>
        /// Get a specific course by ID
        /// </summary>
        public Course GetCourseById(int courseId)
        {
            string sql = "SELECT * FROM [Courses] WHERE [id] = ?";
            var list = Select(sql, new OleDbParameter("@courseId", courseId))
                .OfType<Course>()
                .ToList();

            return list.Count == 1 ? list[0] : null;
        }

        /// <summary>
        /// Add a new course
        /// </summary>
        public int AddCourse(Course course)
        {
            string sql = @"INSERT INTO [Courses]
                ([CourseName], [Description], [DisplayOrder], [IsActive], [CreatedDate])
                VALUES (?, ?, ?, ?, ?)";

            var parameters = new[]
            {
                new OleDbParameter("@courseName", OleDbType.VarWChar) { Value = course.CourseName },
                new OleDbParameter("@description", OleDbType.LongVarWChar) { Value = course.Description ?? "" },
                new OleDbParameter("@displayOrder", OleDbType.Integer) { Value = course.DisplayOrder },
                new OleDbParameter("@isActive", OleDbType.Boolean) { Value = course.IsActive },
                new OleDbParameter("@createdDate", OleDbType.Date) { Value = DateTime.Now }
            };

            return SaveChanges(sql, parameters);
        }

        /// <summary>
        /// Update an existing course
        /// </summary>
        public int UpdateCourse(Course course)
        {
            string sql = @"UPDATE [Courses] SET
                [CourseName] = ?,
                [Description] = ?,
                [DisplayOrder] = ?,
                [IsActive] = ?
                WHERE [id] = ?";

            var parameters = new[]
            {
                new OleDbParameter("@courseName", OleDbType.VarWChar) { Value = course.CourseName },
                new OleDbParameter("@description", OleDbType.LongVarWChar) { Value = course.Description ?? "" },
                new OleDbParameter("@displayOrder", OleDbType.Integer) { Value = course.DisplayOrder },
                new OleDbParameter("@isActive", OleDbType.Boolean) { Value = course.IsActive },
                new OleDbParameter("@id", OleDbType.Integer) { Value = course.Id }
            };

            return SaveChanges(sql, parameters);
        }

        /// <summary>
        /// Deactivate a course (soft delete)
        /// </summary>
        public int DeactivateCourse(int courseId)
        {
            string sql = "UPDATE [Courses] SET [IsActive] = ? WHERE [id] = ?";
            return SaveChanges(sql,
                new OleDbParameter("@isActive", false),
                new OleDbParameter("@id", courseId));
        }

        /// <summary>
        /// Delete a course permanently (use with caution)
        /// </summary>
        public int DeleteCourse(int courseId)
        {
            string sql = "DELETE FROM [Courses] WHERE [id] = ?";
            return SaveChanges(sql, new OleDbParameter("@id", courseId));
        }

        /// <summary>
        /// Get total module count for a course
        /// </summary>
        public int GetModuleCount(int courseId)
        {
            string sql = "SELECT COUNT(*) FROM [CourseModules] WHERE [CourseID] = ?";
            object result = SelectScalar(sql, new OleDbParameter("@courseId", courseId));
            return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// Reorder courses - update display order
        /// </summary>
        public void UpdateDisplayOrder(int courseId, int newOrder)
        {
            string sql = "UPDATE [Courses] SET [DisplayOrder] = ? WHERE [id] = ?";
            SaveChanges(sql,
                new OleDbParameter("@displayOrder", newOrder),
                new OleDbParameter("@id", courseId));
        }
    }
}
