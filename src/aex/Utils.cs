using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Newtonsoft.Json;

/*
*	File: Utils.cs
*	Author: Slep.
*	Description: Supporting utilities for the extension (Makes shit go)
*	
*	Do not remvoe these comment blocks!
*/

namespace aex
{
    public class Utility
    {
#region sysdef
#if is64
        public static bool x64 = true;
#else
        public static bool x64 = false;
#endif
        #endregion

        public static readonly string Dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string CfgDir = Dir + @"\cfg";
        public static readonly string CfgJson = Dir + @"\cfg\config.json";
        public static readonly string LogDir = Dir + @"\Logs";

        public static readonly string rundate = DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss");

        internal static readonly char[] Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        internal static readonly int AlphabetLength = Alphabet.Length;

        public class Session
        {
            public static string CreateID()
            {
                using (MD5 x = MD5.Create())
                {
                    var r = new Random();
                    string rchars = "";
                    for (var i = 0; i < 9; i++)
                        rchars = rchars + Alphabet[r.Next(AlphabetLength)];

                    byte[] interm = { };
                    interm = x.ComputeHash(Encoding.UTF8.GetBytes(rchars));

                    StringBuilder sb = new StringBuilder();
                    foreach (byte i in interm)
                        sb.Append(i.ToString("X2"));

                    return sb.ToString();
                }
            }


            internal static async void LogThis(string input)
            {
                if (Directory.Exists(LogDir))
                {
                    string filename;

                    if (x64)
                        filename = "aex_x64_" + rundate + ".log";
                    else
                        filename = "aex_x86_" + rundate + ".log";
                    
                    using (var sw = new StreamWriter(LogDir + @"\" + filename, true, Encoding.UTF8))
                    {
                        string curdate = DateTime.Now.ToString(@"[dd/MM/yyyy hh:mm:ss] ");
                        await sw.WriteLineAsync(curdate + input);
                    }
                }
                else
                {
                    Directory.CreateDirectory(LogDir);
                    LogThis(input);
                }
            }
        }

        internal class JSON
        {
            internal class JSONFormat
            {
                [JsonProperty("EnableDiscord")]
                public bool EnableDiscord;

                [JsonProperty("DiscordUsername")]
                public string DiscordUsername;

                [JsonProperty("DiscordAvatar")]
                public string DiscordAvatar;

                [JsonProperty("DiscordApid")]
                public string DiscordApid;

                [JsonProperty("DiscordApikey")]
                public string DiscordApikey;

                [JsonProperty("EnableMysql")]
                public bool EnableMysql;

                [JsonProperty("MysqlUsername")]
                public string MysqlUsername;

                [JsonProperty("MysqlPassword")]
                public string MysqlPassword;

                [JsonProperty("MysqlServerAddress")]
                public string MysqlServerAddress;

                [JsonProperty("MysqlServerPort")]
                public int MysqlServerPort;

                [JsonProperty("MysqlDatabase")]
                public string MysqlDatabase;

                [JsonProperty("EnableDebug")]
                public bool EnableDebug;
            }

            internal class RequestFormat
            {
                [JsonProperty("username")]
                public string username;

                [JsonProperty("avatar_url")]
                public string avatar_url;

                [JsonProperty("content")]
                public string content;
            }

            internal static bool createJSON()
            {
                JSONFormat jsformat = new JSONFormat
                {
                    EnableDiscord = true,
                    DiscordUsername = "<Add the username of your webhook here>",
                    DiscordAvatar = "<Add the url to the avatar of your webhook here>",
                    DiscordApid = "<Add your webhook ID here>",
                    DiscordApikey = "<Add your webhook private key here>",

                    EnableMysql = true,

                    MysqlUsername = "<Add your MySql server username here>",
                    MysqlPassword = "<Add your MySql server password here>",
                    MysqlServerAddress = "<Add the IP to your Mysql server here>",
                    MysqlServerPort = 3306,
                    MysqlDatabase = "<Name of the database you want to access>",

                    EnableDebug = false
                };

                var serialiser = new JsonSerializer();
                    
                using (var sw = new StreamWriter(CfgJson, false))
                using (var jsw = new JsonTextWriter(sw))
                {
                    serialiser.Formatting = Formatting.Indented;
                    serialiser.Serialize(jsw, jsformat);
                }
                Session.LogThis("[AEX::JSON] Created config.json in cfg directory.");
                return true;
                
            }

            public static string formatRequest(string input)
            {
                RequestFormat req = new RequestFormat
                {
                    username = Discord.hookname,
                    avatar_url = Discord.avatar,
                    content = input
                };
                return JsonConvert.SerializeObject(req, Formatting.Indented);
            }


            public static object readJSON(string module, string attrib)
            {
                if (!(Directory.Exists(CfgDir)))
                {
                    Session.LogThis("[AEX::JSON] Config directory missing, creating a new directory.");
                    Directory.CreateDirectory(CfgDir);
                }
                if (!(File.Exists(CfgJson)))
                {
                    Session.LogThis("[AEX::JSON] Readable file not found, creating a new json file. Please configure this otherwise the extension will not work.");
                    createJSON();
                }

                JSONFormat ds = null;

                try
                {
                    ds = JsonConvert.DeserializeObject<JSONFormat>(File.ReadAllText(CfgDir + @"\config.json"));
                }
                catch (EntryPointNotFoundException e)
                {
                    createJSON();
                    System.Threading.Thread.Sleep(1000);
                    readJSON(module, attrib);
                }
                if (module == "discord")
                {
                    if (Discord.ModuleActivated == false) { return "ERR_INVALID_MODULE_DISCORD"; }
                    switch (attrib)
                    {
                        case "username":
                            return ds.DiscordUsername;

                        case "avatar":
                            return ds.DiscordAvatar;

                        case "apid":
                            return ds.DiscordApid;

                        case "apikey":
                            return ds.DiscordApikey;

                        case "enable":
                            return ds.EnableDiscord;

                        default:
                            return "ERR_INVALID_ATTRIB";
                    }
                }
                else if (module == "mysql")
                {
                    if (Mysql.ModuleActivated == false) { return "ERR_INVALID_MODULE_MYSQL"; }
                    switch (attrib)
                    {
                        case "username":
                            return ds.MysqlUsername;

                        case "password":
                            return ds.MysqlPassword;

                        case "address":
                            return ds.MysqlServerAddress;

                        case "port":
                            return ds.MysqlServerPort;

                        case "database":
                            return ds.MysqlDatabase;

                        case "enable":
                            return ds.EnableMysql;

                        default:
                            return "ERR_INVALID_ATTRIB";
                    }
                }

                return "ERR_INVALID_PARAMS";
            }
        }
    }
}
