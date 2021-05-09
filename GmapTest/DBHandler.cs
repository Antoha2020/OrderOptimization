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
    class DBHandler
    {
        public static MySqlConnection conn;
        public static void DBConnection()
        {
            String connString = "Server=" + Constants.HOST + ";Database=" + Constants.DATABASE
                + ";port=" + Constants.PORT + ";User Id=" + Constants.USERNAME + ";password=" + Constants.PASSWORD;
            conn = new MySqlConnection(connString);            
        }

        public string getAuth(string Login, string pwd) //проверка логина и пароля в базе данных
        {
            DBConnection();
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM users";
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader["login"].Equals(Login) && reader["password"].Equals(pwd))
                {
                    reader.Close();
                    conn.Close();
                    return Login;
                }
            }

            reader.Close();
            conn.Close();
            return null;
        }

        public static List<string> getPoints(string Code)
        {
            bool enter = false;
            List<string> listData = new List<string>();
            DBConnection();
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name_contragent FROM tradePoints WHERE code='" + Code.Trim() + "'";
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                listData.Add(reader["name_contragent"].ToString());
                enter = true;
                break;
            }
            if (!enter)
                listData.Add("-");
            enter = false;
            reader.Close();

            cmd.CommandText = "SELECT name_point FROM tradePoints WHERE code='" + Code.Trim() + "'";
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                listData.Add(reader["name_point"].ToString());
                enter = true;
                break;
            }
            if (!enter)
                listData.Add("-");
            enter = false;
            reader.Close();

            cmd.CommandText = "SELECT work_time_beg FROM tradePoints WHERE code='" + Code.Trim() + "'"; 
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                listData.Add(reader["work_time_beg"].ToString());
                enter = true;
                break;
            }
            if (!enter)
                listData.Add("-");
            enter = false;
            reader.Close();

            cmd.CommandText = "SELECT work_time_end FROM tradePoints WHERE code='" + Code.Trim() + "'";
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                listData.Add(reader["work_time_end"].ToString());
                enter = true;
                break;
            }
            if (!enter)
                listData.Add("-");
            enter = false;
            reader.Close();
            conn.Close();

            return listData;
        }

        public const string CONNECTION_STRING = "Data Source=db/OrdersOptimization.db;Version=3;";
               
        public static void GetMasters()
        {
            if (!File.Exists("db/OrdersOptimization.db"))
                return;
            Constants.MASTERS.Clear();
            SQLiteConnection conn = new SQLiteConnection(CONNECTION_STRING);
            try
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM masters";
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Master master = new Master(reader["name"].ToString(),
                        Convert.ToDouble(reader["startLat"].ToString().Replace('.', ',')),
                        Convert.ToDouble(reader["startLon"].ToString().Replace('.', ',')),
                        //Convert.ToDouble(reader["currentLat"].ToString().Replace('.', ',')),
                        //Convert.ToDouble(reader["currentLon"].ToString().Replace('.', ',')),
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
            SQLiteConnection conn = new SQLiteConnection(CONNECTION_STRING);
            try
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
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
            SQLiteConnection conn = new SQLiteConnection(CONNECTION_STRING);
            try
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
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
            bool res = true;
            SQLiteConnection conn = new SQLiteConnection(CONNECTION_STRING);
            try
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
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
            if (!File.Exists("db/OrdersOptimization.db"))
                return;
            Constants.ORDERS.Clear();
            SQLiteConnection conn = new SQLiteConnection(CONNECTION_STRING);
            try
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM orders where completed='False'";
                SQLiteDataReader reader = cmd.ExecuteReader();
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
                        reader["timeEnd"].ToString(),
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
           string flat, string office, string porch, bool intercom, string floor, string dateOrder, string timeBeg, string timeEnd,
           string phone1, string phone2, string master)
        {
            string dateCreate = DateTime.Now.ToShortDateString();
            SQLiteConnection conn = new SQLiteConnection(CONNECTION_STRING);
            try
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO orders (name, description, lat, lon, city, street, house, flat, office," +
                    "porch, intercom, floor, dateOrder, dateCreate, timeBeg, timeEnd, phone1, phone2, master, completed) VALUES ('" +
                    name + "','" + description + "','" + lat + "', '" + lon + "','" + city + "','" + street +
                    "','" + house + "','" + flat + "','" + office + "','" + porch + "','" + intercom.ToString() + "','" + floor + "','" + dateOrder + 
                    "','" + dateCreate + "','" + timeBeg + "','" + timeEnd + "','" + phone1 + "','" + phone2 + "','" + master + "','False')";
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

        public static void UpdateOrder(string id, string name, string description, string lat, string lon, string city, string street, string house,
           string flat, string office, string porch, bool intercom, string floor, string dateOrder, string timeBeg, string timeEnd,
           string phone1, string phone2, string master, bool completed)
        {
            SQLiteConnection conn = new SQLiteConnection(CONNECTION_STRING);
            try
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE orders SET name='" + name + "', lat='" + lat + "', lon='" + lon +
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
                    "', timeEnd='" + timeEnd +
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

        /// <summary>
        /// Удалить мастера
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public static bool DeleteOrder(string id)
        {
            bool res = true;
            SQLiteConnection conn = new SQLiteConnection(CONNECTION_STRING);
            try
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
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
