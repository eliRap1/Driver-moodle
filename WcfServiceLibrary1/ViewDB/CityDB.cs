using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewDB
{
    public class CityDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new City();
        }
        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    City c = (City)entity;
                    c.CityName = reader["cityName"].ToString();
                }
                catch
                {
                    Console.WriteLine("No ID in DB");
                }
            }
        }
        public City GetCityById(int id)
        {
            string sqlStr = "Select * From Cities Where id=" + id;
            List<Base> list = Select(sqlStr);
            if (list.Count == 1)
            { return (City)list[0]; }
            else { return null; }
        }
        public CitiesList GetAllCities()
        {
            List<Base> list = Select("Select * From Cities");
            CitiesList cities = new CitiesList(list);
            return cities;
        }
    }
}
