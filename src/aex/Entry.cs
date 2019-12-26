using System.Text;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;


/*
*	File: Entry.cs
*	Author: Slep.
*	Description: Main entrypoint for calls to RvExtension methods
*	
*	Do not remvoe these comment blocks!
*/

#if !A3COMPAT
using System.Text.RegularExpressions;
class main
{
    static Regex cr = new Regex(@"(\[.*?\])|(\"".*?\"")|(\d)");
    static bool Loaded = false;
    static string sid;
    static void Main()
    {
        bool exit = false;
        StringBuilder output = new StringBuilder();
        if (!Loaded)
        {
            aex.EntryPoint.RvExtension(ref output, 1024, "load");
            sid = output.ToString();
            output.Clear();
            Loaded = true;
        }

        while (!exit)
        {
            try
            {
                string cmd = Console.ReadLine();
                MatchCollection cm = cr.Matches(cmd);
                List<string> parameters = new List<string>();
                string command = "";

                parameters.Add(sid);

                foreach (Match x in cm)
                {
                    string y = x.ToString();
                    Console.WriteLine(y);
                    if (y.StartsWith("[")) {
                        command = y.Trim(new char[] { '[', ']' });
                        Console.WriteLine(command);
                    } else if (y.StartsWith("\""))
                        parameters.Add(y.Trim(new char[] { '"', '"' }));
                    else
                        parameters.Add(y);
                }
                if (command != "" && parameters.Count > 1)
                {
                    int res = aex.EntryPoint.RvExtensionArgs(ref output, 1024, command, parameters.ToArray(), parameters.Count);
                }
                else if (command == "version")
                    aex.EntryPoint.RvExtensionVersion(ref output, 1024);
                else
                    aex.EntryPoint.RvExtension(ref output, 1024, command);

                Console.Write(output.ToString());
                output.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
#endif


namespace aex
{
    public class EntryPoint
    {
        internal static bool init = false;
        internal static string sessionid = Utility.Session.CreateID();
        internal static string[] modules = { "Discord Disabled", "MySql Disabled" };
        internal static readonly string version = "1.0.10";
#if A3COMPAT
#if is64
        [DllExport("RVExtensionVersion", CallingConvention = CallingConvention.Winapi)]
#else
        [DllExport("_RVExtensionVersion@8", CallingConvention = CallingConvention.Winapi)]
#endif
#endif
        public static void RvExtensionVersion(ref StringBuilder output, int outputSize)
        {
            outputSize--;
            output.Append(version);
        }

#if A3COMPAT
#if is64
        [DllExport("RVExtension", CallingConvention = CallingConvention.Winapi)]
#else
        [DllExport("_RVExtension@12", CallingConvention = CallingConvention.Winapi)]
#endif
        public static void RvExtension(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string function)
#else
        public static void RvExtension(ref StringBuilder output, int outputSize, string function)
#endif
        {
            outputSize--;
            if (function == "load")
            {
                if (!init)
                {
                    try
                    {
                        Utility.Session.LogThis("[AEX::INIT] Current SessionID: " + sessionid);
                        
                        Discord.ModuleInit();
                        Mysql.ModuleInit();
                        string ActiveModules = "[AEX::INIT::MODULES] " + modules[0] + " | " + modules[1];
                        Utility.Session.LogThis(ActiveModules);
                        output.Append(sessionid);
                        init = true;
                    }
                    catch (Exception e)
                    {
                        Utility.Session.LogThis("[AEX::EXCEPTION] " + e.ToString());
                    }
                }
                else
                {
                    Utility.Session.LogThis("[AEX::INIT] Blocked extension reload");
                    output.Append("");
                }
            }
        }

#if A3COMPAT
#if is64
        [DllExport("RVExtensionArgs", CallingConvention = CallingConvention.Winapi)]
#else
        [DllExport("_RVExtensionArgs@20", CallingConvention = CallingConvention.Winapi)]
#endif
        public static int RvExtensionArgs(StringBuilder output, int outputSize, 
            [MarshalAs(UnmanagedType.LPStr)] string function, 
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 4)] string[] args, int argCount)
#else
        public static int RvExtensionArgs(ref StringBuilder output, int outputSize, string function, string[] args, int argCount)
#endif
        {
            outputSize--;
            try
            {
                string cid = args[0].Replace("\"", "");
                if (cid != sessionid) { output.Append("Sessionid's dont match. Provided ID: " + cid); return 1; }

                switch (function.ToLower())
                {
                    case "discord:send":
                        if (args.Length == 3)
                            Discord.Send(args[1], args[2]);
                        else
                            output.Append("Discord:Send::INVALID_ARGS");
                        break;
                    case "discord:sendrich":
                        if (args.Length == 5 || args.Length == 4 || args.Length == 3)
                        {
                            int color;
                            switch (args[3].ToLower())
                            {
                                case "green":
                                    color = Utility.EmbedColors[0];
                                    break;
                                case "orange":
                                    color = Utility.EmbedColors[1];
                                    break;
                                case "yellow":
                                    color = Utility.EmbedColors[2];
                                    break;
                                case "red":
                                    color = Utility.EmbedColors[3];
                                    break;
                                default:
                                    color = Utility.EmbedColors[0];
                                    break;
                            }
                            if (args.Length == 5)
                            {
                                Discord.SendRich(args[1], color, args[2], args[3]);
                            }
                            else if (args.Length == 4)
                            {
                                Discord.SendRich(args[1], color, args[2]);
                            }
                            else
                            {
                                Discord.SendRich(args[1], color);
                            }
                        }
                        else
                            output.Append("Discord:Send::INVALID_ARGS");
                        break;
                    case "mysql:async":
                        if (args.Length == 3)
                        {
                            Task<string> SQLAsync = Mysql.ExecuteAsync(args[1], Convert.ToBoolean(Convert.ToInt32(args[2])));
                            SQLAsync.Wait();
                            output.Append(SQLAsync.Result);
                        }  else
                            output.Append("Mysql:Async::INVALID_ARGS");
                        break;
                    case "mysql:buffer":
                        if (args.Length == 3)
                        {
                            try
                            {
                                Task<string> SQLBuffer = Mysql.FetchBuffer(Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
                                SQLBuffer.Wait();
                                output.Append(SQLBuffer.Result);
                            } catch (Exception e)
                            {
                                Utility.Session.LogThis(e.InnerException.ToString());
                                output.Append("Mysql:Async::INVALID_ARGS");
                            }

                        } else
                            output.Append("Mysql:Async::INVALID_ARGS");
                        break;
                    default:
                        output.Append("INVALID_FNC");
                        Utility.Session.LogThis("[AEX::FNC] Function " + function + " does not exist.");
                        break;
                }
                return 0;
            }
            catch (Exception e)
            {
#if !A3COMPAT
                output.Append(e.ToString());
#endif
                Utility.Session.LogThis("[AEX::EXCEPTION] " + e.ToString());
                return 1;
            }
        }
    }
}