using FoodJournal.FoodJournalService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;

namespace FoodJournal.AppModel
{
    public class SyncService
    {
				
		//static string URL = "http://192.168.1.4:81/api/foodsync";
		static string URL = "http://dailylogservice.cloudapp.net/api/foodsync";

		public static async void Post(List<Package> packages)
		{
			var url = string.Format("{0}/{1}?ClientTime={2}", URL, AppStats.Current.AppInstance, DateTime.Now.ToString("s"));
			using (HttpClient client = new HttpClient ())
			{
				//#if DEBUG
				string sData =  Newtonsoft.Json.JsonConvert.SerializeObject(packages);
				HttpContent content = new System.Net.Http.StringContent(sData, System.Text.Encoding.UTF8,  "application/json") ;
				var result = await client.PostAsync(url,content);
			}
		}

		public static async Task<List<Package>> Get(int SkipRows, String LastSync)
		{
			var url = string.Format("{0}/{1}?ClientTime={2}&UserID={3}&Skip={4}&Version={5}&LastSync={6}", URL, AppStats.Current.AppInstance, DateTime.Now.ToString("s"), AppStats.Current.UserId, SkipRows, AppStats.Current.Version, LastSync);

            using (HttpClient client = new HttpClient())
            {
                var result = await client.GetAsync(url);
                var data = await result.Content.ReadAsStringAsync();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Package>>(data);
            }
		}
    }

	public class Package
	{
		public string AppInstance { get; set; }
		public string Type { get; set; }
		public string Key { get; set; }
		public string Contents { get; set; }
		public DateTime ClientTime { get; set; }
	}
}
