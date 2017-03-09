using FoodJournal.AppModel.SQLite;
using FoodJournal.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournal.Messages
{

    public class MessageQueueDB : SQLiteConnection
    {

        const string sqliteFilename = "QDB.db3";

        static MessageQueueDB me = new MessageQueueDB();

        static object locker = new object();

        #region GetDBPath
        public static string DatabaseFilePath
        {
            get
            {

#if NETFX_CORE
                var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, sqliteFilename);
#else

#if SILVERLIGHT
				// Windows Phone expects a local path, not absolute
	            var path = sqliteFilename;
#else

#if __ANDROID__
				// Just use whatever directory SpecialFolder.Personal returns
	            string libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); ;
#else
                // we need to put in /Library/ on iOS5.1 to meet Apple's iCloud terms
                // (they don't want non-user-generated data in Documents)
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
                string libraryPath = Path.Combine(documentsPath, "../Library/"); // Library folder
#endif
                var path = Path.Combine(libraryPath, sqliteFilename);
#endif

#endif
                return path;
            }
        }
        #endregion

#if WINDOWS_PHONE
        public System.Data.Linq.Table<MessageQueueRow> rows;
#endif

		#if !WINDOWS_PHONE
		private void DeleteObject<T>(MessageQueueRow row) { me.Delete<MessageQueueRow>(row.Id); }
		#endif

        /// <summary>
        /// Initializes a new instance of the Message Queue Database (phone local message cache)
        /// if the database doesn't exist, it will create the database and all the tables.
        /// </summary>
        private MessageQueueDB()
            : base(DatabaseFilePath, true)
        {
            // create the tables
            CreateTable<MessageQueueRow>();
        }

        public static int DeleteWhere(System.Linq.Expressions.Expression<Func<MessageQueueRow, bool>> predicate)
        {
            lock (locker)
            {
                var i = 0;
                var rows = me.Table<MessageQueueRow>().Where(predicate);
                foreach (var row in rows)
                {
                    me.DeleteObject<MessageQueueRow>(row);
                    i++;
                }
                return i;
            }
        }

        public static List<MessageQueueRow> SelectWhere(System.Linq.Expressions.Expression<Func<MessageQueueRow, bool>> predicate)
        {
            lock (locker)
            {
                return me.Table<MessageQueueRow>().Where(predicate).ToList();
            }
        }

        public static int Insert(MessageQueueRow row)
        {
            lock (locker)
            {
                return me.Insert(row as object);
            }
        }

        public static int Update(MessageQueueRow row)
        {
            lock (locker)
            {
                return me.Update(row as object);
            }
        }
    }
}

