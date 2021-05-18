using GMap.NET;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmapTest
{
    class DBHandlerMySQL
    {
        public static MySqlConnection conn;
        public static void DBConnection()
        {
            //string connString = "Server = server211.hosting.reg.ru ;Database = u1381592_default; port=3306 ;User Id= u1381592_default ;password = pGDcz75BZgkTJ4G3; charset=utf8;";
            conn = new MySqlConnection(Constants.CONNECTION_STRING);            
        }        
                       
        public static void GetMasters()
        {
            DBConnection();
            Constants.MASTERS.Clear();            
            try
            {
                int k = 0;
                conn.Open();
               MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM masters";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Master master = new Master(k++, reader["name"].ToString(),
                        Convert.ToDouble(reader["startLat"].ToString().Replace('.', ',')),
                        Convert.ToDouble(reader["startLon"].ToString().Replace('.', ',')),
                        Convert.ToBoolean(reader["inWork"].ToString()));
                    Constants.MASTERS.Add(master);
                }
                reader.Close();
                conn.Close();
                conn.Dispose();
            }
            catch (Exception e) { }
        }

        public static void AddMaster(string name, string lat, string lon, bool inWork)
        {
            DBConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO masters (name, startLat, startLon, currentLat, currentLon, inWork) VALUES ('" +
                    name + "','" + lat + "','" + lon + "', '" + lat + "','" + lon + "','" + inWork + "')";
                cmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                conn = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Обновить данные о пользователе
        /// </summary>
        
        public static void UpdateMaster(string name, string lat, string lon, bool inWork)
        {
            DBConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE masters SET startLat='" + lat + "', startLon='" + lon +
                    "', inWork='" + inWork + "' WHERE name='" + name + "'";
                cmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                conn = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Удалить мастера
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public static bool DeleteMaster(string name)
        {
            DBConnection();
            bool res = true;            
            try
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM masters WHERE name='" + name + "'";
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { res = false; }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return res;
        }

        public static void GetOrders()
        {
            DBConnection();
            Constants.ORDERS.Clear();
            try
            {
                DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                DateTime future = new DateTime(2100, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);

                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM orders where dateOrder between '" +
                         now.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + future.ToString("yyyy-MM-dd HH:mm:ss") + "'";// completed='False'";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {

                    Order order = new Order(reader["id"].ToString(), reader["name"].ToString(),
                        reader["lat"].ToString().Replace('.', ','),
                        reader["lon"].ToString().Replace('.', ','),
                        reader["dateCreate"].ToString(),
                        reader["description"].ToString(),
                        reader["city"].ToString(),
                        reader["street"].ToString(),
                        reader["house"].ToString(),
                        reader["flat"].ToString(),
                        reader["office"].ToString(),
                        reader["porch"].ToString(),
                        reader["floor"].ToString(),
                        Convert.ToBoolean(reader["intercom"].ToString()),
                        reader["dateOrder"].ToString(),
                        reader["timeBeg"].ToString(),
                        reader["phone1"].ToString(),
                        reader["phone2"].ToString(),
                        reader["master"].ToString()
                        );
                    Constants.ORDERS.Add(order);

                }
                reader.Close();
                conn.Close();
                conn.Dispose();
            }
            catch (Exception e) { }
        }

        public static void AddOrder(string name, string description, string lat, string lon, string city, string street, string house,
           string flat, string office, string porch, bool intercom, string floor, DateTime dateOrder, string timeBeg, 
           string phone1, string phone2, string master)
        {
            DBConnection();
            string dateCreate = DateTime.Now.ToShortDateString();
            DateTime dt = new DateTime(dateOrder.Year, dateOrder.Month, dateOrder.Day, 2,0,0);
            try
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO orders (name, description, lat, lon, city, street, house, flat, office," +
                    "porch, intercom, floor, dateOrder, dateCreate, timeBeg, phone1, phone2, master, completed) VALUES ('" +
                    name + "','" + description + "','" + lat + "', '" + lon + "','" + city + "','" + street +
                    "','" + house + "','" + flat + "','" + office + "','" + porch + "','" + intercom.ToString() + "','" + floor + "','" + dt.ToString("yyyy-MM-dd HH:mm:ss") + 
                    "','" + dateCreate + "','" + timeBeg + "','" + phone1 + "','" + phone2 + "','" + master + "','False')";
                cmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                conn = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Обновить данные о пользователе
        /// </summary>

        public static void UpdateOrder(string id, string name, string description, string city, string street, string house,
           string flat, string office, string porch, bool intercom, string floor, string dateOrder, string timeBeg, 
           string phone1, string phone2, string master, bool completed)
        {
            DBConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE orders SET name='" + name + 
                    "', description='" + description +
                    "', city='" + city +
                    "', street='" + street +
                    "', house='" + house +
                    "', flat='" + flat +
                    "', office='" + office +
                    "', porch='" + porch +
                    "', floor='" + floor +
                    "', intercom='" + intercom.ToString() +
                    "', dateOrder='" + dateOrder +
                    "', timeBeg='" + timeBeg +
                    "', phone1='" + phone1 +
                    "', phone2='" + phone2 +
                    "', master='" + master +
                    "', completed='" + completed.ToString() +
                    "' WHERE id='" + id + "'";
                cmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                conn = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void SetMasterOrder(Order order)
        {
            DBConnection();
            try
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE orders SET master='" + order.Master +                    
                    "' WHERE id='" + order.Id + "'";
                cmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                conn = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        /// <summary>
        /// Удалить мастера
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public static bool DeleteOrder(string id)
        {
            DBConnection();
            bool res = true;            
            try
            {
                conn.Open();
               MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM orders WHERE id='" + id + "'";
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { res = false; }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return res;
        }
    }
}
