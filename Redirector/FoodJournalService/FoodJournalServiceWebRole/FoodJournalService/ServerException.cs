using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace FoodJournalServiceWebRole.FoodJournalService
{
    public class ServerException
    {

        public static void Report(Exception ex)
        {
            try
            {
                var msg = new StringBuilder();
                try
                {
                    msg.AppendLine("Message: " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        msg.AppendLine("InnerException: " + ex.InnerException.Message);
                        if (ex.InnerException.InnerException != null)
                        {
                            msg.AppendLine("InnerInnerException: " + ex.InnerException.InnerException.Message);
                            if (ex.InnerException.InnerException.InnerException != null)
                            {
                                msg.AppendLine("InnerInnerInnerException: " + ex.InnerException.InnerException.InnerException.Message);
                            }
                        }
                    }
                    msg.AppendLine("Stack: " + ex.StackTrace);
                }
                catch { }
                //EMail.Send(EMail.DAILYJOURNALEMAIL, "Server Exception", msg.ToString());
            }
            catch { }
        }

    }
}