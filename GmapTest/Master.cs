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
        public int Id { get;  }
        public string Name { get; set; }
        public double StartLat { get; set; }
        public double StartLon { get; set; }
        public double CurrentLat { get; set; }
        public double CurrentLon { get; set; }

        public List<Order> myOrders = new List<Order>();
        public int currentIndex;
        public bool InWork { get; set; }

        public Dictionary<string, List<PointLatLng>> routeWhole = new Dictionary<string, List<PointLatLng>>();
        public GMapRoute currentRoute = null;
        public string lastTimeEnd = "00:00";
        double currentDistance = 0;
        public Master(int id, string name, double startLat, double startLon, bool inWork)
        {
            Id = id;
            this.Name = name;
            this.StartLat = startLat;
            this.StartLon = startLon;
            this.CurrentLat = startLat;
            this.CurrentLon = startLon;
            this.InWork = inWork;
        }

        public void SetGMapRoute(List<PointLatLng> routePoints, double distance, string id)
        {
            currentRoute = new GMapRoute(routePoints, "Route"+Name);
            currentRoute.IsVisible = true;
            currentRoute.Stroke = new Pen(Color.Blue, 3);

            currentDistance = distance;
            if (routeWhole.ContainsKey(id))
            {
                routeWhole.Remove(id);
                routeWhole.Add(id, routePoints);
            }
            else
            {
                routeWhole.Add(id, routePoints);
            }
        }

        public GMapRoute GetWholeRoute()
        {
            return new GMapRoute(null, "Route" + Name);
        }
    }    
}
