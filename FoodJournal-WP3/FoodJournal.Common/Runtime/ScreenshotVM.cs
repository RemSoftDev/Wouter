//#if !WINDOWS_PHONE
//#define SCREENSHOT
//#endif
using System.Linq;
using FoodJournal.AppModel;
using FoodJournal.Model;
using FoodJournal.ResourceData;
using FoodJournal.Values;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using Android.Content;
//using FoodJournal.Android15;
using FoodJournal.Resources;
using FoodJournal.Runtime;
//using FoodJournal.Android15;
//using Android.Content;

namespace FoodJournal.ViewModels
{
#if SCREENSHOT
    public class ScreenshotVM
    {
        private const int wait = 1100;

//		public static List<String> SupportedCultures2 = new List<string>() {
//			"ar-SA", "da-DK", "de-DE", "nl-NL"
//		};

//		public static List<String> SupportedCultures2 = new List<string>() {
//			"ar-SA", "bg-BG", "ca-ES", "cs-CZ", "da-DK", "de-DE", "el-GR", "en-GB", "en-US", "es-ES", "es-MX", "et-EE", "fa-IR", "fi-FI", "fr-FR", "he-IL",
//			"hi-IN", "hu-HU", "id-ID", "it-IT", "ja-JP", "ko-KR", "lt-LT", "lv-LV", "ms-MY", "nb-NO", "nl-NL", "pl-PL", "pt-BR", "pt-PT", "ro-RO", "ru-RU",
//			"sk-SK", "sl-SI", "sv-SE", "th-TH", "tr-TR", "uk-UA", "vi-VN", "zh-CN", "zh-TW"        
//		};
		public static List<String> SupportedCultures2 = new List<string>() {
			 "ko-KR", "lt-LT", "lv-LV", "ms-MY", "nb-NO", "nl-NL", "pl-PL", "pt-BR", "pt-PT", "ro-RO", "ru-RU",
			"sk-SK", "sl-SI", "sv-SE", "th-TH", "tr-TR", "uk-UA", "vi-VN", "zh-CN", "zh-TW"        
		};

		public static Context Context { get; set; }
        public static bool InScreenshot = false;

        private static string culturetl = "sdas";
        private static Entry detailentry;
        private static int cultureid = -1;

        public static void SetCulture(string lang)
        {
            AppStats.appForceCulture = lang;
            App.SetThreadCulture();
            Property.Reset();
            AppStats.Current.Culture = AppStats.appForceCulture;
            AppStats.Current.ResetCulture();
            AppStats.Current.AdShows = false;
            UserSettings.Current.SelectedProperties = new List<Property>() { StandardProperty.Calories, StandardProperty.TotalFat, StandardProperty.Carbs, StandardProperty.Protein };

            culturetl = AppStats.appForceCulture.ToLower();

            if (culturetl == "nb-no") culturetl = "no";
            if (culturetl == "zh-tw") culturetl = "zt";

            if (culturetl == "en-gb") culturetl = "gb";
            if (culturetl == "es-mx") culturetl = "mx";
            if (culturetl == "pt-br") culturetl = "br";

            culturetl = culturetl.Substring(0, 2);
            if (!ResourceDatabase2.SupportedCultures2.Contains(culturetl))
                throw new ArgumentOutOfRangeException("culture");
            System.Diagnostics.Debug.WriteLine(string.Format("Applied {0} {1} {2}", cultureid, AppStats.appForceCulture, culturetl));
        }

        private static string getfilename(int id) { if (culturetl == null) return null; return string.Format("shots/screen {0} {1}.jpg", culturetl, id.ToString()); }

        private static void SaveScreenshotEntry(Entry entry)
        {
            entry.Save();
            //Cache.AddEntry(entry);
            //Cache.AddItem(entry.Item);
        }

