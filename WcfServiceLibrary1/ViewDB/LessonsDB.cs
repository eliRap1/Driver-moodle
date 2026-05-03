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

            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>
        /// Counts active (non-cancelled) lessons for a teacher's confirmed students.
        /// Uses an INNER JOIN between Lessons and Student so cancelled lessons and
        /// unconfirmed students are excluded in a single query.
        /// </summary>
        public int CountActiveLessonsForConfirmedStudents(int teacherId)
        {
            string sql = @"SELECT COUNT(*)
                           FROM [Lessons] AS L
                           INNER JOIN [Student] AS S ON L.[StudentID] = S.[id]
                           WHERE L.[TeacherID] = ?
                             AND L.[Canceled] = 0
                             AND S.[Confirmed] = TRUE";
            object result = SelectScalar(sql, new OleDbParameter("@tid", teacherId));
            return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// Returns the unpaid lessons for a teacher that belong to confirmed students,
        /// using an INNER JOIN. Pre-filters at the SQL level so the client never sees
        /// orphaned rows (cancelled lessons or unconfirmed students).
        /// </summary>
        public List<Lessons> GetUnpaidLessonsForTeacher(int teacherId)
        {
            string sql = @"SELECT L.[LessonID], L.[StudentID], L.[TeacherID],
                                  L.[Date], L.[Time], L.[paid], L.[Canceled]
                           FROM [Lessons] AS L
                           INNER JOIN [Student] AS S ON L.[StudentID] = S.[id]
                           WHERE L.[TeacherID] = ?
                             AND L.[paid] = FALSE
                             AND L.[Canceled] = 0
                             AND S.[Confirmed] = TRUE
                           ORDER BY L.[Date], L.[Time]";
            // Map by id alias so BaseDB.CreateModel can read [id]; use LessonID as identity
            // Note: BaseDB pulls "id" from the reader, but the Lessons CreateModel reads LessonID
            // directly, so the absence of an "id" column here is fine.
            return Select(sql, new OleDbParameter("@tid", teacherId))
                .OfType<Lessons>()
                .ToList();
        }
    }
}