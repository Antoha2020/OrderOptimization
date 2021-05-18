using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.StyledXmlParser.Jsoup.Nodes;
using Itinero;
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using Itinero.Osm.Vehicles;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Document = iText.Layout.Document;

namespace GmapTest
{
    public partial class Form1 : Form
    {
        GMapOverlay markersOverlay = new GMapOverlay("marker");
        public RouterDb routerDb = new RouterDb();
        public Router router;
        GMarkerGoogleType[] col = { GMarkerGoogleType.green_dot, GMarkerGoogleType.yellow_dot,
                                  GMarkerGoogleType.red_dot,GMarkerGoogleType.blue_dot,
                                  GMarkerGoogleType.orange_dot,GMarkerGoogleType.lightblue_dot,
                                  GMarkerGoogleType.pink_dot,GMarkerGoogleType.purple_dot,
                                  GMarkerGoogleType.green_dot, GMarkerGoogleType.yellow_dot
                                   };
        GMarkerGoogleType[] orderMarker = { GMarkerGoogleType.green_pushpin, GMarkerGoogleType.red_pushpin, GMarkerGoogleType.yellow_pushpin };
        Button[] masterButtons = new Button[10];
        CheckBox[] masterCheckBoxes = new CheckBox[10];
        Button[] routeList = new Button[10];

