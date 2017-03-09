using FoodJournal.AppModel.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodJournal.AppModel;
using FoodJournal.Logging;
using FoodJournal.Values;

namespace FoodJournal.Model.Data
{

    public class FoodJournalDB : SQLiteConnection
    {

        const string sqliteFilename = "FoodDB.db3";

        private static object locker = new object();

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
				//string libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); ;
				string libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); ;
#else
                // we need to put in /Library/ on iOS5.1 to meet Apple's iCloud terms
                // (they don't want non-user-generated data in Documents)
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
                string libraryPath = Path.Combine(documentsPath, "../Library/"); // Library folder
#endif
                var path = Path.Combine(libraryPath, sqliteFilename);
				//var Exists = System.IO.File.Exists(path);
#endif

#endif
                return path;
            }
        }
        #endregion

#if WINDOWS_PHONE
        public System.Data.Linq.Table<FoodItemDO> rows1;
        public System.Data.Linq.Table<EntryDO> rows2;
        public System.Data.Linq.Table<RecipeDO> rows3;
        public System.Data.Linq.Table<IngredientDO> rows4;
#endif

        private FoodJournalDB(bool AutoCreate)
            : base(DatabaseFilePath, AutoCreate)
        {
            if (AutoCreate)
            {
                CreateTable<FoodItemDO>();
                CreateTable<EntryDO>();
                CreateTable<RecipeDO>();
                CreateTable<IngredientDO>();
            }
        }

		#if !WINDOWS_PHONE
		public bool DatabaseExists() {return System.IO.File.Exists (DatabaseFilePath);}
		public void DeleteDatabase() {System.IO.File.Delete (DatabaseFilePath);
		}

		#endif

        public static bool MigrationNeeded
        {
            get
            {  
				#if WINDOWS_PHONE
				FoodJournalDB db = new FoodJournalDB(false);
				if (!db.DatabaseExists()) return false;
				return true;
				#else
				return System.IO.File.Exists (DatabaseFilePath);
				#endif
            }
        }

        public static void Migrate()
        {

            try
            {
				#if WINDOWS_PHONE
				FoodJournalDB db = new FoodJournalDB(false);
				#else
				FoodJournalDB db = new FoodJournalDB(true);
				#endif

				if (!db.DatabaseExists()) return;

				if (db.Table<FoodItemDO>().Count() == 0 && db.Table<EntryDO>().Count() == 0)
                {
#if !WINDOWS_PHONE
					db.Close();
#endif
                    Logging.SessionLog.RecordMilestone("Deleting Legacy FoodItem DB", "");
                    db.DeleteDatabase();
                    return;
                }

				#if !WINDOWS_PHONE
				if (System.IO.File.Exists (DatabaseFilePath))
				{
					db.Close();
					System.IO.File.Copy(DatabaseFilePath, DatabaseFilePath.Replace(".db3",".bak"));
					db = new FoodJournalDB(true);
				}
				#endif
				
				DateTime start = DateTime.Now;

                int ExceptionCount = 0;
                Dictionary<int, int> itemmap = new Dictionary<int, int>();

                Logging.SessionLog.RecordTraceValue("Database migration - item count", db.Table<FoodItemDO>().Count().ToString());
                Logging.SessionLog.RecordTraceValue("Database migration - selection count", db.Table<EntryDO>().Count().ToString());
				 
                var AllItems = db.Table<FoodItemDO>().ToList();
                var AllSelections = db.Table<EntryDO>().ToList();

                List<FoodItemDO> ToDeleteItems = new List<FoodItemDO>();
                List<EntryDO> ToDeleteSelections = new List<EntryDO>();

                Dictionary<int, String> items = new Dictionary<int, string>();
                List<DateTime> savedates = new List<DateTime>();

                foreach (FoodItemDO item in AllItems)
                {
                    try
                    {

                        items.Add(item.Id, item.TextDB);

                        var NewItem = new FoodJournal.Model.FoodItem(item.TextDB, false);

                        NewItem.Culture = item.Culture;
                        NewItem.DescriptionDB = item.DescriptionDB;
                        NewItem.LastAmountDB = item.LastAmountDB;
                        NewItem.NutritionDB = item.NutritionDB;
                        NewItem.ServingSizesDB = item.ServingSizesDB;
                        NewItem.SourceID = item.SourceID;

                        Cache.MergeItem(NewItem);

                        ToDeleteItems.Add(item);

                    }
                    catch (Exception ex) { ExceptionCount++; LittleWatson.ReportException(ex); }
                }

                foreach (EntryDO entry in AllSelections)
                {
                    try
                    {

						var entryDate = DateTime.Parse(entry.Date);

                        var NewSelection = new FoodJournal.Model.Entry(entryDate, entry.Period, items[entry.FoodItemId]);
                        NewSelection.AmountScaleDB = entry.AmountScaleDB;
                        NewSelection.AmountSelectedDB = entry.AmountSelectedDB;

						Cache.AddEntry(NewSelection);

						ToDeleteSelections.Add(entry);

                        if (!savedates.Contains(entryDate))
                            savedates.Add(entryDate);

                    }
                    catch (Exception ex) { ExceptionCount++; LittleWatson.ReportException(ex); }
                }

                foreach (Period period in PeriodList.All)
                    foreach (DateTime date in savedates)
                        FoodJournalNoSQL.StartSaveDay(date, period);

                int i = 120;
                while (i-- > 0 && FoodJournalNoSQL.IsSaving())
                    System.Threading.Thread.Sleep(500);

                if (i == 0)
                {
                    SessionLog.RecordMilestone("Upgrade failed in 1 minute", AllSelections.Count.ToString());
                    throw new Exception("Upgrade failed in 1 minute");
                }

                foreach (EntryDO Selection in ToDeleteSelections)
                {
                    try
                    {
						#if WINDOWS_PHONE
                        db.Table<EntryDO>().DeleteOnSubmit(Selection);
                        db.SubmitChanges();
						#else
						db.Delete<EntryDO>(Selection.Id);
						#endif
                    }
                    catch (Exception ex) { db = new FoodJournalDB(false); ExceptionCount++; LittleWatson.ReportException(ex); }
                }

                foreach (FoodItemDO Item in ToDeleteItems)
                {
                    try
                    {
						#if WINDOWS_PHONE
                        db.Table<FoodItemDO>().DeleteOnSubmit(Item);
                        db.SubmitChanges();
						#else
						db.Delete<FoodItemDO>(Item.Id);
						#endif
                    }
                    catch (Exception ex) { db = new FoodJournalDB(false); ExceptionCount++; LittleWatson.ReportException(ex); }
                }

                Logging.SessionLog.RecordMilestone("Database migration - exception count", ExceptionCount.ToString());
                Logging.SessionLog.RecordMilestone("Database migration - duration", string.Format("{0} s", DateTime.Now.Subtract(start).TotalSeconds));

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

        }


#if LegacyDB

		#if !WINDOWS_PHONE
		private void DeleteObject<T>(EntryDO row) { me.Delete<EntryDO>(row.Id); }
		private void DeleteObject<T>(IngredientDO row) { me.Delete<IngredientDO>(row.Id); }
		#endif

		#if !WINDOWS_PHONE

		#if DEBUG
		public void DeleteAllForScreenshots() {
			DeleteAll<EntryDO>();
			DeleteAll<FoodItemDO>();
		}
		#endif

		#endif


        public static void Reset() { me = new FoodJournalDB(); }

        // ENTRY
        public static List<Entry> SelectEntriesByDateRangeSorted(DateTime startDateInclusive, DateTime endDateInclusive)
        {
            // where date in range, order by date, convert to Entry
            lock (locker)
            {
                return me.Table<EntryDO>()
                         .Where(e => e.Date >= startDateInclusive && e.Date <= endDateInclusive)
                         .OrderBy(e => e.Date)
						 .ToList()
                         .Select(e => new Entry(e))
                         .ToList();
            }
        }

		public static void SaveEntryDO(EntryDO row) { lock (locker) { if (row.Id == 0) me.Insert(row as object); else me.Update(row as object); } }
		public static void DeleteEntryDO(EntryDO row) { lock (locker) { me.DeleteObject<EntryDO>(row); } }
        public static List<EntryDO> SelectEntriesDOsForLegacy() { lock (locker) { return me.Table<EntryDO>().OrderBy(e => e.Date).ToList(); } }

        // FOODITEM
		public static List<FoodItem> SelectAllFoodItems() { lock (locker) { return me.Table<FoodItemDO>().ToList().Select(i => new FoodItem(i)).ToList(); } }
		public static List<Recipe> SelectAllRecipes() { lock (locker) { return me.Table<RecipeDO>().ToList().Select(i => new Recipe(i)).ToList(); } }

        public static void SaveFoodItemDO(IFoodItemDO row) { lock (locker) { if (row.Id == 0) me.Insert(row as object); else me.Update(row as object); } }
        // cant delete FoodItems

        public static int GetItemIDFromOldItemID(int OldId) { lock (locker) { var item = me.Table<FoodItemDO>().FirstOrDefault(i => i.OldDBID == OldId); return item == null ? -1 : item.Id; } }

        // INGREDIENT
        public static List<Ingredient> SelectIngredients(Recipe recipe)
        {
            lock (locker)
            {
                int id = recipe.GetFoodItemId();
                return me.Table<IngredientDO>()
                         .Where(i => i.RecipeId == id)
						 .ToList()
						 .Select(i => new Ingredient(recipe, i))
                         .ToList();
            }
        }
        public static void SaveIngredientDO(IngredientDO row) { lock (locker) { if (row.Id == 0) me.Insert(row as object); else me.Update(row as object); } }
        public static void DeleteIngredientDO(IngredientDO row) { lock (locker) { me.DeleteObject<IngredientDO>(row); } }

#endif

    }

}
