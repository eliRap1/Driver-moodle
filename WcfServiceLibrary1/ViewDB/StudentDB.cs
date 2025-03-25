using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewDB
{
    public class StudentDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new Student();
        }
        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    Student s = (Student)entity;
                    s.Age = int.Parse(reader["Age"].ToString());
                    s.Fname = reader["lastName"].ToString();
                    s.Name = reader["name"].ToString(); 
                    s.CityId = int.Parse(reader["cityId"].ToString()); 
                    s.Gender = bool.Parse(reader["gender"].ToString());
                }
                catch
                {
                    Console.WriteLine("No ID in DB");
                }
            }
        }
        public Student GetStudentById(int id)
        {
            string sqlStr = "Select * From Studets Where id=" + id;
            List<Base> list = Select(sqlStr);
            if (list.Count == 1)
            { return (Student)list[0]; }
            else { return null; }
        }
        public StudentsList GetAllStudents()
        {
            List<Base> list = Select("Select * From Studets");
            StudentsList studs = new StudentsList(list);
            return studs;
        }

        public bool AddStudent(Student student)
        {
            string sqlstr = $"INSERT INTO Studets ([name], [lastName], [Age],[cityId],[gender]) "+"" +
                $"VALUES ('{student.Name}', '{student.Fname}', {student.Age},{student.CityId},{student.Gender})";
            //INSERT INTO table_name (column1, column2, column3, ...)
            //VALUES(value1, value2, value3, ...);

            return SaveChanges(sqlstr) != 0;

        }
        
    }

}