        private static void SaveScreenshotEntry4(Entry entry, float p1, float p2, float p3, float p4)
        {

            var newItem = new FoodItem(entry.EntryText + "a", false);
            newItem.ServingSizes.Add("1 cup", "100 g");
            newItem.Values[StandardProperty.Calories] = entry.GetPropertyValue(StandardProperty.Calories).ToSingle() * p1;
            newItem.Values[StandardProperty.Protein] = entry.GetPropertyValue(StandardProperty.Protein).ToSingle() * p1;
            newItem.Values[StandardProperty.TotalFat] = entry.GetPropertyValue(StandardProperty.TotalFat).ToSingle() * p1;
            newItem.Values[StandardProperty.Carbs] = entry.GetPropertyValue(StandardProperty.Carbs).ToSingle() * p1;
            var newEntry = new Entry(entry.Date, Period.Breakfast, newItem);
            newEntry.Save();

            newItem = new FoodItem(entry.EntryText + "b", false);
            newItem.ServingSizes.Add("1 cup", "100 g");
            newItem.Values[StandardProperty.Calories] = entry.GetPropertyValue(StandardProperty.Calories).ToSingle() * p2;
            newItem.Values[StandardProperty.Protein] = entry.GetPropertyValue(StandardProperty.Protein).ToSingle() * p2;
            newItem.Values[StandardProperty.TotalFat] = entry.GetPropertyValue(StandardProperty.TotalFat).ToSingle() * p2;
            newItem.Values[StandardProperty.Carbs] = entry.GetPropertyValue(StandardProperty.Carbs).ToSingle() * p2;
            newEntry = new Entry(entry.Date, Period.Lunch, newItem);
            newEntry.Save();

            newItem = new FoodItem(entry.EntryText + "c", false);
            newItem.ServingSizes.Add("1 cup", "100 g");
            newItem.Values[StandardProperty.Calories] = entry.GetPropertyValue(StandardProperty.Calories).ToSingle() * p3;
            newItem.Values[StandardProperty.Protein] = entry.GetPropertyValue(StandardProperty.Protein).ToSingle() * p3;
            newItem.Values[StandardProperty.TotalFat] = entry.GetPropertyValue(StandardProperty.TotalFat).ToSingle() * p3;
            newItem.Values[StandardProperty.Carbs] = entry.GetPropertyValue(StandardProperty.Carbs).ToSingle() * p3;
            newEntry = new Entry(entry.Date, Period.Dinner, newItem);
            newEntry.Save();

            newItem = new FoodItem(entry.EntryText + "d", false);
            newItem.ServingSizes.Add("1 cup", "100 g");
            newItem.Values[StandardProperty.Calories] = entry.GetPropertyValue(StandardProperty.Calories).ToSingle() * p4;
            newItem.Values[StandardProperty.Protein] = entry.GetPropertyValue(StandardProperty.Protein).ToSingle() * p4;
            newItem.Values[StandardProperty.TotalFat] = entry.GetPropertyValue(StandardProperty.TotalFat).ToSingle() * p4;
            newItem.Values[StandardProperty.Carbs] = entry.GetPropertyValue(StandardProperty.Carbs).ToSingle() * p4;
            newEntry = new Entry(entry.Date, Period.Snack, newItem);
            newEntry.Save();
        }

        public static Task SetupScreenshotsData()
        {
            return Task.Run(() =>
            {
                Task.Delay(2000).Wait();
                
                string[] titles = new string[9];
                titles[0] = AppResources.Screenshot_1a;
                titles[1] = AppResources.Screenshot_1b;
                titles[2] = AppResources.Screenshot_2a;
                titles[3] = AppResources.Screenshot_2b;
                titles[4] = AppResources.Screenshot_3a;
                titles[5] = AppResources.Screenshot_3b;

                titles[6] = AppResources.Screenshot_H1;
                titles[7] = AppResources.Screenshot_H2;
                titles[8] = AppResources.Screenshot_H3;

                Cache.ResetForScreenshots();

                FoodItem newItem;
                Entry newEntry;

                newItem = new FoodItem(titles[0], false);
                newItem.ServingSizes.Add(titles[1], "100 g");
                newItem.Values[StandardProperty.Calories] = 112;
                newItem.Values[StandardProperty.Protein] = 1;
                newItem.Values[StandardProperty.Carbs] = 26;
                newEntry = new Entry(DateTime.Now.Date, Period.Breakfast, newItem);
                SaveScreenshotEntry(newEntry);

                detailentry = newEntry;

                newItem = new FoodItem(titles[4], false);
                newItem.ServingSizes.Add(titles[5], "100 g");
                newItem.Values[StandardProperty.Calories] = 63;
                newItem.Values[StandardProperty.TotalFat] = 4;
                newItem.Values[StandardProperty.Protein] = 6;
                newEntry = new Entry(DateTime.Now.Date, Period.Breakfast, newItem);
                SaveScreenshotEntry(newEntry);

                newItem = new FoodItem(titles[6], false);
                newEntry = new Entry(DateTime.Now.AddDays(-1).Date, Period.Breakfast, newItem);
                SaveScreenshotEntry(newEntry);

                newItem = new FoodItem(titles[7], false);
                newEntry = new Entry(DateTime.Now.AddDays(-1).Date, Period.Breakfast, newItem);
                SaveScreenshotEntry(newEntry);

                newItem = new FoodItem(titles[8], false);
                newEntry = new Entry(DateTime.Now.AddDays(-1).Date, Period.Breakfast, newItem);
                SaveScreenshotEntry(newEntry);

                newItem = new FoodItem("prev 1", false);
                newItem.ServingSizes.Add("1 cup", "100 g");
                newItem.Values[StandardProperty.Calories] = 2321;
                newItem.Values[StandardProperty.Protein] = 54; // 50-75 / day
                newItem.Values[StandardProperty.TotalFat] = 63; // 44-77
                newItem.Values[StandardProperty.Carbs] = 75; // losing weight: 20-70, normal: 180-230
                newEntry = new Entry(DateTime.Now.AddDays(-1).Date, Period.Lunch, newItem);
                SaveScreenshotEntry4(newEntry, 0.2f, 0.3f, 0.4f, 0.1f);

                newItem = new FoodItem("prev 2", false);
                newItem.ServingSizes.Add("1 cup", "100 g");
                newItem.Values[StandardProperty.Calories] = 2132;
                newItem.Values[StandardProperty.Protein] = 66; // 50-75 / day
                newItem.Values[StandardProperty.TotalFat] = 77; // 44-77
                newItem.Values[StandardProperty.Carbs] = 75; // losing weight: 20-70, normal: 180-230
                newEntry = new Entry(DateTime.Now.AddDays(-2).Date, Period.Lunch, newItem);
                SaveScreenshotEntry4(newEntry, 0.22f, 0.19f, 0.38f, 0.12f);

                newItem = new FoodItem("prev 3", false);
                newItem.ServingSizes.Add("1 cup", "100 g");
                newItem.Values[StandardProperty.Calories] = 2432;
                newItem.Values[StandardProperty.Protein] = 28; // 50-75 / day
                newItem.Values[StandardProperty.TotalFat] = 48; // 44-77
                newItem.Values[StandardProperty.Carbs] = 75; // losing weight: 20-70, normal: 180-230
                newEntry = new Entry(DateTime.Now.AddDays(-3).Date, Period.Lunch, newItem);
                SaveScreenshotEntry4(newEntry, 0.24f, 0.34f, 0.44f, 0f);

                newItem = new FoodItem("prev 4", false);
                newItem.ServingSizes.Add("1 cup", "100 g");
                newItem.Values[StandardProperty.Calories] = 2312;
                newItem.Values[StandardProperty.Protein] = 75; // 50-75 / day
                newItem.Values[StandardProperty.TotalFat] = 52; // 44-77
                newItem.Values[StandardProperty.Carbs] = 75; // losing weight: 20-70, normal: 180-230
                newEntry = new Entry(DateTime.Now.AddDays(-4).Date, Period.Lunch, newItem);
                SaveScreenshotEntry4(newEntry, 0.21f, 0.32f, 0.44f, 0.12f);

                newItem = new FoodItem("prev 5", false);
                newItem.ServingSizes.Add("1 cup", "100 g");
                newItem.Values[StandardProperty.Calories] = 2200;
                newItem.Values[StandardProperty.Protein] = 66; // 50-75 / day
                newItem.Values[StandardProperty.TotalFat] = 60; // 44-77
                newItem.Values[StandardProperty.Carbs] = 75; // losing weight: 20-70, normal: 180-230
                newEntry = new Entry(DateTime.Now.AddDays(-5).Date, Period.Lunch, newItem);
                SaveScreenshotEntry4(newEntry, 0.19f, 0.12f, 0.62f, 0.12f);

                newItem = new FoodItem("prev 6", false);
                newItem.ServingSizes.Add("1 cup", "100 g");
                newItem.Values[StandardProperty.Calories] = 2412;
                newItem.Values[StandardProperty.Protein] = 73; // 50-75 / day
                newItem.Values[StandardProperty.TotalFat] = 53; // 44-77
                newItem.Values[StandardProperty.Carbs] = 75; // losing weight: 20-70, normal: 180-230
                newEntry = new Entry(DateTime.Now.AddDays(-6).Date, Period.Lunch, newItem);
                SaveScreenshotEntry4(newEntry, 0.21f, 0.3f, 0.41f, 0.1f);
            });
        }

