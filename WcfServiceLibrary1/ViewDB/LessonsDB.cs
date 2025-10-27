using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewDB
{
    public class Lessons :Base
    {
        //public int Id { get; set; }
        public int LessonId { get; set; }
        public int StudentId { get; set; }
        public int TeacherId { get; set; }
        public bool paid { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }

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

                }
                catch
                {
                    Console.WriteLine("No ID in DB");
                }
            }
        }
        public List<Lessons> GetAllTeacherLessons(int tid)
        {
            string sql = "Select * From Lessons where TeacherID=" + tid;
            List<Lessons> list = Select(sql).OfType<Lessons>().ToList();
            return list;
        }
        //public List<Lessons> GetAllTeacherLessonsForDate(int tid, string date)
        //{
        //    string sql = "Select * From Lessons where TeacherID=" + tid + " and Date='" + date + "'";
        //    List<Lessons> list = Select(sql).OfType<Lessons>().ToList();
        //    return list;
        //}
        public List<Lessons> GetAllStudentLessons(int sid)
        {
            string sql = "Select * From Lessons where StudentID=" + sid;
            List<Lessons> list = Select(sql).OfType<Lessons>().ToList();
            return list;
        }
        public void AddLessonForStudent(int sid,string Date,string time)
        {
            UserDB udb = new UserDB();
            int tid = udb.GetTeacherId(sid);
            //string sql = $"Select * From Lessons where TeacherID={tid} and StudentID= {sid}";//
            //List<Lessons> lst = Select(sql).OfType<Lessons>().ToList();//
            //int lesID = lst.Capacity + 1;//pretty useless (just use auto number) in the acsses
            string sql = $"INSERT into Lessons (StudentID, TeacherID,[Date],[Time],paid) VALUES ({sid},{tid},'{Date}','{time}',0)";
            SaveChanges(sql);
        }
        public void MarkLessonPaid(int id)
        {
            string sql = $"Update Lessons set paid={true} where id={id}";
            SaveChanges(sql);
        }
        

    }
}
