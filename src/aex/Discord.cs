using System;
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
        internal static readonly string apid = (string)Utility.JSON.readJSON("discord","apid");
        internal static readonly string apikey = (string)Utility.JSON.readJSON("discord","apikey");
        internal static readonly string hookname = (string)Utility.JSON.readJSON("discord","username");
        internal static readonly string avatar = (string)Utility.JSON.readJSON("discord","avatar");

        internal static void ModuleInit()
        {
            if (ModuleActivated)
            {
                 ModuleActivated = (bool)Utility.JSON.readJSON("discord", "enable");
                Entry.modules.SetValue("Discord Enabled", 0);
            }
        }

        internal static bool Send(string args)
        {
            if (!ModuleActivated) return false;

            try
            {
                args = args.Replace("\"", "");

                if (args.Length > 1999)
                {
                    return false;
                }
                var fcontent = Utility.JSON.formatRequest(args);

                using (var req = new WebClient())
                {
                    Uri exturl = new Uri("https://discordapp.com/api/webhooks/" + apid + @"\" + apikey, UriKind.Absolute);
                    req.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    req.UploadStringCompleted += Req_UploadStringCompleted;
                    req.UploadStringAsync(exturl, "POST", fcontent);
                }
                return true;
            } catch (Exception e)
            {
                Utility.Session.LogThis("[AEX::EXCEPTION] " + e.ToString());
                return false;
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
