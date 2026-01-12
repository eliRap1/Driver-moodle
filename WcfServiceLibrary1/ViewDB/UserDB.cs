using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewDB
{
    public class UserDB : BaseDB
    {

        protected override Base NewEntity()
        {
            return new UserInfo();
        }
      
        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    UserInfo s = (UserInfo)entity;
                    s.Username = reader["username"].ToString();
                    s.Password = reader["password"].ToString();
                    try
                    {
                        s.Rewiew = reader["Rewiew"].ToString();
                    }
                    catch { }
                    try
                    {
                        s.Rating = (double)reader["Rating"];
                    }
                    catch { }
                    s.Confirmed = bool.Parse(reader["Confirmed"].ToString());
                    s.Email = reader["email"].ToString();
                    s.Phone = reader["phone"].ToString();
                    s.TeacherId = (int)reader["TeacherId"];
                    if (s.TeacherId != 0)
                        s.StudentId = (int)reader["id"];
                    //s.Lessons = (string)reader["Lessons"];
                    s.LessonPrice = (int)reader["lessonPrice"];
                    


                }
                catch
                {
                    Console.WriteLine("No ID in DB");
                }
            }
        }
        
        public UserInfo GetUserById(int id, string table)
        {
            string sqlStr = "Select * From " +table+ " Where id=" + id;
            List<UserInfo> list = Select(sqlStr).OfType<UserInfo>().ToList();
            if (list.Count == 1)
            { return (UserInfo)list[0]; }
            else { return null; }
        }
        public AllUsers GetAllStudents()
        {
            List<Base> list = Select("Select * From Student");
            AllUsers studs = new AllUsers(list);
            return studs;
        }
        public AllUsers GetAllTeacher()
        {
            List<Base> list = Select("Select * From Teacher");
            AllUsers studs = new AllUsers(list);
            return studs;
        }

        public bool AddUser(UserInfo user)
        {
            string table = "Teacher"; 
            string sqlstr = "";
            sqlstr = $"INSERT INTO " + table + " ([username], [password], [email], [phone],[Rating]) " + "" +
                $"VALUES ('{user.Username}', '{user.Password}','{user.Email}' ,'{user.Phone}',{user.Rating})";
            return SaveChanges(sqlstr) != 0;

        }
        public object SelectScalar(string query)
        {
            try
            {
                connection.Open(); //was missing
                command.CommandText = query;
                reader = command.ExecuteReader();
                //NULLבנתיים לא בודקים האם אחד השדות הוא 
                while (reader.Read())
                {
                    return reader[0];
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
            return null;
        }

        public void UpdateRating(int tid, int rating, string review)
        {
            // Get current average
            string sqlStr = $"SELECT AVG(Rating) FROM Ratings WHERE TeacherID = {tid}";
            object avgObj = SelectScalar(sqlStr); // you need this helper
            double avg = avgObj != DBNull.Value ? Convert.ToDouble(avgObj) : 0;

            // Get current count
            string sqlStr2 = $"SELECT COUNT(*) FROM Ratings WHERE TeacherID = {tid}";
            object countObj = SelectScalar(sqlStr2); // you need this helper
            int count = countObj != DBNull.Value ? Convert.ToInt32(countObj) : 0;

            // Calculate new average
            double newAvg = (avg * count + rating) / (count + 1);

            // Update teacher rating
            string sqlStr3 = $"UPDATE Teacher SET Rating = {newAvg} WHERE id = {tid}";
            SaveChanges(sqlStr3);

            // Insert new rating and review
            string sqlStr4 = $"INSERT INTO Ratings (teacherID, rating, rewiew) VALUES ({tid}, {rating}, '{review.Replace("'", "''")}')";
            SaveChanges(sqlStr4);
        }

        public void UpdateTeacherId(int sid, int tid)
        {
            string sqlStr = "Update Student Set teacherId=" + tid + " Where id=" + sid;
            SaveChanges(sqlStr);
        }
        public List<string> GetTeacherReviews(int tid)
        {
            string sqlStr = "Select [Rewiew] From Ratings Where teacherID=" + tid;
            List<string> reviews = SelectRewiew(sqlStr);
            return reviews;
        }
        public bool AddStudent(UserInfo user)
        {
            string table = "Student"; 
            string id = user.TeacherId.ToString();
            string sqlstr = "";
            sqlstr = $"INSERT INTO " + table + " ([username], [password], [email], [phone], [teacherId],[Confirmed]) " + "" +
                $"VALUES ('{user.Username}', '{user.Password}','{user.Email}' ,'{user.Phone}', {user.TeacherId}, {false})";
            return SaveChanges(sqlstr) != 0;
        }
        public void TeacherConfirm(int sid, int tid)
        {
            string sqlStr = "Update Student Set Confirmed=1 Where id=" + sid + "and teacherId=" + tid;
            SaveChanges(sqlStr);
        }
        public List<UserInfo> GetTeacherStudents(int tid)
        {
            string sqlStr = "Select * From Student Where teacherId=" + tid;
            List<UserInfo> list = Select(sqlStr).OfType<UserInfo>().ToList();
            return list;
        }
        public bool IsConfirmed(int id)
        {
            string sqlStr = "Select * From Student Where id=" + id;
            List<UserInfo> list = Select(sqlStr).OfType<UserInfo>().ToList();
            if (list.Count == 1)
            { return list[0].Confirmed; }
            else { return false; }
        }
        //public int GetTeacherId(int id)
        //{
        //    string sqlStr = "Select teacherId From Student Where id=" + id;
        //    List<Base> list = Select(sqlStr);
        //    if (list.Count == 1)
        //    { return int.Parse(list[0].Id.ToString()); }
        //    else { return -1; }
        //}
        public int GetUserID(string username, string table)
        {
            string sqlStr = "Select id From " + table + " Where username='" + username + "'";
            List<Base> list = Select(sqlStr);
            if (list.Count == 1)
            { 
                int id = int.Parse(list[0].Id.ToString());
                return id;
            }
            else { return -1; }
        }

        public int GetTeacherId(int sid)
        {
            string sql = "SELECT * FROM Student WHERE id=" + sid;
            List<UserInfo> list = Select(sql).OfType<UserInfo>().ToList();
            if (list.Count == 1)
            { return list[0].TeacherId; }
            else { return 0; }
        }

        public void AddLessonToStudent(int id,string less)
        {
            string sql = "insert into Student (Lessons) Values (',') where id=" + id;
            SaveChanges(sql);
            string all = GetStudentLessons(id)+less;
            sql = "update Student set [lessons] ='," + all+",' where id=" + id;
            SaveChanges(sql);
        }
        public string GetStudentLessons(int id)
        {
            string sql = "SELECT * from Student where id=" + id;
            List<UserInfo> list = Select(sql).OfType<UserInfo>().ToList();
            if (list.Count == 1)
            { return list[0].Lessons; }
            else { return ""; }
        }

    }
}
