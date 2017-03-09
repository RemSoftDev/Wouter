using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoodJournal.Parsing;
using System.Windows;
using FoodJournal.Logging;
using FoodJournal.Model;
using FoodJournal.Values;
using FoodJournal.WinPhone.Common.Resources;
using System.Threading;
using FoodJournal.AppModel;
using FoodJournal.Model.Data;
#if WINDOWS_PHONE
using System.Windows.Resources;
#endif
#if ANDROID
using FoodJournal.Android15;
#endif

namespace FoodJournal.ResourceData
{

    public struct ResourceRecord2
    {
        public int IndexPos;
        public string DBCulture;
        public int Row;
        public string Meal;
        public string Name;
        public string Brand;
        public int DataPos;

        public ResourceRecord2(string DBCulture, int IndexPos, string[] fields)
        {
            this.DBCulture = DBCulture;
            this.IndexPos = IndexPos;

            Row = Ints.ParseStorage(fields[0]);

            if (fields.Length != 5)
            {
                Row = 0;
                Name = null;
                Meal = null;
                Brand = null;
                DataPos = 0;
                return;
            }

            Meal = fields[1];
            Name = fields[2];
            Brand = fields[3];
            DataPos = Ints.ParseStorage(fields[4]);

            if (Name != null && Name.Length == 0) Name = null;
            if (Meal != null && Meal.Length == 0) Meal = null;
            if (Brand != null && Brand.Length == 0) Brand = null;

            if (Name != null && Brand != null)
                Name += " (" + Brand + ")";

            if (Meal == null)
            {
                if (Brand == null && Row % 3 != 0) Meal = "P";
                if (Brand != null && Row % 5 != 0) Meal = "P";
            }

        }
    }

    public class ResourceDatabase2
    {
#if ANDROID
        public static Android.Content.Res.AssetManager assetManager;

        //public static List<String> SupportedCultures2 = new List<string>() { "de", "en", "es", "fr", "it", "nl", "pt", "ru", "th", "zh" };
		public static List<String> SupportedCultures2 = new List<string>() { "ar", "bg", "ca", "cs", "da", "de", "el", "en", "gb", "es", "mx", "et", "fa", 
			"fi", "fr", "he", "hi", "hu", "id", "it", "ja", "ko", "lt", "lv", "ms", "no", 
			"nl", "pl", "br", "pt", "ro", "ru", "sk", "sl", "sv", "th", "tr", "uk", "vi", 
			"zh", "zt"};

#else

		public static List<String> SupportedCultures2 = new List<string>() { "ar", "bg", "ca", "cs", "da", "de", "el", "en", "gb", "es", "mx", "et", "fa", 
                                                                            "fi", "fr", "he", "hi", "hu", "id", "it", "ja", "ko", "lt", "lv", "ms", "no", 
                                                                            "nl", "pl", "br", "pt", "ro", "ru", "sk", "sl", "sv", "th", "tr", "uk", "vi", 
                                                                            "zh", "zt"};
#endif

        private static Dictionary<String, ResourceDatabase2> set;

        private string culture;
        private string index;
        private int indexlen;
        private string data;

        private const char RECORDDELIMIT = '\n';
        private const char FIELDDELIMIT = '\t';
        private const string INDEXHEADERROW = "Row	Meal	Name	Brand	Pos";
        private const string DATAHEADERROW = "Row	ID	AllWeights	AllProperties";

        public static ResourceDatabase2 Default { get { return FromCulture(AppStats.Current.DatabaseCulture); } }
        public static ResourceDatabase2 FromCulture(String culture)
        {
            if (set == null) set = new Dictionary<string, ResourceDatabase2>();
            culture = culture.ToLower();
            if (!SupportedCultures2.Contains(culture)) culture = "en";
            // throw new ArgumentOutOfRangeException("Culture");
            if (!set.ContainsKey(culture))
            {
                set.Add(culture, new ResourceDatabase2(culture));
            }
            return set[culture];
        }

        public static FoodItem FoodItemFromResourceRecord(ResourceRecord2 record)
        {
            return FromCulture(record.DBCulture).ItemFromResourceRecord(record);
        }

        private ResourceDatabase2(string culture)
        {
            if (!SupportedCultures2.Contains(culture)) throw new ArgumentOutOfRangeException("Culture");
            this.culture = culture;
        }

