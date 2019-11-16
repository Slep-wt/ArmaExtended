using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Net;

/*
*	File: Mysql.cs
*	Author: Slep.
*	Description: Discord utilities and API calling method
*	
*	Do not remvoe these comment blocks!
*/

namespace aex
{
    internal class Discord
    {
        internal static bool ModuleActivated = true;
        internal static readonly string[] apid = (string[])Utility.JSON.readJSONArray("discord","apid");
        internal static readonly string[] apikey = (string[])Utility.JSON.readJSONArray("discord","apikey");
        internal static readonly string[] channels = (string[])Utility.JSON.readJSONArray("discord","channels"); 
        internal static readonly string hookname = (string)Utility.JSON.readJSON("discord","username");
        internal static readonly string avatar = (string)Utility.JSON.readJSON("discord","avatar");
        internal static void ModuleInit()
        {
            if (ModuleActivated)
            {
                 ModuleActivated = (bool)Utility.JSON.readJSON("discord", "enable");
                EntryPoint.modules.SetValue("Discord Enabled", 0);
            }
        }

        internal static async void SendRich(string args, int color = 0, string hook_select = "0", string channel_select = "0")
        {
            if (!ModuleActivated || !Int32.TryParse(channel_select, out int channel) || (Int32.TryParse(hook_select, out int hook))) throw new Exception("Module deactivated or input variables incorrect.");
            try
            {
                string capid = apid[hook];
                Utility.Session.LogThis(capid);
                string capikey = apikey[hook];
                string cid = channels[channel];
                Utility.Session.LogThis(capikey);
                args = args.Replace("\"", "");
                List<string> toUtility = new List<string>();
                foreach (string x in args.Split(';'))
                    toUtility.Add(x);
                toUtility.Add(cid);

                var fcontent = Utility.JSON.formatRequestRich(toUtility.ToArray(),color);
                using (var req = new WebClient())
                {
                    Uri exturl = new Uri("https://discordapp.com/api/webhooks/" + capid + @"\" + capikey, UriKind.Absolute);
                    req.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    req.UploadStringCompleted += Req_UploadStringCompleted;
                    await req.UploadStringTaskAsync(exturl, "POST", fcontent);
                }
                return;
            }
            catch (Exception e)
            {
                Utility.Session.LogThis("[AEX::EXCEPTION] " + e.ToString());
                return;
            }
        }

        internal static async void Send(string Args, string HookSelect = "0", string ChannelSelect = "0")
        {
            if (!ModuleActivated || !Int32.TryParse(ChannelSelect, out int channel) || !Int32.TryParse(HookSelect, out int hook)) throw new Exception("Module deactivated or input variables incorrect.");
            try
            {
                string capid = apid[hook];
                Utility.Session.LogThis(capid);
                string cid = channels[channel];
                string capikey = apikey[hook];
                Utility.Session.LogThis(capikey);

                Args = Args.Replace("\"", "");
                if (Args.Length > 1999)
                    throw new ArgumentException("The input argument exceeded 1999 characters.");

                var fcontent = Utility.JSON.formatRequest(Args, cid);
                using (var req = new WebClient())
                {
                    Uri exturl = new Uri("https://discordapp.com/api/webhooks/" + capid + @"\" + capikey, UriKind.Absolute);
                    req.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    req.UploadStringCompleted += Req_UploadStringCompleted;
                    await req.UploadStringTaskAsync(exturl, "POST", fcontent);

                }
                return;
            } catch (Exception e)
            {
                Utility.Session.LogThis("[AEX::EXCEPTION] " + e.ToString());
                return;
            }
        }

        internal static void Req_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            try
            {
                Utility.Session.LogThis("[AEX::SENDMSG::SUCCESS]: " + e.ToString());
            }
            catch (Exception ex)
            {
                Utility.Session.LogThis("[AEX::SENDMSG::FAILURE]: " + ex.InnerException.ToString());
            }
        }
    }
}
