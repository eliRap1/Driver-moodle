using Model;
using System;
using System.Collections.Generic;
using System.Data;
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
        protected List<UserInfo> Selectu(string sqlCommandTxt)
        {
            List<UserInfo> list = new List<UserInfo>();
            try
            {
                connection.Open(); //was missing
                command.CommandText = sqlCommandTxt;
                reader = command.ExecuteReader();
                //NULLבנתיים לא בודקים האם אחד השדות הוא 
                while (reader.Read())
                {
                    UserInfo entity = new UserInfo(); //יוצר אובייקט מטיפוס המתאים
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
                    UserInfo s = (UserInfo)entity;
                    s.Username = reader["username"].ToString();
                    s.Password = reader["password"].ToString();
                    try
                    {
                        s.Rewiew = reader["Rewiew"].ToString();
                    }
                    catch
                    {

                    }
                    s.Confirmed = bool.Parse(reader["Confirmed"].ToString());
                    s.Email = reader["email"].ToString();
                    s.Phone = reader["phone"].ToString();
                    
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
            List<Base> list = Select(sqlStr);
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
                $"VALUES ('{user.Username}', '{user.Password}','{user.Email}' ,'{user.Phone},{user.Rating}')";
            return SaveChanges(sqlstr) != 0;

        }
        public void UpdateRating(int tid, int rating,string rewiew)
        {
            string sqlStr = "SELECT AVG(rating) FROM Ratings Where teacherID=" + tid;
            List<Base> list = Select(sqlStr);
            double avg = double.Parse(list[0].ToString());
            string sqlStr2 = "SELECT COUNT(*) FROM Ratings Where teacherID=" + tid;
            List<Base> list2 = Select(sqlStr2);
            int count = int.Parse(list2[0].ToString());
            avg = (avg * count + rating) / (count + 1);
            string sqlStr3 = "Update Teacher Set Rating=" + avg + " Where id=" + tid;
            SaveChanges(sqlStr3);
            string sqlStr4 = "INSERT INTO Ratings ([teacherID], [rating], [rewiew]) " + "" +
                $"VALUES ({tid}, {rating},'{rewiew}')";
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
            List<UserInfo> list = Selectu(sqlStr);
            return list;
        }
        public bool IsConfirmed(int id)
        {
            string sqlStr = "Select Confirmed From Student Where id=" + id;
            List<UserInfo> list = Selectu(sqlStr);
            if (list.Count == 1)
            { return list[0].Confirmed; }
            else { return false; }
        }
        public int GetTeacherId(int id)
        {
            string sqlStr = "Select teacherId From Student Where id=" + id;
            List<Base> list = Select(sqlStr);
            if (list.Count == 1)
            { return int.Parse(list[0].Id.ToString()); }
            else { return -1; }
        }
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
        public bool SetTeacherCalendar(Calendars cal, int teacherId)
        {
            string sqlstr = $"INSERT INTO Availability ([TeacherID], [UnavailableDate], [StartTime], [EndTime]) " + "" +
                $"VALUES ({teacherId}, '{cal.GetDatesUnavailable()}', '{cal.StartTime}', '{cal.EndTime}')";
            return SaveChanges(sqlstr) != 0;
        }
        public Calendars GetTeacherCalendar(int teacherId)
        {
            string sqlStr = "Select * From Availability Where TeacherID=" + teacherId;
            List<Base> list = Select(sqlStr);
            if (list.Count == 1)
            { return (Calendars)list[0]; }
            else { return null; }
        }
    }
}
