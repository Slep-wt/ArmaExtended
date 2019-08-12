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
        public static bool init = false;
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
                    init = true;
                    try
                    {
                        Utility.Session.LogThis("[AEX::INIT] Current SessionID: " + sessionid);
                        
                        Discord.ModuleInit();
                        Mysql.ModuleInit();
                        string ActiveModules = "[AEX::INIT::MODULES] " + modules[0] + " | " + modules[1];
                        Utility.Session.LogThis(ActiveModules);
                    }
                    catch (Exception e)
                    {
                        Utility.Session.LogThis("[AEX::EXCEPTION] " + e.ToString());
                    }

                    output.Append(sessionid);
                }
                else
                {
                    Utility.Session.LogThis("[AEX::INIT] Blocked extension reload");
                    output.Append(sessionid);
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
                        if (args.Length == 2)
                            Discord.Send(args[1]);
                        else
                            output.Append("Discord:Send::INVALID_ARGS");
                        break;
                    case "mysql:async":
                        if (args.Length == 3)
                        {
                            bool Read = Convert.ToBoolean(args[2]);
                            Task<string> SQLAsync = Mysql.ExecuteAsync(args[1], Read);
                            SQLAsync.Wait();
                            output.Append(SQLAsync.Result);
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

        public static object DebugEntry(string function, string[] args = null , int timeout = 0)
        {
            string fresult = "";
            switch (function.ToLower())
            {
                case "discord.send":
                        Discord.Send(args[0]);
                    break;
                case "mysql:async":
                    bool Read = Convert.ToBoolean(args[1]);
                    Task<string> SQLAsync = Mysql.ExecuteAsync(args[0], Read);
                    SQLAsync.Start();
                    SQLAsync.Wait();
                    fresult = SQLAsync.Result;
                    break;
                case "utility:genkey":

                default:
                    fresult = "INVALID_FNC";
                    break;
            }

            Thread.Sleep(timeout * 1000);
            return fresult;
        }
    }
}
