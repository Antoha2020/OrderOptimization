using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public List<Order> myOrders = new List<Order>();

        public bool InWork { get; set; }

        public GMapRoute currentRoute = null;
        double currentDistance = 0;
        public Master(string name, double startLat, double startLon, bool inWork)
        {
            this.Name = name;
            this.StartLat = startLat;
            this.StartLon = startLon;
            this.CurrentLat = startLat;
            this.CurrentLon = startLon;
            this.InWork = inWork;
        }

        public void SetGMapRoute(List<PointLatLng> routePoints, double distance)
        {
            currentRoute = new GMapRoute(routePoints, "Route"+Name);
            currentRoute.IsVisible = true;
            currentRoute.Stroke = new Pen(Color.Blue, 3);

            currentDistance = distance;
        }
    }

    
}
