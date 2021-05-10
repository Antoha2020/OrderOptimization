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
        GMapOverlay routesOverlay = new GMapOverlay("marker");
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

            DBHandler.GetOrders();
            FillCombo1();
            FillMasters();
            GetMyOrders();
            ShowMarkers();
            
            Logger.Log.Info("Form1 загружена");
        }

        private void GetMyOrders()
        {
            for (int j = 0; j < Constants.MASTERS.Count; j++)
            {
                Constants.MASTERS[j].myOrders.Clear();                
            }

            for (int i=0;i<Constants.ORDERS.Count;i++)
            {
                for(int j=0;j<Constants.MASTERS.Count;j++)
                {
                    if(Constants.ORDERS[i].Master.Equals(Constants.MASTERS[j].Name))
                    {
                        Constants.MASTERS[j].myOrders.Add(Constants.ORDERS[i]);
                    }
                }
            }
        }

        private void FillCombo1()
        {
            comboBox1.Items.Clear();
            foreach (Order ord in Constants.ORDERS)
            {
                if (ord.Master.Equals("Нет") && ord.DateOrder.ToShortDateString().Equals(dateTimePicker1.Value.ToShortDateString()))
                    comboBox1.Items.Add(ord.Name);
            }
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
            else
            {
                comboBox1.Text = "";
                ShowMarkers();
            }
        }

        private void FillMasters()
        {
            for (int i = 0; i < masterButtons.Length; i++)
            {
                masterButtons[i].Visible = false;
            }
            panel1.Height = 230;
            groupBox1.Height = 20;
            comboBox2.Items.Add("Нет");
            for (int i=0;i<Constants.MASTERS.Count;i++)
            {
                masterButtons[i].Visible = true;
                masterButtons[i].Text = Constants.MASTERS[i].Name;
                masterCheckBoxes[i].Visible = true;
                panel1.Height += 30;
                groupBox1.Height += 30;

                comboBox2.Items.Add(Constants.MASTERS[i].Name);
            }
            comboBox2.SelectedIndex = 0;
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
       
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            panel3.Visible = true;
            panel4.Visible = false;
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

        private double [] GetCurrentCoord(Master master, DateTime date)
        {
            double[] result = new double[2] { master.StartLat, master.StartLon };
            DateTime begin = new DateTime(2000, 1, 1, 0, 0, 0);
            DateTime end = new DateTime(2000, 1, 1, 0, 0, 0);
            foreach (Order order in master.myOrders)
            {
                if (order.DateOrder.ToShortDateString().Equals(date.ToShortDateString()))
                {
                    DateTime dt = Convert.ToDateTime(order.TimeEnd);
                    if (end.Hour * 60 + end.Minute <= dt.Hour * 60 + dt.Minute)
                    {
                        result[0] = Convert.ToDouble(order.Lat);
                        result[1] = Convert.ToDouble(order.Lon);
                        end = dt;
                    }
                }
            }
            return result;
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
                    break;
                }
            }
            if (order != null)
            {
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(Convert.ToDouble(order.Lat), Convert.ToDouble(order.Lon)), orderMarker[0]);
                marker.ToolTip = new GMapRoundedToolTip(marker);
                marker.ToolTip.Fill = Brushes.White;
                marker.ToolTip.Foreground = Brushes.Black;
                marker.ToolTip.Stroke = Pens.Black;
                marker.ToolTip.TextPadding = new Size(20, 20);
                marker.ToolTip.Font = new Font("Arial", 12);
                marker.ToolTipText = " \n" + order.Name;
                marker.ToolTipMode = MarkerTooltipMode.Always;
                markersOverlay.Markers.Add(marker);
            }
            
            int k = 0;
            foreach (Master master in Constants.MASTERS)
            {
                double[] currCoord = GetCurrentCoord(master, dateTimePicker1.Value);
                GMarkerGoogle markerMaster = new GMarkerGoogle(new PointLatLng(currCoord[0], currCoord[1]), col[k]);
                markerMaster.ToolTip = new GMap.NET.WindowsForms.ToolTips.GMapRoundedToolTip(markerMaster);
                markerMaster.ToolTip.Fill = Brushes.White;
                markerMaster.ToolTip.Foreground = Brushes.Black;
                markerMaster.ToolTip.Stroke = Pens.Black;
                markerMaster.ToolTip.TextPadding = new Size(20, 20);
                markerMaster.ToolTip.Font = new Font("Arial", 12);
                //markerMaster.ToolTip.Offset = GMapRoundedToolTip.DefaultForeground;
                markerMaster.ToolTipMode = MarkerTooltipMode.Always;
                markerMaster.ToolTipText = " \n"+Constants.MASTERS[k++].Name;
                markersOverlay.Markers.Add(markerMaster);
            }
            if (order != null)
                gMapControl1.Position = new PointLatLng(Convert.ToDouble(order.Lat), Convert.ToDouble(order.Lon));
            else
                gMapControl1.Position = new PointLatLng(Convert.ToDouble(Constants.MASTERS[0].CurrentLat), Convert.ToDouble(Constants.MASTERS[0].CurrentLon));
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

                var route = router.Calculate(profile, (float)Convert.ToDouble(order.Lat), (float)Convert.ToDouble(order.Lon),
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
                double res = 0;
                if (radioButton3.Checked)
                {
                    res = GetDistRouteOSM(order, Constants.MASTERS[j]);
                }
                else
                {
                    double[] currCoord = GetCurrentCoord(Constants.MASTERS[j], dateTimePicker1.Value);
                    res = Math.Round(Constants.getDistance(Convert.ToDouble(order.Lat), Convert.ToDouble(order.Lon),
                        currCoord[0], currCoord[1]) / 1000, 3);
                    List<PointLatLng> list = new List<PointLatLng>();
                    list.Add(new PointLatLng(Convert.ToDouble(order.Lat), Convert.ToDouble(order.Lon)));
                    list.Add(new PointLatLng(currCoord[0], currCoord[1]));
                    Constants.MASTERS[j].SetGMapRoute(list, res);
                }
                if (j == 0)
                { button7.Text = Constants.MASTERS[j].Name + ";" + res; }
                if (j == 1)
                { button8.Text = Constants.MASTERS[j].Name + ";" + res; }
                if (j == 2)
                { button9.Text = Constants.MASTERS[j].Name + ";" + res; }
                if (j == 3)
                { button10.Text = Constants.MASTERS[j].Name + ";" + res; }
                if (j == 4)
                { button11.Text = Constants.MASTERS[j].Name + ";" + res; }
                if (j == 5)
                { button12.Text = Constants.MASTERS[j].Name + ";" + res; }
                if (j == 6)
                { button13.Text = Constants.MASTERS[j].Name + ";" + res; }
                if (j == 7)
                { button14.Text = Constants.MASTERS[j].Name + ";" + res; }
                if (j == 8)
                { button15.Text = Constants.MASTERS[j].Name + ";" + res; }
                if (j == 9)
                { button16.Text = Constants.MASTERS[j].Name + ";" + res; }
            }


        }

        
        private void gMapControl1_MouseClick(object sender, MouseEventArgs e)
        {

            if(panel3.Visible)
            {
                textBox13.Text = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat.ToString();
                textBox14.Text = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng.ToString();
                 
            }
            
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            new ManageMasters().ShowDialog();
            FillCombo1();
            FillMasters();
            ShowMarkers();
           // this.Refresh();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            panel3.Visible = false; 
        }

        private void button17_Click(object sender, EventArgs e)
        {
            try
            {
                string name = textBox15.Text;
                string description = textBox10.Text;
                string lat = textBox13.Text;
                string lon = textBox14.Text;
                string city = textBox1.Text;
                string street = textBox2.Text;
                string house = textBox3.Text;
                string flat = textBox5.Text;
                string office = textBox4.Text;
                string porch = textBox6.Text;
                bool intercom = checkBox11.Checked;
                string dateOrder = dateTimePicker2.Value.ToShortDateString();
                string floor = textBox7.Text;
                string timeBeg = dateTimePicker3.Value.ToShortTimeString();
                string timeEnd = dateTimePicker4.Value.ToShortTimeString();
                string phone1 = textBox8.Text;
                string phone2 = textBox9.Text;
                string master = comboBox2.Text;
                if(lat.Length==0 || lon.Length==0)
                {
                    MessageBox.Show("Не заданы координаты заказа!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if(dateTimePicker2.Value.Date<DateTime.Now.Date)
                {
                    MessageBox.Show("Проверьте дату выполнения заказа!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if(name.Length==0)
                {
                    MessageBox.Show("Не указано название заказа!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if((dateTimePicker3.Value.Hour*60+ dateTimePicker3.Value.Minute)> (dateTimePicker4.Value.Hour * 60 + dateTimePicker4.Value.Minute))
                {
                    MessageBox.Show("Неправильно задано время выполнения заказа!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DBHandler.AddOrder(name, description, lat, lon, city, street, house, flat, office, porch, intercom, floor, dateOrder,
                    timeBeg, timeEnd, phone1, phone2, master);
                RefreshTable();
                FillCombo1();
                MessageBox.Show("Заказ " + name + " успешно создан!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {

            }
        }

        private void RefreshTable()
        {
            DBHandler.GetOrders();
            dataGridView1.Rows.Clear();
            for (int i = 0; i < Constants.ORDERS.Count; i++)
            {
                dataGridView1.Rows.Add(Constants.ORDERS[i].Id, 
                    Constants.ORDERS[i].DateCreate.ToShortDateString(),
                    Constants.ORDERS[i].Name, 
                    Constants.ORDERS[i].Phone1,
                    Constants.ORDERS[i].DateOrder.ToShortDateString(),
                    Constants.ORDERS[i].Description,
                    Constants.ORDERS[i].City,
                    Constants.ORDERS[i].Street,
                    Constants.ORDERS[i].House,
                    Constants.ORDERS[i].Flat,
                    Constants.ORDERS[i].Office,
                    Constants.ORDERS[i].Porch,
                    Constants.ORDERS[i].Floor,
                    Constants.ORDERS[i].Intercom,
                    Constants.ORDERS[i].Phone2,
                    Constants.ORDERS[i].TimeBeg,
                    Constants.ORDERS[i].TimeEnd,
                    Constants.ORDERS[i].Lat,
                    Constants.ORDERS[i].Lon,
                    Constants.ORDERS[i].Master
                    );
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            try
            {
                string id = textBox12.Text;
                string name = textBox15.Text;
                string description = textBox10.Text;
                string lat = textBox13.Text;
                string lon = textBox14.Text;
                string city = textBox1.Text;
                string street = textBox2.Text;
                string house = textBox3.Text;
                string flat = textBox5.Text;
                string office = textBox4.Text;
                string porch = textBox6.Text;
                bool intercom = checkBox11.Checked;
                string dateOrder = dateTimePicker2.Value.ToShortDateString();
                string floor = textBox7.Text;
                string timeBeg = dateTimePicker3.Value.ToShortTimeString();
                string timeEnd = dateTimePicker4.Value.ToShortTimeString();
                string phone1 = textBox8.Text;
                string phone2 = textBox9.Text;
                string master = comboBox2.Text;
                bool completed = checkBox12.Checked;
                if (lat.Length == 0 || lon.Length == 0)
                {
                    MessageBox.Show("Не заданы координаты заказа!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (dateTimePicker2.Value.Date < DateTime.Now.Date)
                {
                    MessageBox.Show("Проверьте дату выполнения заказа!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (name.Length == 0)
                {
                    MessageBox.Show("Не указано название заказа!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DBHandler.UpdateOrder(id, name, description, lat, lon, city, street, house, flat, office, porch, intercom, floor, dateOrder,
                    timeBeg, timeEnd, phone1, phone2, master, completed);
                RefreshTable();
                GetMyOrders();
                ShowMarkers();
                FillCombo1();
                if (completed)
                    MessageBox.Show("Заказ " + name + " выполнен!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Информация о заказе " + name + " успешно обновлена!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {

            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            button1.Enabled = radioButton4.Checked ? false : true;
        }

        private void panel3_VisibleChanged(object sender, EventArgs e)
        {
            if(panel3.Visible)
            {
                RefreshTable();
            }
        }
               
        private void button20_Click(object sender, EventArgs e)
        {
            if (textBox12.Text != "")
            {
                if (MessageBox.Show("Вы действительно хотите удалить заказ " + textBox15.Text + "?", "", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    DBHandler.DeleteOrder(textBox12.Text);
                    RefreshTable();
                    GetMyOrders();
                    ShowMarkers();
                    FillCombo1();
                    MessageBox.Show("Заказ " + textBox15.Text + " успешно удален!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    textBox1.Clear();
                    textBox2.Clear();
                    textBox3.Clear();
                    textBox4.Clear();
                    textBox5.Clear();
                    textBox6.Clear();
                    textBox7.Clear();
                    textBox10.Clear();
                    textBox12.Clear();
                    textBox13.Clear();
                    textBox14.Clear();
                    textBox15.Clear();
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            textBox12.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[0].Value.ToString();
            textBox13.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[17].Value.ToString();
            textBox14.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[18].Value.ToString();
            textBox1.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[6].Value.ToString();
            textBox2.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[7].Value.ToString();
            textBox3.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[8].Value.ToString();
            textBox4.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[10].Value.ToString();
            textBox5.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[9].Value.ToString();
            textBox6.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[11].Value.ToString();
            textBox7.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[12].Value.ToString();
            textBox15.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[2].Value.ToString();
            textBox10.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[5].Value.ToString();
            comboBox2.Text = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[19].Value.ToString();
            dateTimePicker3.Value = Convert.ToDateTime(dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[15].Value);
            dateTimePicker4.Value = Convert.ToDateTime(dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[16].Value);
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            FillCombo1();
        }

        private void button24_Click(object sender, EventArgs e)
        {
            panel4.Visible = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenMyOrdersPanel(button7);
        }

        private void OpenMyOrdersPanel(Button btn)
        {
            panel4.Visible = true;
            panel4.Location = new Point(257, 29);
            panel3.Visible = false;
            foreach (Master master in Constants.MASTERS)
            {
                string[] str = btn.Text.Split(new Char[] { ';' });
                if(str[0].Equals(master.Name))
                {
                    label1.Text = master.Name;
                    dateTimePicker5.Value = dateTimePicker1.Value;
                    FillDataGridMyOrder(master);
                    break;
                }
            }
            textBox16.Text = comboBox1.Text;
        }

        private void FillDataGridMyOrder(Master master)
        {
            dataGridView2.Rows.Clear();
            int count = 0;
            foreach(Order order in master.myOrders)
            {
                if (order.DateOrder.ToShortDateString().Equals(dateTimePicker5.Value.ToShortDateString()))
                {
                    dataGridView2.Rows.Add(++count, order.Id, order.Name,
                        order.City + "," + order.Street + "," + order.House, order.TimeBeg, order.TimeEnd);
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            OpenMyOrdersPanel(button8);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            OpenMyOrdersPanel(button9);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            OpenMyOrdersPanel(button10);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            OpenMyOrdersPanel(button11);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            OpenMyOrdersPanel(button12);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            OpenMyOrdersPanel(button13);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            OpenMyOrdersPanel(button14);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            OpenMyOrdersPanel(button15);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            OpenMyOrdersPanel(button16);
        }

        private void dateTimePicker5_ValueChanged(object sender, EventArgs e)
        {
            foreach (Master master in Constants.MASTERS)
            {
                if (label1.Text.Equals(master.Name))
                {
                    FillDataGridMyOrder(master);
                    break;
                }
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            Master master=null;
            foreach(Master mast in Constants.MASTERS)
            {
                if (mast.Name.Equals(label1.Text))
                    master = mast;
            }
            foreach(Order order in Constants.ORDERS)
            {
                if(order.Name.Equals(textBox16.Text) && master!=null)
                {
                    foreach(Order ord in master.myOrders)
                    {
                        if(ord.Id==order.Id)
                        {
                            MessageBox.Show("Этот заказ уже назначен данному мастеру!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    order.DateOrder = dateTimePicker5.Value;
                    order.TimeBeg = dateTimePicker6.Value.ToShortTimeString();
                    order.TimeEnd = dateTimePicker7.Value.ToShortTimeString();
                    order.Master = master.Name;
                    DBHandler.SetMasterOrder(order);
                    master.myOrders.Add(order);
                    FillCombo1();
                    FillDataGridMyOrder(master);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {

        }

        private void button25_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            //for (int i = gMapControl1.Overlays.Count - 1; i >= 0; i--)
            //    gMapControl1.Overlays[i].Clear();
            markersOverlay.Routes.Clear();
            foreach(CheckBox ch in masterCheckBoxes)
            {
                ch.Checked = false;
            }

            for(int i=0;i<Constants.MASTERS.Count;i++)
            {
                masterButtons[i].Text = Constants.MASTERS[i].Name;
            }
            ShowMarkers();
            gMapControl1.Refresh();

            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked && Constants.MASTERS[0].currentRoute != null)
            {
                markersOverlay.Routes.Add(Constants.MASTERS[0].currentRoute);
                gMapControl1.Overlays.Add(markersOverlay);
                gMapControl1.Refresh();
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked && Constants.MASTERS[1].currentRoute != null)
            {
                markersOverlay.Routes.Add(Constants.MASTERS[1].currentRoute);
                gMapControl1.Overlays.Add(markersOverlay);
                gMapControl1.Refresh();
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked && Constants.MASTERS[3].currentRoute != null)
            {
                markersOverlay.Routes.Add(Constants.MASTERS[3].currentRoute);
                gMapControl1.Overlays.Add(markersOverlay);
                gMapControl1.Refresh();
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked && Constants.MASTERS[4].currentRoute != null)
            {
                markersOverlay.Routes.Add(Constants.MASTERS[4].currentRoute);
                gMapControl1.Overlays.Add(markersOverlay);
                gMapControl1.Refresh();
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked && Constants.MASTERS[2].currentRoute != null)
            {
                markersOverlay.Routes.Add(Constants.MASTERS[2].currentRoute);
                gMapControl1.Overlays.Add(markersOverlay);
                gMapControl1.Refresh();
            }
        }

    }
}
