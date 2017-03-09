using FoodJournal.Logging;
using FoodJournal.Values;
//using Microsoft.Phone.Data.Linq;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FoodJournal.Legacy.Model
{

    [System.Data.Linq.Mapping.Database]
    public partial class Database : DataContext
    {

        public Database() : base("Data Source = isostore:/FoodJournal.sdf") { }

        public Table<SelectableItem> AllItems;
        public Table<ItemSelection> AllSelections;

        public static bool DeDupeNeeded { get { return DateTime.Now < new DateTime(2014, 7, 14) && !FoodJournal.AppModel.AppStats.Current.DeDupeDone; } }

        public static void DeDupe()
        {

            try
            {

                List<FoodJournal.Model.Data.EntryDO> ToDelete = new List<FoodJournal.Model.Data.EntryDO>();
                List<int> set = new List<int>();

                var AllSel = FoodJournal.Model.Data.FoodJournalDB.SelectEntriesDOsForLegacy();
                DateTime lastdate = DateTime.Now.AddDays(1);

                foreach (var entry in AllSel)
                {
                    if (entry.Date != lastdate) { set.Clear(); lastdate = entry.Date; }

                    int itemid = (int)entry.Period * 10000 + entry.FoodItemId;
                    if (set.Contains(itemid)) ToDelete.Add(entry); else set.Add(itemid);
                }

                foreach (var entry in ToDelete)
                    FoodJournal.Model.Data.FoodJournalDB.DeleteEntryDO(entry);

                FoodJournal.AppModel.AppStats.Current.DeDupeDone = true;

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

            FoodJournal.Model.Data.FoodJournalDB.Reset();

        }

        public static bool MigrationNeeded
        {
            get
            {

                Database db = new Database();
                if (!db.DatabaseExists()) return false;

                return true;
            }
        }

        public static void Migrate()
        {

            try
            {
                Database db = new Database();
                if (!db.DatabaseExists()) return;

                if (db.AllItems.Count() == 0 && db.AllSelections.Count() == 0)
                {
                    Logging.SessionLog.RecordMilestone("Deleting Legacy DB", "");
                    db.DeleteDatabase();
                    return;
                }

                DateTime start = DateTime.Now;
                int ExceptionCount = 0;
                Dictionary<int, int> itemmap = new Dictionary<int, int>();

                Logging.SessionLog.RecordTraceValue("Database migration - item count", db.AllItems.Count().ToString());
                Logging.SessionLog.RecordTraceValue("Database migration - selection count", db.AllSelections.Count().ToString());

                var AllItems = db.AllItems.ToList();
                var AllSelections = db.AllSelections.ToList();

                List<FoodItem> ToDeleteItems = new List<FoodItem>();
                List<Entry> ToDeleteSelections = new List<Entry>();

                foreach (FoodItem Item in AllItems)
                {
                    try
                    {

                        if (FoodJournal.Model.Data.FoodJournalDB.GetItemIDFromOldItemID(Item.Id) == -1)
                        {

                            var NewItem = new FoodJournal.Model.Data.FoodItemDO();

                            NewItem.Culture = Item.Culture;
                            NewItem.DescriptionDB = Item.Description;
                            NewItem.LastAmountDB = Item.LastAmount.ToStorageString();
                            NewItem.NutritionDB = Item.PropertyValues;
                            NewItem.OldDBID = Item.Id;
                            NewItem.ServingSizesDB = Item.ServingSizesData;
                            NewItem.SourceID = Item.SourceID;
                            NewItem.TextDB = Item.Text;

                            FoodJournal.Model.Data.FoodJournalDB.SaveFoodItemDO(NewItem);

                            itemmap.Add(Item.Id, NewItem.Id);

                        }

                        ToDeleteItems.Add(Item);

                    }
                    catch (Exception ex) { ExceptionCount++; LittleWatson.ReportException(ex); }
                }

                foreach (Entry Selection in AllSelections)
                {
                    try
                    {

                        var NewSelection = new FoodJournal.Model.Data.EntryDO();
                        NewSelection.AmountScaleDB = Selection.AmountScale;
                        NewSelection.AmountSelectedDB = Selection.AmountSelected.ToStorageString();
                        NewSelection.Date = Selection.Date;
                        NewSelection.FoodItemType = FoodJournal.Model.Data.FoodItemType.Food;
                        NewSelection.Period = Selection.Period;

                        if (!itemmap.ContainsKey(Selection.ItemId))
                            itemmap.Add(Selection.ItemId, FoodJournal.Model.Data.FoodJournalDB.GetItemIDFromOldItemID(Selection.ItemId));

                        NewSelection.FoodItemId = itemmap[Selection.ItemId];

                        if (NewSelection.FoodItemId > 0)
                            FoodJournal.Model.Data.FoodJournalDB.SaveEntryDO(NewSelection);
                        else
                            SessionLog.RecordTraceValue("Missing FoodItem converting ItemSelection", Selection.ItemId.ToString());

                        ToDeleteSelections.Add(Selection);

                    }
                    catch (Exception ex) { ExceptionCount++; LittleWatson.ReportException(ex); }
                }


                foreach (Entry Selection in ToDeleteSelections)
                {
                    try
                    {
                        db.AllSelections.DeleteOnSubmit(Selection);
                        db.SubmitChanges();
                    }
                    catch (Exception ex) { db = new Database(); ExceptionCount++; LittleWatson.ReportException(ex); }
                }

                foreach (FoodItem Item in ToDeleteItems)
                {
                    try
                    {
                        db.AllItems.DeleteOnSubmit(Item);
                        db.SubmitChanges();
                    }
                    catch (Exception ex) { db = new Database(); ExceptionCount++; LittleWatson.ReportException(ex); }
                }

                Logging.SessionLog.RecordTraceValue("Database migration - exception count", ExceptionCount.ToString());
                Logging.SessionLog.RecordMilestone("Database migration - duration", string.Format("{0} s", DateTime.Now.Subtract(start).TotalSeconds));

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

        }

    }
}
