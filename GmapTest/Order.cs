using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmapTest
{
    class Order
    {
        public string Name { set; get; }
        public double lat { set; get; }
        public double lon { set; get; }

        public Order(string Name, double lat, double lon)
        {
            this.Name = Name;
            this.lat = lat;
            this.lon = lon;
        }
    }


}
