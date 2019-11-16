using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Linq;
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
        internal static MySqlConnection conn = new MySqlConnection(cstr);
        internal static readonly int MaxReturnSize = 8192;

        internal static string[][] SQLResultBuffer = new string[256][];

        internal static void ModuleInit()
        {
            if (ModuleActivated)
            {
                ModuleActivated = (bool)Utility.JSON.readJSON("mysql", "enable");
                EntryPoint.modules.SetValue("Mysql Enabled", 1);
            }
        }

        public static async Task<string> FetchBuffer(int BufferIndex,int DataIndex, bool FreeBuffer = false)
        {
            try
            {
                if (DataIndex == 0) FreeBuffer = true;

                string ToReturn = "[" + SQLResultBuffer[BufferIndex][DataIndex] + ",[" + (BufferIndex) + "," + (DataIndex - 1) + "]]";

                if (FreeBuffer)
                    SQLResultBuffer[BufferIndex] = null;
                else
                    Array.Resize(ref SQLResultBuffer[BufferIndex], SQLResultBuffer[BufferIndex].Length - 1);

                return ToReturn;
            } catch (Exception e)
            {
                Utility.Session.LogThis(e);
                return "[`BAD_BUFFER`,"+BufferIndex+","+DataIndex+"]";
            }

        }

        internal static string[] SplitString(string str)
        {
            int i, ei, si= 1;
            string[] rs = new string[256];
            i = 1;
            while (i <= str.Length / MaxReturnSize + 1)
            {
                si = (i - 1) * MaxReturnSize;
                ei = i * MaxReturnSize;
                if (ei > MaxReturnSize)
                {
                    ei = str.Length - (si);
                }
                rs.SetValue(str.Substring(si, ei), i - 1);
                i++;
            }
            rs = rs.Where(c => c != null).ToArray();
            return rs;
        }

        public static async Task<string> ExecuteAsync(string query, bool read)
        {
            if (!ModuleActivated) return "MODULE_MYSQL_DISABLED";
            query = query.Trim('"');
            try
            {
                conn.Open();
                if (conn.State != System.Data.ConnectionState.Open) throw new System.Exception("Mysql connection is not in the open state.");
                if (!read)
                {
                    int response = await MySqlHelper.ExecuteNonQueryAsync(conn, query);
                    if (response >= 1)
                    {
                        conn.Close();
                        return "MYSQL_NQ_SUCCESS";
                    }
                    conn.Close();
                    return "MYSQL_NQ_NOROWS";
                }
                else
                {
                    string result = "[";
                    
                    using (MySqlDataReader reader = MySqlHelper.ExecuteReader(conn, query))
                    {
                        while (reader.Read())
                        {
                            if (reader.FieldCount == 1)
                                result = reader[0].ToString();
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
                        if (reader.FieldCount > 1)
                        {
                            result += "]";
                        }
                        reader.Close();
                        conn.Close();
                        if (System.Text.Encoding.UTF8.GetByteCount(result) > MaxReturnSize)
                        {
                            string[] SplitResult = SplitString(result);
                            Utility.Session.LogThis(SplitResult.Length);
                            Array.Reverse(SplitResult);
                            Random r = new Random();
                            int BufferIndex = 0;
                            while (SQLResultBuffer[BufferIndex] != null)
                            {
                                BufferIndex = r.Next(0, 256);
                            }
                            int DataIndex = (SplitResult.Length - 2);
                            SQLResultBuffer[BufferIndex] = SplitResult;
                            return ("[`" + SplitResult[SplitResult.Length - 1] + "`,[" + BufferIndex + "," + DataIndex + "]]");
                        }
                        return result;
                    }
                }
            }
            catch (MySqlException e)
            {
                Utility.Session.LogThis("[AEX::MYSQL::ERROR] " + e.Message);
                return "MYSQL_REQUEST_FAILED_EXCEPTION";
            }
        }
    }
}