        public static async void CaptureScreens(Context context)
        {
            Context = context;
            ScreenshotVM.InScreenshot = true;

            foreach (var item in SupportedCultures2)//.Take(3))
            {

                SetCulture(item);
                await SetupScreenshotsData();
                await CaptureToday();
                await CaptureEntry();
                await CaptureJournal();
                await CaptureReport();
                await CaptureSetting();

            }
        }

        static async Task CaptureToday()
        {
            Intent i = new Intent(Context, typeof(MainActivity));
            i.PutExtra("Screen", "Today");
            //  i.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask | ActivityFlags.ClearTop);
            Context.StartActivity(i);
            await Wait();
            Screenshot.MakeShot(Navigate.screenshotScreen.Window.DecorView.RootView, getfilename(1));
        }
        static async Task CaptureEntry()
        {
            Navigate.ToEntryDetail(detailentry);
            await Wait();
            Screenshot.MakeShot(Navigate.screenshotScreen.Window.DecorView.RootView, getfilename(2));
        }

        static async Task CaptureJournal()
        {
            Intent i = new Intent(Context, typeof(MainActivity));
            i.PutExtra("Screen", "Journal");
            //   i.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask | ActivityFlags.ClearTop);
            Context.StartActivity(i);
            await Wait();
            Screenshot.MakeShot(Navigate.screenshotScreen.Window.DecorView.RootView, getfilename(3));
        }

        static async Task CaptureReport()
        {
            Intent i = new Intent(Context, typeof(MainActivity));
            i.PutExtra("Screen", "Report");
            //    i.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask | ActivityFlags.ClearTop);
            Context.StartActivity(i);
            await Wait();
            Screenshot.MakeShot(Navigate.screenshotScreen.Window.DecorView.RootView, getfilename(4));
        }
        static async Task CaptureSetting()
        {
            Navigate.ToSettingsPage(true);
            await Wait();
            Screenshot.MakeShot(Navigate.screenshotScreen.Window.DecorView.RootView, getfilename(5));
        }

        static Task Wait()
        {
            return Task.Delay(wait);
        }
    }

#endif

}