        public Form1()
        {
            InitializeComponent();
            if(!Constants.ReadConfig())
            {
                MessageBox.Show("Ошибка чтения конфигурационного файла!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Process.GetCurrentProcess().Kill(); //Application.Exit();
            }
            DBHandlerMySQL.GetMasters();
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

            routeList[0] = button4;
            routeList[1] = button5;
            routeList[2] = button22;
            routeList[3] = button23;
            routeList[4] = button24;
            routeList[5] = button25;
            routeList[6] = button26;
            routeList[7] = button27;
            routeList[8] = button28;
            routeList[9] = button29;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //gMapControl1.MapProvider = GMap.NET.MapProviders.GMapProviders.OpenStreetMap;
            //GMaps.Instance.Mode = AccessMode.ServerOnly;
            gMapControl1.Position = new PointLatLng(55.75006997032796, 37.62540103074475);
            gMapControl1.ShowTileGridLines = false;
            gMapControl1.ShowCenter = false;
            gMapControl1.DragButton = MouseButtons.Left;

            DBHandlerMySQL.GetOrders();
            FillCombo1();
            FillMasters();
            GetMyOrders();
            ShowMarkers();
            ShowRoutes();

            Logger.Log.Info("Form1 загружена");
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
                button1.Enabled = false;

            }
            catch
            {
                MessageBox.Show("Отсутствует файл Branches/MOSCOW.pbf!", "Ошибка!!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Изменение даты в основном окне
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            FillCombo1();
            toolStripButton5_Click(sender, e);
            ShowRoutes();
            ShowMarkers();
        }

        /// <summary>
        /// Поиск координат по адресу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            TestRequestAsync();
        }

        private async System.Threading.Tasks.Task TestRequestAsync()
        {
            this.Cursor = Cursors.WaitCursor;
            WebRequest request = WebRequest.Create("http://search.maps.sputnik.ru/search?q=" + textBox17.Text);// Москва, Тверская улица 13");
            WebResponse response = await request.GetResponseAsync();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    //Console.WriteLine(reader.ReadToEnd());
                    try
                    {
                        string s = reader.ReadToEnd();
                        JObject CoordinateSearch = JObject.Parse(s.ToString());
                        IList<JToken> results = CoordinateSearch["result"].Children().ToList();
                        JObject CoordinateSearch1 = JObject.Parse(results[0].ToString());
                        IList<JToken> results1 = CoordinateSearch1["position"].Children().ToList();

                        for (int i = 0; i < 2; i++)
                        {
                            if (results1[i].ToString().Contains("lat"))
                            {
                                string[] str = results1[i].ToString().Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                textBox19.Text = str[1];
                            }
                            if (results1[i].ToString().Contains("lon"))
                            {
                                string[] str = results1[i].ToString().Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                textBox20.Text = str[1];
                            }
                        }
                    }
                    catch
                    {
                        textBox19.Clear();
                        textBox20.Clear();
                        MessageBox.Show("Координаты не найдены!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    this.Cursor = Cursors.Default;
                }
            }
            response.Close();
        }

        /// <summary>
        /// Показать маркер с найденными координатами на карте
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                //toolStripButton5_Click(sender, e);
                double lat = Convert.ToDouble(textBox19.Text.Replace('.', ','));
                double lon = Convert.ToDouble(textBox20.Text.Replace('.', ','));
                GMarkerGoogle testMarker = new GMarkerGoogle(new PointLatLng(lat, lon), GMarkerGoogleType.green_big_go);
                gMapControl1.Position = new PointLatLng(lat, lon);
                markersOverlay.Markers.Add(testMarker);
                //gMapControl1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте правильность ввода координат!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Добавить заказ в систему
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                string name = textBox11.Text;
                string lat = textBox19.Text;
                string lon = textBox20.Text;
                string timeBeg = maskedTextBox1.Text;
                DateTime dateOrder = dateTimePicker1.Value;//.ToShortDateString();

                if (lat.Length == 0 || lon.Length == 0)
                {
                    MessageBox.Show("Не заданы координаты заказа!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    Convert.ToDouble(lat.Replace('.', ','));
                    Convert.ToDouble(lon.Replace('.', ','));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка в координатах заказа!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (dateTimePicker1.Value.Date < DateTime.Now.Date)
                {
                    MessageBox.Show("Проверьте дату выполнения заказа!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (name.Length == 0)
                {
                    MessageBox.Show("Не указано название заказа!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!CheckTimeBeg())
                {
                    MessageBox.Show("Ошибка формата времени начала работы!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DBHandlerMySQL.AddOrder(name, "", lat, lon, "", "", "", "", "", "", false, "", dateOrder,
                    timeBeg, "", "", "Нет");
                DBHandlerMySQL.GetOrders();
                FillCombo1();

                textBox11.Clear();
                maskedTextBox1.Clear();
                textBox17.Clear();
                textBox19.Clear();
                textBox20.Clear();
                //MessageBox.Show("Заказ " + name + " успешно создан!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Карта Google
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void googleMapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logger.Log.Info("Выбрано Google карту");
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            gMapControl1.Refresh();
        }

        /// <summary>
        /// Спутниковая карта
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void спутниковаяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logger.Log.Info("Выбрано спутниковую Google карту");
            gMapControl1.MapProvider = GoogleSatelliteMapProvider.Instance;
            gMapControl1.Refresh();
        }

        /// <summary>
        /// OSM карта
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openStreetMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logger.Log.Info("Выбрано OSM карту");
            gMapControl1.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            gMapControl1.Refresh();
        }

        /// <summary>
        /// Яндекс карта
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void другаяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMap.NET.MapProviders.YandexMapProvider.Instance;
            gMapControl1.Refresh();
        }

        /// <summary>
        /// Бегунок для изменения масштаба карты
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            gMapControl1.Zoom = trackBar1.Value;
        }

        /// <summary>
        /// Обновление заказов, назначенных мастерам
        /// </summary>
        private void GetMyOrders()
        {
            for (int j = 0; j < Constants.MASTERS.Count; j++)
            {
                Constants.MASTERS[j].myOrders.Clear();
            }

            for (int i = 0; i < Constants.ORDERS.Count; i++)
            {
                for (int j = 0; j < Constants.MASTERS.Count; j++)
                {
                    if (Constants.ORDERS[i].Master.Equals(Constants.MASTERS[j].Name))
                    {
                        Constants.MASTERS[j].myOrders.Add(Constants.ORDERS[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Заполнение выпадающего списка нераспределенных заказов
        /// </summary>
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
                ShowRoutes();
            }
        }

        /// <summary>
        /// Открытие кнопок мастеров в зависимости от того сколько их в БД
        /// </summary>
        private void FillMasters()
        {
            for (int i = 0; i < masterButtons.Length; i++)
            {
                masterButtons[i].Visible = false;
            }
            panel1.Height = 300;
            groupBox1.Height = 20;
            for (int i = 0; i < Constants.MASTERS.Count; i++)
            {
                masterButtons[i].Visible = true;
                masterButtons[i].Text = Constants.MASTERS[i].Name;
                routeList[i].Visible = true;
                //masterCheckBoxes[i].Visible = true;
                panel1.Height += 30;
                groupBox1.Height += 30;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            new ManageOrders().ShowDialog();
            GetMyOrders();
            toolStripButton5_Click(sender, e);
            ShowMarkers();
            ShowRoutes();
            FillCombo1();
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
                else
                {
                    if (ord.DateOrder.ToShortDateString().Equals(dateTimePicker1.Value.ToShortDateString()))
                    {
                        GMarkerGoogleType mark = orderMarker[0];
                        if (ord.Master.Equals("Нет"))
                            mark = orderMarker[2];
                        GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(Convert.ToDouble(ord.Lat), Convert.ToDouble(ord.Lon)), mark);
                        marker.ToolTip = new GMapRoundedToolTip(marker);
                        marker.ToolTip.Fill = Brushes.White;
                        marker.ToolTip.Foreground = Brushes.Black;
                        marker.ToolTip.Stroke = Pens.Black;
                        marker.ToolTip.TextPadding = new Size(10, 10);
                        marker.ToolTip.Font = new Font("Arial", 11);
                        marker.ToolTipText = " \n" + ord.Name + "\n" + ord.TimeBeg;
                        marker.ToolTipMode = MarkerTooltipMode.Always;
                        markersOverlay.Markers.Add(marker);
                    }
                }
            }
            if (order != null)
            {
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(Convert.ToDouble(order.Lat), Convert.ToDouble(order.Lon)), orderMarker[1]);
                marker.ToolTip = new GMapRoundedToolTip(marker);
                marker.ToolTip.Fill = Brushes.White;
                marker.ToolTip.Foreground = Brushes.Black;
                marker.ToolTip.Stroke = Pens.Black;
                marker.ToolTip.TextPadding = new Size(10, 10);
                marker.ToolTip.Font = new Font("Arial", 11);
                marker.ToolTipText = " \n" + order.Name + "\n" + order.TimeBeg;
                marker.ToolTipMode = MarkerTooltipMode.Always;
                markersOverlay.Markers.Add(marker);
            }

            int k = 0;
            foreach (Master master in Constants.MASTERS)
            {
                //double[] currCoord = GetCurrentCoord(master, dateTimePicker1.Value);
                GMarkerGoogle markerMaster = new GMarkerGoogle(new PointLatLng(master.StartLat, master.StartLon), col[k]);
                markerMaster.ToolTip = new GMap.NET.WindowsForms.ToolTips.GMapRoundedToolTip(markerMaster);
                markerMaster.ToolTip.Fill = Brushes.White;
                markerMaster.ToolTip.Foreground = Brushes.Black;
                markerMaster.ToolTip.Stroke = Pens.Black;
                markerMaster.ToolTip.TextPadding = new Size(10, 10);
                markerMaster.ToolTip.Font = new Font("Arial", 11);
                //markerMaster.ToolTip.Offset = GMapRoundedToolTip.DefaultForeground;
                markerMaster.ToolTipMode = MarkerTooltipMode.Always;
                markerMaster.ToolTipText = " \n" + master.Name;
                markersOverlay.Markers.Add(markerMaster);
                k++;
            }
            if (order != null)
                gMapControl1.Position = new PointLatLng(Convert.ToDouble(order.Lat), Convert.ToDouble(order.Lon));
            else
            {
                if (Constants.MASTERS.Count > 0)
                    gMapControl1.Position = new PointLatLng(Convert.ToDouble(Constants.MASTERS[0].StartLat), Convert.ToDouble(Constants.MASTERS[0].StartLon));
                else
                    gMapControl1.Position = new PointLatLng(55.75115385725043, 37.61728461663421);
            }
            gMapControl1.Overlays.Add(markersOverlay);
            gMapControl1.Refresh();
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            toolStripButton5_Click(sender, e);
            ShowMarkers();
            ShowRoutes();
        }

        private void gMapControl1_OnMapZoomChanged()
        {
            trackBar1.Value = (int)gMapControl1.Zoom;
        }

        private bool CheckTimeBeg()
        {
            try
            {
                string[] str = maskedTextBox1.Text.Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (Convert.ToInt32(str[0]) > 23 || Convert.ToInt32(str[1]) > 59)
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void gMapControl1_MouseClick(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right)
            {
                //toolStripButton5_Click(sender, e);
                textBox19.Text = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat.ToString();
                textBox20.Text = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng.ToString();
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            new ManageMasters().ShowDialog();
            FillCombo1();
            FillMasters();
            toolStripButton5_Click(sender, e);
            ShowMarkers();
            ShowRoutes();
            // this.Refresh();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            AppointOrder(Constants.MASTERS[0]);
            toolStripButton5_Click(sender, e);
            ShowMarkers();
            ShowRoutes();
        }

        private void AppointOrder(Master master)
        {
            Order order = null;
            for (int i = 0; i < Constants.ORDERS.Count; i++)
            {
                if (Constants.ORDERS[i].Name.Equals(comboBox1.Text))
                    order = Constants.ORDERS[i];
            }
            if (order != null)
            {
                order.Master = master.Name;
                DBHandlerMySQL.SetMasterOrder(order);
                master.myOrders.Add(order);
                FillCombo1();
            }
        }

        private void ShowRoutes()
        {
            Cursor.Current = Cursors.WaitCursor;
            foreach (Master master in Constants.MASTERS)
            {
                master.GetDistRouteOSM1(router, dateTimePicker1.Value);
                markersOverlay.Routes.Add(master.currentRoute);
            }
            gMapControl1.Overlays.Add(markersOverlay);
            gMapControl1.Refresh();
            Cursor.Current = Cursors.Default;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            AppointOrder(Constants.MASTERS[1]);
            toolStripButton5_Click(sender, e);
            ShowMarkers();
            ShowRoutes();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            AppointOrder(Constants.MASTERS[2]);
            toolStripButton5_Click(sender, e);
            ShowMarkers();
            ShowRoutes();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            AppointOrder(Constants.MASTERS[3]);
            toolStripButton5_Click(sender, e);
            ShowMarkers();
            ShowRoutes();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            AppointOrder(Constants.MASTERS[4]);
            toolStripButton5_Click(sender, e);
            ShowMarkers();
            ShowRoutes();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            AppointOrder(Constants.MASTERS[5]);
            toolStripButton5_Click(sender, e);
            ShowMarkers();
            ShowRoutes();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            AppointOrder(Constants.MASTERS[6]);
            toolStripButton5_Click(sender, e);
            ShowMarkers();
            ShowRoutes();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            AppointOrder(Constants.MASTERS[7]);
            toolStripButton5_Click(sender, e);
            ShowMarkers();
            ShowRoutes();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            AppointOrder(Constants.MASTERS[8]);
            toolStripButton5_Click(sender, e);
            ShowMarkers();
            ShowRoutes();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            AppointOrder(Constants.MASTERS[9]);
            toolStripButton5_Click(sender, e);
            ShowMarkers();
            ShowRoutes();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            for (int i = gMapControl1.Overlays.Count - 1; i >= 0; i--)
                gMapControl1.Overlays[i].Clear();
            markersOverlay.Routes.Clear();
            //foreach(CheckBox ch in masterCheckBoxes)
            //{
            //    ch.Checked = false;
            //}

            for (int i = 0; i < Constants.MASTERS.Count; i++)
            {
                masterButtons[i].Text = Constants.MASTERS[i].Name;
                Constants.MASTERS[i].currentRoute = null;
            }
            //ShowMarkers();
            gMapControl1.Refresh();
        }

        public static String FONT = "7454.ttf";
        private void RouteListPdf(Master master)
        {
            string strPdf = DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "\n" + master.Name + "\nМаршрутный лист на " + dateTimePicker1.Value.ToShortDateString() + "\n";
            strPdf += "---------------------------------------------\n";
            for (int i = 0; i < master.myOrders.Count; i++)
            {
                string intercom = master.myOrders[i].Intercom?"есть":"нет";

                strPdf += (i + 1).ToString() + "  " + master.myOrders[i].Name + "  " + master.myOrders[i].TimeBeg + "  " +
                    master.myOrders[i].City + ", " + master.myOrders[i].Street + " д." + master.myOrders[i].House + ", кв." + 
                    master.myOrders[i].Flat + " (оф." + master.myOrders[i].Office + "), под."+ master.myOrders[i].Porch + ", эт." +
                    master.myOrders[i].Floor+", домофон "+ intercom + ", тел.1:" + master.myOrders[i].Phone1 + ", тел.2:" + 
                    master.myOrders[i].Phone2 + "   " + master.myOrders[i].Description + "\n";

            }
            saveFileDialog1.Filter = "Формат .PDF (*.pdf)|*.pdf|Все файлы (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //using (PdfWriter pdfWriter = new PdfWriter(Constants.MASTERS[0].Name+"_"+dateTimePicker1.Value.ToShortDateString()+".pdf"))
                using (PdfWriter pdfWriter = new PdfWriter(saveFileDialog1.FileName))
                using (PdfDocument pdfDocument = new PdfDocument(pdfWriter))
                using (Document document = new Document(pdfDocument))
                {
                    PdfFont f2 = PdfFontFactory.CreateFont(FONT, "Identity-H", true);
                    document.Add(new Paragraph(strPdf).SetFont(f2));
                    
                }
            }           
        }

        private void button4_Click(object sender, EventArgs e)
        {
            RouteListPdf(Constants.MASTERS[0]);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            RouteListPdf(Constants.MASTERS[1]);
        }

        private void button22_Click(object sender, EventArgs e)
        {
            RouteListPdf(Constants.MASTERS[2]);
        }
    }
}
