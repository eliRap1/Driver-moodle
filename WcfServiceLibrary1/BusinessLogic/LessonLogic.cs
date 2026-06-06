using Model;
using System;
using System.Collections.Generic;
using ViewDB;

namespace BusinessLogic
{
    /// <summary>
    /// Business logic for lessons: scheduling, cancellation (with teacher notification),
    /// marking paid, and lesson queries.
    /// </summary>
    public static class LessonLogic
    {
        public static void CancelLesson(int lessonId)
        {
            try
            {
                // Get lesson details before cancelling
                var lesson = new LessonsDB().GetLessonById(lessonId);

                if (lesson != null)
                {
                    // Get student name
                    var student = new UserDB().GetUserById(lesson.StudentId, "Student");
                    string studentName = student?.Username ?? "Unknown";

                    // Cancel the lesson
                    new LessonsDB().CancelLesson(lessonId);

                    // Send notification to teacher
                    new NotificationDB().SendLessonCancelledNotification(
                        lesson.StudentId,
                        studentName,
                        lesson.TeacherId,
                        lesson.Date,
                        lesson.Time
                    );
                }
                else
                {
                    new LessonsDB().CancelLesson(lessonId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CancelLesson Error: {ex.Message}");
                new LessonsDB().CancelLesson(lessonId);
            }
        }

        public static void MarkLessonPaid(int id)
        {
            new LessonsDB().MarkLessonPaid(id);
        }

        public static void AddLessonForStudent(int sid, string Date, string time)
        {
            try
            {
                new LessonsDB().AddLessonForStudent(sid, Date, time);
            }
            catch (Exception ex)
            {
                throw new System.ServiceModel.FaultException(
                    "AddLessonForStudent failed: " + ex.GetBaseException().Message);
            }
        }

        public static List<Lessons> GetAllStudentLessons(int id)
        {
            return new LessonsDB().GetAllStudentLessons(id);
        }

        public static List<Lessons> GetAllTeacherLessons(int tid)
        {
            return new LessonsDB().GetAllTeacherLessons(tid);
        }
    }
}
