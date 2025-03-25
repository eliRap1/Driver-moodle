using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Student : Base
    {
        private string fname;
        private int age;
        private string name;
        private City city;
        private int cityId;//להעברת נתונים  משרת ללוקח שיש לו את כל האוביקטים של ערים
        private bool gender;

        public string Fname { get => fname; set => fname = value; }
        public int Age { get => age; set => age = value; }
        public string Name { get => name; set => name = value; }
        public City City { get => city; set => city = value; }
        public int CityId { get => cityId; set => cityId = value; }
        public bool Gender { get => gender; set => gender = value; }
    }
}
