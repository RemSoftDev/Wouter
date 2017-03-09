using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournalServiceWebRole.SyncService
{
    public static class Extensions
    {

        public static string ToKey(this Guid AppinstanceID) { return AppinstanceID.ToString("D"); }

        public static TimeSpan GetClientLag(this DateTime ClientTime) { return DateTime.Now - ClientTime; }  // ie. Server = 2pm. Client = 1pm. Client lags by 1 hour
        public static DateTime ToServerTime(this DateTime ClientTime, TimeSpan ClientLag) { return (ClientTime == DateTime.MinValue) ? DateTime.MinValue : ClientTime + ClientLag; }
        public static DateTime ToClientTime(this DateTime ClientTime, TimeSpan ClientLag) { return (ClientTime == DateTime.MinValue) ? DateTime.MinValue : ClientTime - ClientLag; }


    }
}
