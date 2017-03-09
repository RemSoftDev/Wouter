using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournalServiceWebRole.SyncService
{

    public class UserRow : TableEntity
    {

        public enum UserIDClass
        {
            none,
            WP,
            AndroidShort,
            AndroidLong
        }
        
        public UserRow() { }
        public UserRow(String UserID, Guid AppinstanceID) { this.PartitionKey = UserID; this.RowKey = AppinstanceID.ToKey(); }

        public DateTime ServerTime { get; set; }

        public Guid AppinstanceID { get { return new Guid(RowKey); } }//set { this.RowKey = value.ToKey(); } }

        public static bool IsValidUserID(string UserID)
        {
            if (UserID == "-") return false;
            return true;
        }

        public static UserIDClass GetClass(String UserID)
        {
            if (string.IsNullOrEmpty(UserID)) return UserIDClass.none;
            if (UserID == "0123456789ABCDEF") return UserIDClass.none;
            if (UserID.Length > 40 && UserID.EndsWith("=")) return UserIDClass.WP;
            if (UserID.Contains("-")) return UserIDClass.AndroidLong;
            return UserIDClass.AndroidShort;
        }

        public static String GetAlternativeUserID(String UserID)
        {
            if (GetClass(UserID) != UserIDClass.AndroidLong) return null;
            return UserID.Substring(0, UserID.IndexOf("-"));
        }

    }

}