        private static string LoadResourceFile(string Filename)
        {
            try
            {
#if WINDOWS_PHONE
                //StreamResourceInfo zipInfo = Application.GetResourceStream(new Uri("ResourceData/data-V2.zip", UriKind.RelativeOrAbsolute));
                //StreamResourceInfo package = Application.GetResourceStream(zipInfo, new Uri(Filename, UriKind.Relative));
                StreamResourceInfo package = Application.GetResourceStream(new Uri("Common/ResourceData/" + Filename, UriKind.Relative));
#if DEBUG
                System.IO.StreamReader r = new System.IO.StreamReader(package.Stream, new UTF8Encoding(true, true));
#else
                System.IO.StreamReader r = new System.IO.StreamReader(package.Stream);
#endif
                return r.ReadToEnd();
#else

#if ANDROID
                var stream = assetManager.Open(Filename, Android.Content.Res.Access.Streaming);
                System.IO.StreamReader r = new System.IO.StreamReader(stream);
                return r.ReadToEnd();
#else
                return "";
#endif
#endif
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex, Filename);
                throw;
            }
        }

        private string Index
        {
            get
            {
                if (index == null)
                    using (SessionLog.NewScope("Loading index", culture))
                    {
                        index = LoadResourceFile("V2-" + culture + "-Index.txt");
                        var end = index.IndexOf(RECORDDELIMIT);
                        var headerrow = index.Substring(0, end - 1);
                        if (headerrow != INDEXHEADERROW) throw new Exception("Invalid File Format");
                        indexlen = index.Length;
                    }
                return index;
            }
        }

        private string Data
        {
            get
            {
                if (data == null)
                    using (SessionLog.NewScope("Loading data", culture))
                    {
                        data = LoadResourceFile("V2-" + culture + "-Data.txt");
                        var end = data.IndexOf(RECORDDELIMIT);
                        var headerrow = data.Substring(0, end - 1);
                        if (headerrow != DATAHEADERROW) throw new Exception("Invalid File Format");
                    }
                return data;
            }
        }

        internal int NextRecordPos(int pos)
        {
            return Index.IndexOf(RECORDDELIMIT, pos) + 1;
        }

        internal int NextHit(int pos, string term)
        {
            if (indexlen > 0 && pos >= indexlen) return -1;
            pos = index.IndexOf(term, pos + 1, StringComparison.CurrentCultureIgnoreCase);
            // TODO: make sure pos is part of the name field?
            return pos;
        }

        internal ResourceRecord2 RecordFromPos(int IndexPos)
        {
            if (IndexPos >= indexlen) return new ResourceRecord2() { IndexPos = 0 }; ;
            int startofrecord = Index.LastIndexOf(RECORDDELIMIT, IndexPos) + 1;
            int endofrecord = index.IndexOf(RECORDDELIMIT, IndexPos) - 1;
            if (endofrecord <= 0) return new ResourceRecord2() { IndexPos = 0 };
            return new ResourceRecord2(culture, IndexPos, index.Substring(startofrecord, endofrecord - startofrecord).Split(FIELDDELIMIT));
        }

        public FoodItem ItemFromResourceRecord(ResourceRecord2 record)
        {

            if (record.IndexPos <= 0) return null;
            if (record.DataPos <= 0) return null;

            int endofrecord = Data.IndexOf(RECORDDELIMIT, record.DataPos);

            if (endofrecord <= 0) return null;
            string row = data.Substring(record.DataPos, endofrecord - record.DataPos - 1);

            FoodItem result = new FoodItem(record.Name, false);
            //result.TextDB = record.Name;
            result.DescriptionDB = null;
            result.Culture = record.DBCulture;
            result.CommonMeal = record.Meal;

            var values = row.Split(FIELDDELIMIT);

            //Row	ID	AllWeights	AllProperties

            if (Ints.ParseStorage(values[0]) != record.Row) throw new ArgumentOutOfRangeException("pos");
            result.SourceID = values[1];
            //result.ServingSizesData = values[2];
            string ss = values[2];
            string pp = values[3];
            result.NutritionDB = pp;
            result.ServingSizesDB = ss.Replace(",", ".");

            return result;

        }

    }

}
