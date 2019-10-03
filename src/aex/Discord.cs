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
        internal static readonly string[] apid = (string[])Utility.JSON.readJSONArray("discord","apid");
        internal static readonly string[] apikey = (string[])Utility.JSON.readJSONArray("discord","apikey");
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

        internal static bool Send(string args, string channel_select, string isRich)
        {
            if (!ModuleActivated || !Int32.TryParse(channel_select, out int channel)) throw new Exception("Module deactivated or input variables incorrect."); ;
            try
            {
                bool embeds = bool.Parse(isRich);
                string capid = apid[channel];
                string capikey = apikey[channel];

                args = args.Replace("\"", "");

                object[] toUtility = args.Split(';');
                Int32.TryParse((string)toUtility[1], out int im);
                toUtility[1] = im;
                if (!(im >= 0 && im <= 3)) throw new Exception("Colour variable in request was outside of the common bounds.");
                if (toUtility[0].ToString().ToCharArray().Length > 1999 && !embeds)
                {
                    return false;
                }

                var fcontent = Utility.JSON.formatRequest(toUtility, embeds);

                using (var req = new WebClient())
                {
                    Uri exturl = new Uri("https://discordapp.com/api/webhooks/" + capid + @"\" + capikey, UriKind.Absolute);
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
