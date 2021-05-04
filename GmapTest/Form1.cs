using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using Itinero;
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using Itinero.Osm.Vehicles;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace GmapTest
{
    public partial class Form1 : Form
    {
        string DirName = "";
        GMapOverlay markersOverlay = new GMapOverlay("marker");
        GMapOverlay markersOverlayStartFin = new GMapOverlay("marker");
        bool StartFinish = false;
        double FirstLat = 0, FirstLng = 0;
        List<PointLatLng> TwoPointDist = new List<PointLatLng>();
        double SumDist = 0;
        int CountPts = 1;

        //----для области-----------------
        GMapPolygon polygonHalfWeek;
        List<PointLatLng> pointsHalfWeek = new List<PointLatLng>();
        //List<Elementary_route> List_ER = new List<Elementary_route>();
        GMapOverlay polyOverlayRoute = new GMapOverlay("polygons");
        GMapOverlay polyOverlayBordersPoly = new GMapOverlay("polygons");
        GMapOverlay polyOverlayBordersPolyChange = new GMapOverlay("polygons");
        GMapOverlay polyOverlayChange = new GMapOverlay("polygons");
       List<Point> ChangeSector = new List<Point>();
        int CountInDisrt = 0;
       
        RouterDb routerDb = new RouterDb();
        Router router;
        GMarkerGoogleType[] col = { GMarkerGoogleType.green_small, GMarkerGoogleType.yellow_small,
                                  GMarkerGoogleType.red_small,GMarkerGoogleType.blue_small,
                                  GMarkerGoogleType.brown_small,GMarkerGoogleType.white_small,
                                  GMarkerGoogleType.purple_small,GMarkerGoogleType.orange_small,
                                  GMarkerGoogleType.gray_small, GMarkerGoogleType.black_small
                                   };
        GMarkerGoogleType[] orderMarker = { GMarkerGoogleType.green_pushpin };
        Button[] masterButtons = new Button[10];
        CheckBox[] masterCheckBoxes = new CheckBox[10];

        public Form1()
        {
            InitializeComponent();
            DBHandler.GetMasters();
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            Logger.Log.Info("Main form start");
            //listRoutes = DBHandler.GetListRoutes();//получение всех маршрутов, которые есть в системе
            //SetComboBox();
            //Constants.MASTERS.Add(new Master("Master1", 54.9244764079647, 37.40639153106871));
            //Constants.MASTERS.Add(new Master("Master2", 56.32703566243706, 38.13250808457901,true));
            //Constants.MASTERS.Add(new Master("Master3", 56.038196635851385, 35.95467164156729));
            //Constants.MASTERS.Add(new Master("Master4", 55.754612213242595, 37.70177576322457));
            //Constants.MASTERS.Add(new Master("Master5", 55.887615230803846, 37.51453353697321));
            //Constants.MASTERS.Add(new Master("Master6", 55.80616204541146, 37.93649273958267));

            Constants.ORDERS.Add(new Order("Заказ1", 55.71421256993413, 37.67865790699016));
            Constants.ORDERS.Add(new Order("Заказ2", 55.50371364250953, 36.042567403537504));
            Constants.ORDERS.Add(new Order("Заказ3", 55.809221701400816, 38.989111949671305));

            masterButtons[0] = button7;
            masterButtons[1] = button8;
            masterButtons[2] = button9;
            masterButtons[3] = button10;
            masterButtons[4] = button11;
            masterButtons[5] = button12;
            masterButtons[6] = button13;
            masterButtons[7] = button14;
            masterButtons[8] = button15;
            masterButtons[9] = button16;

            masterCheckBoxes[0] = checkBox1;
            masterCheckBoxes[1] = checkBox2;
            masterCheckBoxes[2] = checkBox3;
            masterCheckBoxes[3] = checkBox4;
            masterCheckBoxes[4] = checkBox5;
            masterCheckBoxes[5] = checkBox6;
            masterCheckBoxes[6] = checkBox7;
            masterCheckBoxes[7] = checkBox8;
            masterCheckBoxes[8] = checkBox9;
            masterCheckBoxes[9] = checkBox10;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //gMapControl1.MapProvider = GMap.NET.MapProviders.GMapProviders.OpenStreetMap;
            //GMaps.Instance.Mode = AccessMode.ServerOnly;
            gMapControl1.Position = new PointLatLng(55.75006997032796, 37.62540103074475);
            gMapControl1.ShowTileGridLines = false;
            gMapControl1.ShowCenter = false;
            gMapControl1.DragButton = MouseButtons.Left;

            FillCombo1();
            FillMasters();
            ShowMarkers();
            Logger.Log.Info("Form1 загружена");
        }

        private void FillCombo1()
        {
            foreach (Order ord in Constants.ORDERS)
            {
                comboBox1.Items.Add(ord.Name);
            }
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void FillMasters()
        {
            panel1.Height = 182;
            groupBox1.Height = 20;
            for (int i=0;i<Constants.MASTERS.Count;i++)
            {
                masterButtons[i].Visible = true;
                masterButtons[i].Text = Constants.MASTERS[i].Name;
                masterCheckBoxes[i].Visible = true;
                panel1.Height += 30;
                groupBox1.Height += 30;
            }           
        }

        private void googleMapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logger.Log.Info("Выбрано Google карту");
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            gMapControl1.Refresh();
        }

        private void спутниковаяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logger.Log.Info("Выбрано спутниковую Google карту");
            gMapControl1.MapProvider = GoogleSatelliteMapProvider.Instance;
            gMapControl1.Refresh();
        }

        private void openStreetMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logger.Log.Info("Выбрано OSM карту");
            gMapControl1.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            gMapControl1.Refresh();
        }

        private void другаяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMap.NET.MapProviders.YandexMapProvider.Instance;
            gMapControl1.Refresh();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            gMapControl1.Zoom = trackBar1.Value;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //toolStripButton2.Checked = false;
            //if (toolStripButton1.Checked)
            //{
            //    panel1.Visible = true;
            //    panel2.Visible = false;

            //}
            //else
            //    panel1.Visible = false;

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            panel3.Visible = true;

        }

        /// <summary>
        /// Загрузка картографии
        /// </summary>

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                gMapControl1.Cursor = Cursors.WaitCursor;
                using (var stream = File.OpenRead("Branches/MOSCOW.pbf"))
                {
                    routerDb.LoadOsmData(stream, Vehicle.Car);
                }
                //routerDb.AddContracted(Vehicle.Car.Fastest());
                router = new Router(routerDb);
                gMapControl1.Cursor = Cursors.Default;
                button6.Enabled = true;
            }
            catch
            {
                MessageBox.Show("Отсутствует файл Branches/MOSCOW.pbf!", "Ошибка!!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowMarkers()
        {
            for (int r = markersOverlay.Markers.Count - 1; r >= 0; r--)
                markersOverlay.Markers.RemoveAt(r);
            //var selectedOrder = Constants.ORDERS.Where(u => u.Name.Equals(comboBox1.Text));
            Order order = null;
            foreach (Order ord in Constants.ORDERS)
            {
                if (ord.Name.Equals(comboBox1.Text))
                {
                    order = ord;
                }
            }
            if (order != null)
            {
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(order.lat, order.lon), orderMarker[0]);
                marker.ToolTip = new GMapRoundedToolTip(marker);
                marker.ToolTipText = order.Name;
                markersOverlay.Markers.Add(marker);
            }
            int k = 0;
            foreach (Master master in Constants.MASTERS)
            {
                GMarkerGoogle markerMaster = new GMarkerGoogle(new PointLatLng(master.StartLat, master.StartLon), col[k]);
                markerMaster.ToolTip = new GMapBaloonToolTip(markerMaster);
                markerMaster.ToolTipText = Constants.MASTERS[k++].Name;
                markersOverlay.Markers.Add(markerMaster);
            }
            gMapControl1.Position = new PointLatLng(order.lat, order.lon);
            gMapControl1.Overlays.Add(markersOverlay);
            gMapControl1.Refresh();
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowMarkers();
        }

        private void gMapControl1_OnMapZoomChanged()
        {
            trackBar1.Value = (int)gMapControl1.Zoom;
        }

        private double GetDistRouteOSM(Order order, Master master)//расчет расстояния из OSM файла
        {
            List<PointLatLng> list = new List<PointLatLng>();
            double dist = 0;
            try
            {
                var profile = Vehicle.Car.Fastest();

                var route = router.Calculate(profile, (float)order.lat, (float)order.lon,
                    (float)master.CurrentLat, (float)master.CurrentLon);
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
                dist = Math.Round(dist / 1000, 3);
                master.SetGMapRoute(list, dist);
            }
            catch
            {
                return 0;
            }

            return dist;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            for (int j = 0; j < Constants.MASTERS.Count; j++)
            {
                Order order = null;
                for (int i = 0; i < Constants.ORDERS.Count; i++)
                {
                    if (Constants.ORDERS[i].Name.Equals(comboBox1.Text))
                        order = Constants.ORDERS[i];
                }

                double res = GetDistRouteOSM(order, Constants.MASTERS[j]);
                if (j == 0)
                { button7.Text = Constants.MASTERS[j].Name + "  " + res; }
                if (j == 1)
                { button8.Text = Constants.MASTERS[j].Name + "  " + res; }
                if (j == 2)
                { button9.Text = Constants.MASTERS[j].Name + "  " + res; }
                if (j == 3)
                { button10.Text = Constants.MASTERS[j].Name + "  " + res; }
                if (j == 4)
                { button12.Text = Constants.MASTERS[j].Name + "  " + res; }
                if (j == 5)
                { button11.Text = Constants.MASTERS[j].Name + "  " + res; }

            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            markersOverlay.Routes.Add(Constants.MASTERS[0].currentRoute);
            gMapControl1.Overlays.Add(markersOverlay);
            gMapControl1.Refresh();
        }







        private void gMapControl1_MouseClick(object sender, MouseEventArgs e)
        {
            //определение расстояния по прямой

            List<double> xp = new List<double>();
            List<double> yp = new List<double>();

            if (toolStripButton6.Checked)
            {
                GMarkerGoogle marker;
                double Lat = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat;
                double Lng = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng;
                if (e.Button == MouseButtons.Right)
                {

                    if (!StartFinish)
                    {
                        FirstLat = Lat;
                        FirstLng = Lng;
                        marker = new GMarkerGoogle(new PointLatLng(FirstLat, FirstLng), GMarkerGoogleType.blue_pushpin);

                        marker.ToolTip = new GMapRoundedToolTip(marker);
                        marker.ToolTipText = "Точка " + CountPts.ToString() + ":\n" + Math.Round(FirstLat, 5).ToString() +
                            "  " + Math.Round(FirstLng, 5).ToString() + "\nРасстояние: " + SumDist.ToString() + " км";
                        markersOverlayStartFin.Markers.Add(marker);
                        TwoPointDist.Add(new PointLatLng(FirstLat, FirstLng));
                        StartFinish = true;
                    }
                    else
                    {
                        marker = new GMarkerGoogle(new PointLatLng(Lat, Lng), GMarkerGoogleType.pink_pushpin);
                        marker.ToolTip = new GMapRoundedToolTip(marker);

                        SumDist += Math.Round(Constants.getDistance(FirstLat, FirstLng, Lat, Lng) / 1000, 3);
                        marker.ToolTipText = "Точка " + (++CountPts).ToString() + ":\n" + Math.Round(Lat, 5).ToString() +
                            "  " + Math.Round(Lng, 5).ToString() + "\nРасстояние: " + SumDist.ToString() + " км";

                        markersOverlayStartFin.Markers.Add(marker);

                        TwoPointDist.Add(new PointLatLng(Lat, Lng));
                        GMapRoute r = new GMapRoute(TwoPointDist, "Route");
                        r.IsVisible = true;
                        r.Stroke = new Pen(Color.Blue, 2);
                        markersOverlayStartFin.Routes.Add(r);

                        FirstLat = Lat;
                        FirstLng = Lng;
                    }
                }
                if (e.Button == MouseButtons.Left && (Control.ModifierKeys & Keys.Shift) == Keys.Shift && StartFinish)
                {
                    marker = new GMarkerGoogle(new PointLatLng(Lat, Lng), GMarkerGoogleType.blue_pushpin);
                    marker.ToolTip = new GMapRoundedToolTip(marker);

                    SumDist += Math.Round(Constants.getDistance(FirstLat, FirstLng, Lat, Lng) / 1000, 3);
                    marker.ToolTipText = "Точка " + (++CountPts).ToString() + ":\n" + Math.Round(Lat, 5).ToString() +
                        "  " + Math.Round(Lng, 5).ToString() + "\nРасстояние: " + SumDist.ToString() + " км";

                    markersOverlayStartFin.Markers.Add(marker);

                    TwoPointDist.Add(new PointLatLng(Lat, Lng));
                    GMapRoute r = new GMapRoute(TwoPointDist, "Route");
                    r.IsVisible = true;
                    r.Stroke = new Pen(Color.Blue, 2);
                    markersOverlayStartFin.Routes.Add(r);
                    SumDist = 0;

                    StartFinish = false;
                    TwoPointDist.Clear();
                    CountPts = 1;
                }

                gMapControl1.Overlays.Add(markersOverlayStartFin);
                gMapControl1.Zoom += 0.1;
                gMapControl1.Zoom -= 0.1;
            }
            else
            {

                if (e.Button == MouseButtons.Right && (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    if (ChangeSector.Count > 0)
                    {
                        for (int i = polyOverlayChange.Polygons.Count - 1; i >= 0; i--)
                            polyOverlayChange.Polygons.RemoveAt(i);
                        for (int i = polyOverlayBordersPolyChange.Polygons.Count - 1; i >= 0; i--)
                            polyOverlayBordersPolyChange.Polygons.RemoveAt(i);
                        for (int i = ChangeSector.Count - 1; i >= 0; i--)
                            ChangeSector.RemoveAt(i);
                    }
                    pointsHalfWeek.Add(new PointLatLng(gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat, gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng));
                    if (pointsHalfWeek.Count > 1)
                    {
                        List<PointLatLng> TwoPoints = new List<PointLatLng>();
                        TwoPoints.Add(pointsHalfWeek[pointsHalfWeek.Count - 2]);
                        TwoPoints.Add(pointsHalfWeek[pointsHalfWeek.Count - 1]);

                        GMapPolygon polygon1 = new GMapPolygon(TwoPoints, "mypolygon");
                        polygon1.Fill = new SolidBrush(Color.FromArgb(50, 105, 105, 105));
                        polygon1.Stroke = new Pen(Color.FromArgb(105, 105, 105), 1);
                        polyOverlayBordersPolyChange.Polygons.Add(polygon1);
                        gMapControl1.Overlays.Add(polyOverlayBordersPolyChange);

                    }
                }

                if (e.Button == MouseButtons.Right && (Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    polygonHalfWeek = new GMapPolygon(pointsHalfWeek, "mypolygon");
                    //GMapOverlay polyOverlayChange = new GMapOverlay("polygons");
                    polygonHalfWeek.Fill = new SolidBrush(Color.FromArgb(10, 0, 255, 0));
                    polygonHalfWeek.Stroke = new Pen(Color.FromArgb(255, 0, 255, 0), 2);
                    polyOverlayChange.Polygons.Add(polygonHalfWeek);
                    gMapControl1.Overlays.Add(polyOverlayChange);

                    for (int i = 0; i < pointsHalfWeek.Count; i++)
                    {
                        xp.Add(pointsHalfWeek[i].Lat);
                        yp.Add(pointsHalfWeek[i].Lng);
                    }
                   // InsidePolygonChangeSector(xp, yp);
                    NewChangeSector();

                    label11.Text = CountInDisrt.ToString();
                    CountInDisrt = 0;
                    for (int i = pointsHalfWeek.Count - 1; i >= 0; i--)
                        pointsHalfWeek.RemoveAt(i);

                }
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            new ManageMasters().ShowDialog();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            panel3.Visible = false; 
        }

        private void button17_Click(object sender, EventArgs e)
        {
            panel3.Visible = false;
        }

        private void NewChangeSector()
        {
            //ChangeSector.Reverse();
            //for (int i = 0; i < ChangeSector.Count; i++)
            //    for (int j = i + 1; j < ChangeSector.Count; j++)
            //    {
            //        if (ChangeSector[i].CodeTradePoint == ChangeSector[j].CodeTradePoint)
            //        {
            //            ChangeSector.RemoveAt(j);
            //            j--;
            //        }
            //    }
        }

        //public void InsidePolygonChangeSector(List<double> xp, List<double> yp)
        //{
        //    try
        //    {
        //        foreach (Point TrPoint in TradePoints)
        //        {
        //            int intersections_num = 0;
        //            int prev = xp.Count - 1;
        //            bool prev_under = yp[prev] < TrPoint.Y;

        //            for (int i = 0; i < xp.Count; ++i)
        //            {
        //                bool cur_under = yp[i] < TrPoint.Y;

        //                double ax = xp[prev] - TrPoint.X;
        //                double ay = yp[prev] - TrPoint.Y;

        //                double bx = xp[i] - TrPoint.X;
        //                double by = yp[i] - TrPoint.Y;

        //                double t = (ax * (by - ay) - ay * (bx - ax));
        //                if (cur_under && !prev_under)
        //                {
        //                    if (t > 0)
        //                        intersections_num += 1;
        //                }
        //                if (!cur_under && prev_under)
        //                {
        //                    if (t < 0)
        //                        intersections_num += 1;
        //                }

        //                prev = i;
        //                prev_under = cur_under;
        //            }

        //            if (intersections_num % 2 == 1)
        //            {
        //                bool IsDouble = false;
        //                for (int i = 0; i < ChangeSector.Count; i++)
        //                {
        //                    if (ChangeSector[i].CodeTradePoint == TrPoint.CodeTradePoint)
        //                    {
        //                        IsDouble = true;
        //                        break;
        //                    }
        //                }
        //                if (!IsDouble)
        //                    CountInDisrt++;

        //                ChangeSector.Add(TrPoint);//содержит точки из выделенной области
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        return;
        //    }
        //}

    }
}
