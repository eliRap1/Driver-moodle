using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewDB;

namespace BusinessLogic
{
    public class AllStudentsListAndLogics
    {

        public static void AddStudent(Student student)
        {
            new StudentDB().AddStudent(student);
        }

        public static StudentsList GetAllStudents()
        {
                return new StudentDB().GetAllStudents();
        }

    }
}
