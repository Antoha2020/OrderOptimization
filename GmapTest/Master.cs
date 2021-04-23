using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmapTest
{
    class Master
    {
        public string Name { get; set; }
        public double StartLat { get; set; }
        public double StartLon { get; set; }
        public double CurrentLat { get; set; }
        public double CurrentLon { get; set; }

        public Master(string name, double startLat, double startLon)
        {
            this.Name = name;
            this.StartLat = startLat;
            this.StartLon = startLon;
            this.CurrentLat = startLat;
            this.CurrentLon = startLon;
        }
    }

    
}
