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
                    s.Password = reader["pass"].ToString();
                    s.IsAdmin = bool.Parse(reader["isAdmin"].ToString());
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
            List<Base> list = Select("Select * From Users");
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
            sqlstr = $"INSERT INTO " + table + " ([username], [password], [email], [phone], [teacherId]) " + "" +
                $"VALUES ('{user.Username}', '{user.Password}','{user.Email}' ,'{user.Phone}', {user.TeacherId})";
            return SaveChanges(sqlstr) != 0;
        }
    }
}
