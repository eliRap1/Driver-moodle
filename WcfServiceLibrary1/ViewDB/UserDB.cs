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
                    s.isAdmin = bool.Parse(reader["isAdmin"].ToString());
                }
                catch
                {
                    Console.WriteLine("No ID in DB");
                }
            }
        }
        public Student GetStudentById(int id)
        {
            string sqlStr = "Select * From Users Where id=" + id;
            List<Base> list = Select(sqlStr);
            if (list.Count == 1)
            { return (Student)list[0]; }
            else { return null; }
        }
        public StudentsList GetAllStudents()
        {
            List<Base> list = Select("Select * From Users");
            StudentsList studs = new StudentsList(list);
            return studs;
        }

        public bool AddStudent(UserInfo user)
        {
            string sqlstr = $"INSERT INTO Users ([name], [password], [isAdmin]) " + "" +
                $"VALUES ('{user.Username}', '{user.Password}', {user.isAdmin})";

            return SaveChanges(sqlstr) != 0;

        }
    }
}
