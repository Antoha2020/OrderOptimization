using GMap.NET;
using GMap.NET.WindowsForms;
using Itinero;
using Itinero.Osm.Vehicles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public int Id { get; }
        public string Name { get; set; }
        public double StartLat { get; set; }
        public double StartLon { get; set; }
        
        public List<Order> myOrders = new List<Order>();
        public int currentIndex;
        public bool InWork { get; set; }

        private Color[] color = {Color.Green, Color.Yellow, Color.Red, Color.Blue, Color.Orange, Color.LightBlue, Color.Pink, Color.Purple,
        Color.Green, Color.Yellow};

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
            this.InWork = inWork;
        }

        private List<Order> SelectOrders(DateTime dt)
        {
            List<Order> selectedOrders = new List<Order>();
            Dictionary<int, Order> dic = new Dictionary<int, Order>();
            //List<Order> sortedOrders = new List<Order>();
            foreach (Order order in myOrders)
            {
                if (order.DateOrder.ToShortDateString().Equals(dt.ToShortDateString()))
                {
                    string[] time = order.TimeBeg.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    int key = Convert.ToInt32(time[0]) * 60 + Convert.ToInt32(time[1]);
                    // selectedOrders.Add(order);
                    if (!dic.ContainsKey(key))
                        dic.Add(key, order);

                }
            }

            var sortedDict = new SortedDictionary<int, Order>(dic);
            foreach (var kvp in sortedDict)
                selectedOrders.Add(kvp.Value);
                //Console.WriteLine("Key: " + kvp.Key + "; Value: " + kvp.Value);
            
            return selectedOrders;
        }
        
        internal double GetDistRouteOSM1(Router router, DateTime selectedDate)//расчет расстояния из OSM файла
        {
            List<Order> listOrders = SelectOrders(selectedDate);
            List <PointLatLng> list = new List<PointLatLng>();
            list.Add(new GMap.NET.PointLatLng(StartLat, StartLon));
            double dist = 0;
            try
            {
                // double[] currCoord = GetCurrentCoord(master, dateTimePicker1.Value);
                double fromLat = StartLat;
                double fromLon = StartLon;
                 
                for (int i = 0; i < listOrders.Count; i++)
                {
                    if (router != null)
                    {
                        try
                        {
                            var profile = Vehicle.Car.Fastest();

                            var route = router.Calculate(profile, (float)fromLat, (float)fromLon,
                                (float)Convert.ToDouble(listOrders[i].Lat), (float)Convert.ToDouble(listOrders[i].Lon));
                            var routeGeoJson = route.ToGeoJson();

                            JObject CoordinateSearch = JObject.Parse(routeGeoJson.ToString());
                            IList<JToken> results = CoordinateSearch["features"].Children().ToList();
                            IList<SearchResult> searchResults = new List<SearchResult>();

                            SearchResult searchResult;
                            foreach (JToken result in results)
                            {
                                if (!result.ToString().Contains("\"type\": \"Point\""))
                                {
                                    searchResult = JsonConvert.DeserializeObject<SearchResult>(result.ToString());
                                    searchResults.Add(searchResult);
                                    for (int d = 0; d < searchResult.geometry.coordinates.Length / 2; d++)
                                        list.Add(new GMap.NET.PointLatLng(searchResult.geometry.coordinates[d, 1], searchResult.geometry.coordinates[d, 0]));
                                    dist = Math.Round(Convert.ToDouble(searchResult.properties.distance), 3);
                                }
                            }

                            if (dist == 0)
                                list.Add(new GMap.NET.PointLatLng(Convert.ToDouble(listOrders[i].Lat), Convert.ToDouble(listOrders[i].Lon)));
                        }
                        catch
                        {
                            list.Add(new GMap.NET.PointLatLng(Convert.ToDouble(listOrders[i].Lat), Convert.ToDouble(listOrders[i].Lon)));
                        }
                    }
                    else
                    {
                        list.Add(new GMap.NET.PointLatLng(Convert.ToDouble(listOrders[i].Lat), Convert.ToDouble(listOrders[i].Lon)));
                    }
                    fromLat = Convert.ToDouble(listOrders[i].Lat);
                    fromLon = Convert.ToDouble(listOrders[i].Lon);
                }
                //dist = Math.Round(dist / 1000, 3);
                //master.SetGMapRoute(list, dist, order.Id);
                currentRoute = new GMapRoute(list, "Route" + Name);
                currentRoute.IsVisible = true;
                currentRoute.Stroke = new Pen(color[Id], 3);
            }
            catch
            {
                return 0;
            }

            return dist;
        }

        //internal void GetDistRouteOSM(Router router, DateTime value)
        //{
        //    throw new NotImplementedException();
        //}
    }    
}
