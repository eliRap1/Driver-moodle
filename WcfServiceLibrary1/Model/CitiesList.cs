using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [CollectionDataContract]
    public class CitiesList : List<City>
    {
        public CitiesList() { }
        public CitiesList(IEnumerable<Base> List) : base(List.Cast<City>().ToList()) { }
    }
}