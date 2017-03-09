#if WINDOWS_PHONE

using FoodJournal.Logging;
using Microsoft.Phone.Data.Linq;
using System;
using System.Linq;
using System.Diagnostics;

namespace FoodJournal.AppModel.SQLite
{

    ////[PrimaryKey, AutoIncrement]
    //[Column(IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]

    public class AutoIncrementAttribute : Attribute { }
    public class PrimaryKeyAttribute : Attribute { }
    //public class TableAttribute : Attribute { public TableAttribute(string name) { } }

    public class SQLiteConnection : System.Data.Linq.DataContext
    {

        public SQLiteConnection(string url, bool AutoCreate)
            : base("Data Source = isostore:/" + url.Replace(".db3", ".sdf"))
        {
            if (AutoCreate)
                if (!DatabaseExists()) this.CreateDatabase();
        }

        public void CreateTable<T>() where T : class
        {
            bool TableExists = this.Table<T>() != null;
            if (TableExists)
                try { var x = this.Table<T>().Count(); }
                catch { TableExists = false; }

            if (!TableExists)
            {
                DatabaseSchemaUpdater su = Microsoft.Phone.Data.Linq.Extensions.CreateDatabaseSchemaUpdater(this);
                su.AddTable<T>();
                su.Execute();
            }
        }

        public int Insert(object row)
        {
            AssertChanges(0);
            //this.ExecuteDynamicInsert(row);
            Type t = row.GetType();
            var table = this.GetTable(t);
            table.InsertOnSubmit(row);
            this.SubmitChanges();
            return 1;
        }

        public int Update(object row)
        {
            AssertChanges(1);
            //this.ExecuteDynamicUpdate(row);
            Type t = row.GetType();
            var table = this.GetTable(t);
            //table.Attach(row, true);
            this.SubmitChanges();
            return 1;
        }

        public void DeleteObject<T>(object row) where T : class
        {
            AssertChanges(0);
            //this.ExecuteDynamicDelete(row);
            var table = this.Table<T>();
            table.DeleteOnSubmit(row as T);
            this.SubmitChanges();
        }

        public System.Data.Linq.Table<T> Table<T>() where T : class { return GetTable<T>(); }

        public void AssertChanges(int count)
        {
            try
            {
                var cs = this.GetChangeSet();
                if (cs.Deletes.Count == 0 && cs.Inserts.Count == 0 && cs.Updates.Count == count) return;

#if DEBUG
                Debugger.Break();
#endif
                SessionLog.ReportException(new Exception("The Database is Dirty when expected Clean"), "AssertChanges");

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

    }


}

#endif