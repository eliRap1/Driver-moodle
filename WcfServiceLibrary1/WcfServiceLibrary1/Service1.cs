using Model;
using BusinessLogic;
using ViewDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfServiceLibrary1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class Service1 : IService1
    {
        public bool AddStudent(Student student)
        {
            //AllStudentsListAndLogics.AddStudent(student);
            return new StudentDB().AddStudent(student);
         
        }

        public StudentsList GetAllStudents()
        {
            return new StudentDB().GetAllStudents();
        }
        public CitiesList GetAllCities()
        {
            return AllCitiesListAnd_Logics.GetAllCities();
        }
        public City GetCityById(int id)
        {
            return new CityDB().GetCityById(id);
        }
    }
}
