using System;
using Android.Content;
using Android.App;

namespace FoodJournal.Android15
{

	public enum RecurrenceInterval {Daily}

	public struct Reminder
	{
		public string Name;
		public string Title;
		public string Content;
		public DateTime BeginTime;
		public DateTime ExpirationTime;
		public RecurrenceInterval RecurrenceType;
		public Uri NavigationUri;
		public Reminder(string name) {this.Name = name; Title = null;Content = null;BeginTime = DateTime.MinValue;ExpirationTime = DateTime.MinValue;RecurrenceType = RecurrenceInterval.Daily;NavigationUri = null;}

		public void ToIntent(Intent intent)
		{
			intent.PutExtra("Name",Name);
			intent.PutExtra("Title",Title);
			intent.PutExtra("Content",Content);
			intent.PutExtra("BeginTime",BeginTime.ToString("O"));
			intent.PutExtra("ExpirationTime",ExpirationTime.ToString("O"));
			intent.PutExtra("RecurrenceType",RecurrenceType.ToString());
			//intent.PutExtra("NavigationUri",NavigationUri.ToString());
		}

		public static Reminder FromIntent(Intent intent)
		{
			Reminder result = new Reminder ();
			result.Name = intent.GetStringExtra ("Name");
			result.Title = intent.GetStringExtra ("Title");
			result.Content = intent.GetStringExtra ("Content");
			DateTime.TryParse( intent.GetStringExtra ("BeginTime"), out result.BeginTime);
			DateTime.TryParse( intent.GetStringExtra ("ExpirationTime"), out result.ExpirationTime);
			Enum.TryParse(intent.GetStringExtra ("RecurrenceType"), out result.RecurrenceType);
			//result.NavigationUri = new Uri(intent.GetStringExtra ("NavigationUri"));
			return result;
		}

	}

	public class ScheduledActionService
	{

		public static object Find(string name)		{			return true;		}
		public static void Remove(string name)		{			AlarmService.StartCancel (name);		}
		public static void Add(Reminder reminder)		{			AlarmService.StartCreate (reminder);		}

	}
}

