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
        public static readonly int[] EmbedColors = { 0x2cc510, 0xee7511, 0xffdf00, 0xe80505 }; // Green, Orange, Yellow, Red
        public static readonly string Dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string CfgDir = Dir + @"\cfg";
        public static readonly string CfgJson = Dir + @"\cfg\config.json";
        public static readonly string LogDir = Dir + @"\Logs";
        public static readonly string rundate = DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss");

#if is64
        public static readonly string Filename = "aex_x64_" + rundate + ".log";
#else
        public static readonly string Filename = "aex_x86_" + rundate + ".log";
#endif

        internal static readonly char[] Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        internal static readonly int AlphabetLength = Alphabet.Length;


        public static int Ciel(int a, int b)
        {
            return (a + b - 1) / b;
        }

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

            internal static async void LogThis(object input)
            {
                if (Directory.Exists(LogDir))
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(LogDir + @"\" + Filename, true, Encoding.UTF8))
                        {
                            string curdate = DateTime.Now.ToString(@"[dd/MM/yyyy hh:mm:ss]");
                            await sw.WriteLineAsync(curdate + input);
                        }
                    } catch (Exception e)
                    {
                        LogThis(input);
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
                public string[] DiscordApid;

                [JsonProperty("DiscordApikey")]
                public string[] DiscordApikey;

                [JsonProperty("DiscordChannels")]
                public string[] DiscordChannels;

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

                [JsonProperty("MysqlBufferSize")]
                public int MysqlBufferSize;

                [JsonProperty("DataMaxReturnSize")]
                public int DataMaxReturnSize;

                [JsonProperty("EnableDebug")]
                public bool EnableDebug;
            }

            internal class RequestFormat
            {
                [JsonProperty("username")]
                public string username;

                [JsonProperty("avatar_url")]
                public string avatar_url;

                [JsonProperty("channel_id")]
                public string channelid;

                [JsonProperty("content")]
                public string content;
            }

            internal class RichRequestFormat
            {
                [JsonProperty("username")]
                public string username;

                [JsonProperty("avatar_url")]
                public string avatar_url;

                [JsonProperty("channel_id")]
                public string channelid;

                [JsonProperty("embeds")]
                public Embeds[] EmbedContent;

                public class Embeds
                {
                    [JsonProperty("title")]
                    public string title;

                    [JsonProperty("color")]
                    public int color;

                    [JsonProperty("fields")]
                    public Fields[] FieldsContent;

                    internal class Fields
                    {
                        [JsonProperty("name")]
                        public string name;

                        [JsonProperty("value")]
                        public string fcontent;
                    }
                }
            }

            internal static bool createJSON()
            {
                JSONFormat jsformat = new JSONFormat
                {
                    EnableDiscord = true,
                    DiscordUsername = "<Add the username of your webhook here>",
                    DiscordAvatar = "<Add the url to the avatar of your webhook here>",
                    DiscordApid = new string[] { "<Add your webhook ID's here seperated by commas>", "<Example ID 1>", "<Example ID 2>" },
                    DiscordApikey = new string[] { "<Add your webhook private keys here seperated by commas>", "<Example Private Key 1>", "<Example Private Key 2>" },
                    DiscordChannels = new string[] { "<Add your webhook channels here seperated by commas>", "<Example Channel 1>", "<Example Channel 2>" },


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

            public static string formatRequest(string input, string cid)
            {
                string result;
                RequestFormat req = new RequestFormat
                {
                    username = Discord.hookname,
                    avatar_url = Discord.avatar,
                    channelid = cid,
                    content = input
                };
                result = JsonConvert.SerializeObject(req, Formatting.Indented);
                Session.LogThis(result);
                return result;
            }

            public static string formatRequestRich(string[] input, int colour)
            {
                string result;
                RichRequestFormat req = new RichRequestFormat
                {
                    username = Discord.hookname,
                    avatar_url = Discord.avatar,
                    channelid = input[3],
                    EmbedContent = new RichRequestFormat.Embeds[] {
                        new RichRequestFormat.Embeds
                        {
                            title = input[0],
                            color = colour,
                            FieldsContent = new RichRequestFormat.Embeds.Fields[]
                            {
                                new RichRequestFormat.Embeds.Fields
                                {
                                     name = input[1],
                                     fcontent = input[2]
                                }
                            }
                        }
                    }
                };
                result = JsonConvert.SerializeObject(req, Formatting.Indented);
                Session.LogThis(result);
                return result;
            }


            public static object[] readJSONArray(string module, string attrib)
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
                    if (Discord.ModuleActivated == false) { return new string[1] { "ERR_INVALID_MODULE_DISCORD" }; }
                    switch (attrib)
                    {
                        case "apid":
                            return ds.DiscordApid;
                        case "apikey":
                            return ds.DiscordApikey;
                        case "channels":
                            return ds.DiscordChannels;

                        default:
                            return new string[1]{ "ERR_INVALID_ATTRIB" };
                    }
                }
                else if (module == "mysql")
                {
                    if (Mysql.ModuleActivated == false) { return new string[1] { "ERR_INVALID_MODULE_MYSQL" }; }
                    switch (attrib)
                    {

                        default:
                            return new string[1] { "ERR_INVALID_ATTRIB" };
                    }
                }

                return new string[1] { "ERR_INVALID_PARAMS" };
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
                        case "buffer":
                            return ds.MysqlBufferSize;
                        case "enable":
                            return ds.EnableMysql;

                        default:
                            return "ERR_INVALID_ATTRIB";
                    }
                }
                else if (module == "misc")
                {
                    switch (attrib)
                    {
                        case "datamaxreturn":
                            return ds.DataMaxReturnSize;
                        case "enabledebug":
                            return ds.EnableDebug;
                    }
                }
                return "ERR_INVALID_PARAMS";
            }
        }
    }
}
