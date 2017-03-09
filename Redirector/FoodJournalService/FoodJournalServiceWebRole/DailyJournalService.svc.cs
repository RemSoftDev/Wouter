using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using FoodJournalServiceWebRole.DailyJournalService;

namespace DailyJournalServiceRole
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class DailyJournalService : IDailyJournalService
    {

        public bool SaveFeedback(string email, bool cancontact, string feedback)
        {
            try
            {
                using (var c = new DailyLogDBClassesDataContext())
                {
                    c.Feedbacks.InsertOnSubmit(new Feedback() { email = email, feedback1 = feedback, timestamp = DateTime.Now });
                    c.SubmitChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                //throw ex;
            }
            return false;
        }


        public bool Feedback(Guid appinstance, string email, string feedback)
        {
            try
            {
                using (var c = new DailyLogDBClassesDataContext())
                {
                    c.Feedbacks.InsertOnSubmit(new Feedback() { appinstance = appinstance, email = email, feedback1 = feedback, timestamp = DateTime.Now });
                    c.SubmitChanges();
                }
                return true;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            EMail.Send(EMail.DAILYJOURNALEMAIL, "Feedback from: " + email, feedback);
            return false;
        }

        public bool ExceptionReport(Guid appinstance, string report)
        {
            try
            {
                using (var c = new DailyLogDBClassesDataContext())
                {
                    c.ExceptionReports.InsertOnSubmit(new ExceptionReport() { appinstance = appinstance, report = report, timestamp = DateTime.Now});
                    c.SubmitChanges();
                }
                return true;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            EMail.Send(EMail.DAILYJOURNALEMAIL, "Exception report", report);
            return false;
        }

        public bool PerformanceReport(Guid appinstance, string report)
        {
            try
            {
                using (var c = new DailyLogDBClassesDataContext())
                {
                    c.PerformanceReports.InsertOnSubmit(new PerformanceReport() { appinstance = appinstance, report = report, timestamp = DateTime.Now });
                    c.SubmitChanges();
                }
                return true;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            //EMail.Send(EMail.DAILYJOURNALEMAIL, "Performance report", report);
            return false;
        }

        public bool EmailReport(Guid appinstance, string email, string report, int days)
        {
            bool result;

            try
            {
                using (var c = new DailyLogDBClassesDataContext())
                {
                    c.EmailReports.InsertOnSubmit(new EmailReport() { appinstance = appinstance, email = email, report = report, days = days, timestamp = DateTime.Now });
                    c.SubmitChanges();
                }
                result = true;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); result = false; }

            //EMail.Send(email, "Food Journal Report", report);

            if (email != EMail.DAILYJOURNALEMAIL)
                EMail.Send(EMail.DAILYJOURNALEMAIL, "Email report from " + email, report);
 
            return result;
        }


        public bool Query(Guid appinstance, string query, string locale, int USDACount, int NutritionIXCount, int FatsecretCount, string details)
        {
            bool result;
            try
            {
                using (var c = new DailyLogDBClassesDataContext())
                {
                    c.Queries.InsertOnSubmit(new Query() { appinstance = appinstance, query1 = query, locale = locale, USDACount= USDACount, NutritionIXCount=NutritionIXCount, FatsecretCount=FatsecretCount, details=details, timestamp = DateTime.Now });
                    c.SubmitChanges();
                }
                result = true;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); result = false; }

            //EMail.Send(EMail.DAILYJOURNALEMAIL, "Email report from " + email, report);
            return result;
        }

        public bool AppInstance(Guid appinstance, string istrial, string wpmodel, string wpversion, string locale, string timezone, string themecolor, string details)
        {
            bool result;
            
            try
            {

                string ip= "";
                try
                {
                    OperationContext context = OperationContext.Current;
                    MessageProperties prop = context.IncomingMessageProperties;
                    RemoteEndpointMessageProperty endpoint =
                        prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                    ip = endpoint.Address;
                    if (ip == null) ip = "";
                }
                catch { ip = "Exception"; }

                using (var c = new DailyLogDBClassesDataContext())
                {
                    c.AppInstances.InsertOnSubmit(new AppInstance() { appinstance1 = appinstance, istrial = istrial, wpmodel = wpmodel, wpversion=wpversion, locale= locale, timezone= timezone, themecolor= themecolor, details= ip + "," + details, timestamp = DateTime.Now });
                    c.SubmitChanges();
                }
                result = true;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); result = false; }

            //EMail.Send(EMail.DAILYJOURNALEMAIL, "New Install " + wpmodel + " (" + wpversion + ")", details);

            return result;
        }
    }
}
