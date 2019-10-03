using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

/*
*	File: Mysql.cs
*	Author: Slep.
*	Description: MySql connector and async methods
*	
*	Do not remvoe these comment blocks!
*/

namespace aex
{
    public class Mysql
    {
        internal static bool ModuleActivated = true;
        private static readonly string DatabaseUsername = (string)Utility.JSON.readJSON("mysql", "username");
        private static readonly string DatabasePassword = (string)Utility.JSON.readJSON("mysql", "password");
        private static readonly string DatabaseAddress = (string)Utility.JSON.readJSON("mysql", "address");
        private static readonly int PortNumber = (int)Utility.JSON.readJSON("mysql", "port");
        private static readonly string DefaultDatabase = (string)Utility.JSON.readJSON("mysql", "database");
        internal static readonly string cstr = "Server= " + DatabaseAddress + ";Port= " + PortNumber + ";Database=" + DefaultDatabase + ";Uid=" + DatabaseUsername + ";Pwd=" + DatabasePassword + ";Pooling=false;";

        private class SqlConnector : IDisposable
        {
            bool isDisposed = false;

            private SqlConnector()
            {
            }

            private string database = string.Empty;
            public string Database { get => database;  set => database = value; }

            private MySqlConnection connection = null;
            public MySqlConnection Connection
            {
                get { return connection; }
            }

            private static SqlConnector instance = null;
            public static SqlConnector Instance()
            {
                if (instance == null)
                    instance = new SqlConnector();
                return instance;
            }

            public bool IsConnect()
            {
                if (Connection == null)
                {
                    if (String.IsNullOrEmpty(database))
                        return false;
                    connection = new MySqlConnection(cstr);
                    connection.Open();
                }
                return true;
            }

            public void Close()
            {
                connection.Close();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool Disposing)
            {
                if (isDisposed)
                    return;

                if (Disposing)
                {
                    connection.Dispose();
                }
            }
        }

        internal static void ModuleInit()
        {
            if (ModuleActivated)
            {
                ModuleActivated = (bool)Utility.JSON.readJSON("mysql", "enable");
                EntryPoint.modules.SetValue("Mysql Enabled", 1);
            }
        }

        public static async Task<string> ExecuteAsync(string query, bool read)
        {
            query = query.Trim('"');
            if (!ModuleActivated) return "MODULE_MYSQL_DISABLED";
            SqlConnector conn = SqlConnector.Instance();
            while (conn == null)
            {
                Console.WriteLine("Connection Null!");
            }
            conn.Database = DefaultDatabase;
            try
            {
                if (conn.IsConnect())
                {
                    if (!read)
                    {
                        int response = await MySqlHelper.ExecuteNonQueryAsync(conn.Connection, query);
                        if (response == 1)
                        {
                            conn.Close();
                            return "MYSQL_NQ_SUCCESS";
                        }
                        return "MYSQL_NQ_NOROWS";
                    }
                    else
                    {
                        using (MySqlDataReader reader = MySqlHelper.ExecuteReader(conn.Connection, query))
                        {
                            string result = "[";
                            while (reader.Read())
                            {
                                if (reader.FieldCount == 1)
                                {
                                    conn.Close();
                                    return reader[0].ToString();
                                }
                                else
                                {
                                    for (var i = 0; i < reader.FieldCount; i++)
                                    {
                                        string stripped = reader[i].ToString();
                                        if (i == 0)
                                            result += stripped;
                                        else
                                            result += ", " + stripped;
                                    }
                                }
                            }
                            result += "]";
                            conn.Close();
                            return result;
                        }
                    }
                }
                return "MYSQL_REQUEST_FAILED";
            }
            catch (MySqlException e)
            {
                Utility.Session.LogThis("[AEX::MYSQL::ERROR] " + e.Message);
                return "MYSQL_REQUEST_EXCEPTION";
            }
        }
    }
}
