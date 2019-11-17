using System.Text;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;
using System;
using System.Threading;
using System.Threading.Tasks;

/*
*	File: Entry.cs
*	Author: Slep.
*	Description: Main entrypoint for calls to RvExtension methods
*	
*	Do not remvoe these comment blocks!
*/

namespace aex
{
    public class EntryPoint
    {
        internal static bool init = false;
        internal static string sessionid = Utility.Session.CreateID();
        internal static string[] modules = { "Discord Disabled", "MySql Disabled" };
        internal static readonly string version = "1.0.00";
#if is64
        [DllExport("RVExtensionVersion", CallingConvention = CallingConvention.Winapi)]
#else
        [DllExport("_RVExtensionVersion@8", CallingConvention = CallingConvention.Winapi)]
#endif
        public static void RvExtensionVersion(StringBuilder output, int outputSize)
        {
            outputSize--;
            output.Append(version);
        }
#if is64
        [DllExport("RVExtension", CallingConvention = CallingConvention.Winapi)]
#else
        [DllExport("_RVExtension@12", CallingConvention = CallingConvention.Winapi)]
#endif

        public static void RvExtension(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string function)
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
                        init = true;
                    }
                    catch (Exception e)
                    {
                        Utility.Session.LogThis("[AEX::EXCEPTION] " + e.ToString());
                    }
                    if (!init)
                    {
                        output.Append(sessionid);
                    }
                }
                else
                {
                    Utility.Session.LogThis("[AEX::INIT] Blocked extension reload");
                    output.Append("");
                }
            }
        }

#if is64
        [DllExport("RVExtensionArgs", CallingConvention = CallingConvention.Winapi)]
#else
        [DllExport("_RVExtensionArgs@20", CallingConvention = CallingConvention.Winapi)]
#endif
        public static int RvExtensionArgs(StringBuilder output, int outputSize, 
            [MarshalAs(UnmanagedType.LPStr)] string function, 
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 4)] string[] args, int argCount)
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
                            Task<string> SQLAsync = Mysql.ExecuteAsync(args[1], Convert.ToBoolean(args[2]));
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
                Utility.Session.LogThis("[AEX::EXCEPTION] " + e.ToString());
                return 1;
            }
        }
    }
}
