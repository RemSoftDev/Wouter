using System;


namespace FoodJournal.iOS
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



	}

	public class ScheduledActionService
	{

		public static object Find(string name)		{			return true;		}
		public static void Remove(string name)		{					}
		public static void Add(Reminder reminder)		{					}

	}
}

