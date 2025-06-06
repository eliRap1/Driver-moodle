using Model;
using System;
using System.Collections.Generic;
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
                    //s.IsAdmin = bool.Parse(reader["isAdmin"].ToString());
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
            sqlstr = $"INSERT INTO " + table + " ([username], [password], [email], [phone]) " + "" +
                $"VALUES ('{user.Username}', '{user.Password}','{user.Email}' ,'{user.Phone}')";
            return SaveChanges(sqlstr) != 0;

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
        public void TeacherConfirm(int sid)
        {
            string sqlStr = "Update Student Set Confirmed=1 Where id=" + sid;
            SaveChanges(sqlStr);
        }
        public string GetTeacherUnconfirmed(int tid, int sid)
        {
            string sqlStr = "Select [id] From Student Where id=" + sid +"and Confirmed=0";
            List<Base> list = Select(sqlStr);
            if (list.Count == 1)
            { return list[0].Id.ToString(); }
            else { return null; }
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
        public bool SetTeacherCalendar(Calendar cal, int teacherId)
        {
            string sqlstr = $"INSERT INTO Availability ([TeacherID], [UnavailableDate], [StartTime], [EndTime]) " + "" +
                $"VALUES ({teacherId}, '{cal.UnavailableDates()}', '{cal.StartTime}', '{cal.EndTime}')";
            return SaveChanges(sqlstr) != 0;
        }
        public Calendar GetTeacherCalendar(int teacherId)
        {
            string sqlStr = "Select * From Availability Where TeacherID=" + teacherId;
            List<Base> list = Select(sqlStr);
            if (list.Count == 1)
            { return (Calendar)list[0]; }
            else { return null; }
        }
    }
}
