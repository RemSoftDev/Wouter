using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Redirector
{
    public partial class Default : System.Web.UI.Page
    {

        private const string version = "1";

        protected void Page_Load(object sender, EventArgs e)
        {

            string target = "http://windowsphone.com/s?appid=2f44a06e-3d7c-4e11-b74d-9135949a1889";

            try
            {

                if (Settings.LastLoaded == DateTime.MinValue)
                {

                    //Settings.Load(null);

                    // load settings, when not loaded yet (by sending empty message queue)
                    DataService.FoodJournalServiceClient c1 = new DataService.FoodJournalServiceClient();
                    var firstsettings = c1.Push("Redirector", "REDIR", version + "?" + Settings.LastLoaded.ToString("yyyy/MM/dd"), new DataService.Message[] { });
                    if (firstsettings != null && firstsettings.Length > 0)
                        Settings.Load(firstsettings[0].Key);

                }

                string Message = @"<?xml version=""1.0"" encoding=""utf-8""?><Redirect>";
                Message += string.Format("<{0}>{1}</{0}>\n", "ClientIP", Request.UserHostAddress);
                Message += string.Format("<{0}>{1}</{0}>\n", "Querystring", Request.QueryString);
                Message += string.Format("<{0}>{1}</{0}>\n", "Referrer", Request.UrlReferrer);
                Message += string.Format("<{0}>{1}</{0}>\n", "Ad", Request["a"]);
                Message += string.Format("<{0}>{1}</{0}>\n", "Campaign", Request["c"]);
                Message += string.Format("<{0}>{1}</{0}>\n", "Platform", Request["p"]);
                Message += string.Format("<{0}>{1}</{0}>\n", "Keywords", Request["k"]);
                Message += string.Format("<{0}>{1}</{0}>\n", "Headers", Request.Headers);
                Message += string.Format("<{0}>{1}</{0}>\n", "UserAgent", Request.UserAgent);
                Message += string.Format("<{0}>{1}</{0}>\n", "Languages", string.Join(" | ", Request.UserLanguages));
                Message += string.Format("<{0}>{1}</{0}>\n", "RequestTarget", Request["t"]);

                target = Settings.GetSettingsRedirect(Message, target);

                if (!string.IsNullOrEmpty(Request["t"])) target = Request["t"];

                Message += string.Format("<{0}>{1}</{0}>\n", "Target", target);
                Message += "</Redirect>";

                DataService.Message[] m = new DataService.Message[] { new DataService.Message() { MessageType = "Redirect", Body = Message, Key = "1" } };
                DataService.FoodJournalServiceClient c = new DataService.FoodJournalServiceClient();

                var result = c.Push("Redirector", "REDIR", version + "?" + Settings.LastLoaded.ToString("yyyy/MM/dd"), m);
                if (result != null && result.Length > 0)
                    foreach(var r in result)
                        if (r.Key != null && r.Key.Length>8)
                            Settings.Load(r.Key);

            }
            catch { }

            Response.Redirect(target, true);

        }
    }
}