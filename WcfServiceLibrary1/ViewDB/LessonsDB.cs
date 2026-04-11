using Model;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace ViewDB
{
    [System.Runtime.Serialization.DataContract]
    public class Lessons : Base
    {
        [System.Runtime.Serialization.DataMember]
        public int LessonId { get; set; }
        [System.Runtime.Serialization.DataMember]
        public int StudentId { get; set; }
        [System.Runtime.Serialization.DataMember]
        public int TeacherId { get; set; }
        [System.Runtime.Serialization.DataMember]
        public bool paid { get; set; }
        [System.Runtime.Serialization.DataMember]
        public string Date { get; set; }
        [System.Runtime.Serialization.DataMember]
        public string Time { get; set; }
        [System.Runtime.Serialization.DataMember]
        public int Canceled { get; set; }
    }

    public class LessonsDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new Lessons();
        }

        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    Lessons s = (Lessons)entity;
                    s.LessonId = (int)reader["LessonID"];
                    s.StudentId = (int)reader["StudentID"];
                    s.TeacherId = (int)reader["TeacherID"];
                    s.paid = bool.Parse(reader["paid"].ToString());
                    s.Date = reader["Date"].ToString();
                    s.Time = reader["Time"].ToString();
                    s.Canceled = (int)reader["Canceled"];
                }
                catch (Exception ex)
                {
                    Console.WriteLine("CreateModel Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// SECURE: Cancel lesson with parameterized query
        /// </summary>
        public void CancelLesson(int lessonId)
        {
            string sql = "UPDATE [Lessons] SET Canceled = 1 WHERE LessonId = ?";
            SaveChanges(sql, new OleDbParameter("@lessonId", lessonId));
        }

        /// <summary>
        /// SECURE: Get all lessons for a teacher
        /// </summary>
        public List<Lessons> GetAllTeacherLessons(int tid)
        {
            string sql = "SELECT * FROM [Lessons] WHERE TeacherID = ?";
            return Select(sql, new OleDbParameter("@tid", tid))
                .OfType<Lessons>()
                .ToList();
        }

        /// <summary>
        /// SECURE: Get all lessons for a student
        /// </summary>
        public List<Lessons> GetAllStudentLessons(int sid)
        {
            string sql = "SELECT * FROM [Lessons] WHERE StudentID = ?";
            return Select(sql, new OleDbParameter("@sid", sid))
                .OfType<Lessons>()
                .ToList();
        }

        /// <summary>
        /// SECURE: Add lesson for student
        /// </summary>
        public void AddLessonForStudent(int sid, string date, string time)
        {
            UserDB udb = new UserDB();
            int tid = udb.GetTeacherId(sid);

            string sql = "INSERT INTO [Lessons] (StudentID, TeacherID, [Date], [Time], paid, Canceled) " +
                        "VALUES (?, ?, ?, ?, ?, ?)";

            SaveChanges(sql,
                new OleDbParameter("@sid", sid),
                new OleDbParameter("@tid", tid),
                new OleDbParameter("@date", date),
                new OleDbParameter("@time", time),
                new OleDbParameter("@paid", false),
                new OleDbParameter("@canceled", 0));
        }

        /// <summary>
        /// SECURE: Mark lesson as paid
        /// </summary>
        public void MarkLessonPaid(int id)
        {
            string sql = "UPDATE [Lessons] SET Paid = ? WHERE LessonId = ?";
            SaveChanges(sql,
                new OleDbParameter("@paid", true),
                new OleDbParameter("@id", id));
        }

        /// <summary>
        /// Get a specific lesson by ID
        /// </summary>
        public Lessons GetLessonById(int lessonId)
        {
            string sql = "SELECT * FROM [Lessons] WHERE LessonID = ?";
            var list = Select(sql, new OleDbParameter("@lessonId", lessonId))
                .OfType<Lessons>()
                .ToList();

            return list.Count == 1 ? list[0] : null;
        }
    }
}