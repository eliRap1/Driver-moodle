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

        protected List<Lessons> SelectLessons(string sqlCommandTxt)
        {
            List<Lessons> list = new List<Lessons>();
            try
            {
                connection.Open(); //was missing
                command.CommandText = sqlCommandTxt;
                reader = command.ExecuteReader();
                //NULLבנתיים לא בודקים האם אחד השדות הוא 
                while (reader.Read())
                {
                    Lessons entity = new Lessons(); //יוצר אובייקט מטיפוס המתאים
                    CreateModel(entity);
                    list.Add(entity);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message); //will word is every world, not only in world of Console

                //the output - we'll see in the output window of VisualStudio
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return list;
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
            List<Lessons> list = SelectLessons(sql);
            return list;
        }
        public List<Lessons> GetAllStudentLessons(int sid)
        {
            string sql = "Select * From Lessons where StudentID=" + sid;
            List<Lessons> list = SelectLessons(sql);
            return list;
        }
        public void AddLessonForStudent(int sid,string Date,string time)
        {
            UserDB udb = new UserDB();
            int tid = udb.GetTeacherId(sid);
            string sql = $"Select * From Lessons where TeacherID={tid} and StudentID= {sid}";
            List<Lessons> lst = SelectLessons(sql);
            int lesID = lst.Capacity + 1;
            sql = $"INSERT into Lessons (StudentID, TeacherID,[Date],[Time],LessonsID,paid) VALUES ({sid},{tid},'{Date}','{time}',{lesID},false)";
            SaveChanges(sql);
        }
        public void MarkLessonPaid(int id)
        {
            string sql = $"Update Lessons set paid={true} where id={id}";
            SaveChanges(sql);
        }

    }
}
