//using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Values;
using FoodJournal.Extensions;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FoodJournal.AppModel;
using FoodJournal.Logging;
using FoodJournal.Resources;
using FoodJournal.WinPhone.Common.AppModel.Data.Serialization;
using FoodJournal.Values;
using FoodJournal.iOS;

#if WINDOWS_PHONE
using Microsoft.Phone.Scheduler;
#elif ANDROID
using FoodJournal.Android15;
#elif __IOS__

#endif

namespace FoodJournal
{

	[DataContract]
	public class ReminderData
	{
		[DataMember]
		public bool Checked;
		[DataMember]
		public DateTime Time;
	}

	[DataContract]
	public class UserSettings
	{

		public static string[] CustomProperties = new string[] { "Water (g)" };
		//public static List<Property> SelectedProperties = new List<Property>() { StandardProperty.Calories, StandardProperty.TotalFat, StandardProperty.Carbs, StandardProperty.Protein };

		private bool DoneLoading = false;

		public UserSettings()
		{
			// defaults
			_EnableLiveTile = true;
			_SnackReminder = true;
		}

		private void VerifyDefaults()
		{
			if (_BreakfastReminderTime.Hour == 0) _BreakfastReminderTime = new DateTime(1, 1, 1, 10, 0, 0);
			if (_LunchReminderTime.Hour == 0) _LunchReminderTime = new DateTime(1, 1, 1, 14, 0, 0);
			if (_DinnerReminderTime.Hour == 0) _DinnerReminderTime = new DateTime(1, 1, 1, 20, 0, 0);
			if (_SnackReminderTime.Hour == 0) _SnackReminderTime = new DateTime(1, 1, 1, 22, 0, 0);

		}

		#region Current

		public static void Reset() {current = null;}

		private static UserSettings current;
		public static UserSettings Current
		{
			get
			{
				try
				{
					if (current == null)
					{
						current = LocalStorage<UserSettings>.LoadOrNew("UserSettings");
						current.VerifyDefaults();
						current.DoneLoading = true;
						current.Save();
						current.UpdateAgent();
						current.UpdateReminders();
					}
				}
				catch (Exception ex) { LittleWatson.ReportException(ex); }
				return current;
			}
		}

		public static void ReplaceWith(String Xml)
		{
			try {
				if (Xml == null) return;
				current = (UserSettings) DataContractSerialization.Deserialize(Xml, typeof(UserSettings));
				current.VerifyDefaults();
				current.DoneLoading = true;
				current.Save();
				current.UpdateAgent();
				current.UpdateReminders();

			}catch (Exception ex)
			{
				LittleWatson.ReportException (ex);
			}
		}

		#endregion


		private void Save ()
		{
			if (!DoneLoading)
				return;
			LocalStorage<UserSettings>.Save ("UserSettings", current);
		}
		//
		//        private void Save()
		//        {
		//            if (!DoneLoading) return;
		//
		//            try
		//            {
		//                FoodJournal.Model.Cache.ISOSync.WaitOne();
		//                IsolatedStorageSettings.ApplicationSettings["UserSettings"] = this;
		//                IsolatedStorageSettings.ApplicationSettings.Save();
		//            }
		//            catch (Exception ex) { LittleWatson.ReportException(ex); }
		//            finally { FoodJournal.Model.Cache.ISOSync.Set(); }
		//
		//#if DEBUG
		//            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue("UserSettings", out current))
		//                System.Diagnostics.Debugger.Break();
		//#endif
		//
		//        }

		#region Properties

		private List<Period> selectedMeals;

		[DataMember]
		public List<Period> SelectedMeals{get {
				if (selectedMeals == null || selectedMeals.Count == 0)
					selectedMeals = new List<Period> () {
					Period.Breakfast,
					Period.Lunch,
					Period.Dinner,
					Period.Snack
				};
				return selectedMeals;
			}
			set{
				selectedMeals = new List<Period> ();
				// makse sure we show meals in the right order
				if (value != null)
					foreach (Period p in PeriodList.All)
						if (value.Contains (p))
							selectedMeals.Add (p);				
			}
		}

		public void AddSelectedMeal(Period p)
		{
			if (!SelectedMeals.Contains (p)) {
				SelectedMeals.Add (p);
				SelectedMeals = SelectedMeals; // sort
				Save ();
			}
		}

		public void RemoveSelectedMeal(Period p)
		{
			if (SelectedMeals.Contains (p)) {
				SelectedMeals.Remove (p);
				Save ();
			}
		}

		private List<ReminderData> reminders;

		[DataMember]
		public List<ReminderData> Reminders {get { 
				if (reminders == null || reminders.Count == 0)
					reminders = new List<ReminderData> () {
					new ReminderData (){ Time = new DateTime (1, 1, 1, 10, 0, 0), Checked = false },
					new ReminderData () {Time = new DateTime (1, 1, 1, 22, 0, 0),Checked = true}
				};
				return reminders; } set {
				reminders = value;
				Save ();
				UpdateReminders ();
			}}


		private bool showMealTime;
		[DataMember]
		public bool ShowMealTime{
			get{ return showMealTime; } set{showMealTime=value; Save();}
		}


		[DataMember]
		public string SelectedTotalID;

		public Property SelectedTotal
		{
			get
			{
				if (SelectedTotalID == null) return StandardProperty.none;
				return Property.GetProperty(SelectedTotalID);
			}
			set
			{
				SelectedTotalID = value.ID;
				Save();
			}
		}

		public bool ShowTotal { get { return (SelectedTotal != StandardProperty.none); } set { SelectedTotal = value ? CurrentProperty : StandardProperty.none; } }


		private bool showNotes;
		[DataMember]
		public bool ShowNotes{
			get{ return showNotes; } set{showNotes=value; Save();}
		}

		private bool unitsCalories;
		[DataMember]
		public bool UnitsCalories{
			get{ return unitsCalories; } set{unitsCalories=value; Save();}
		}

		private bool isMetric;
		[DataMember]
		public bool IsMetric{
			get{ return isMetric; } set{isMetric=value; Save();}
		}

		#region Breakfast Reminder

		private bool _BreakfastReminder;

		[DataMember]
		public bool BreakfastReminder
		{
			get { return _BreakfastReminder; }
			set
			{
				_BreakfastReminder = value;
				Save();
				UpdateReminders();
			}
		}

		private DateTime _BreakfastReminderTime;

		[DataMember]
		public DateTime BreakfastReminderTime
		{
			get { return _BreakfastReminderTime; }
			set
			{
				_BreakfastReminderTime = value;
				Save();
				UpdateReminders();
			}
		}

		#endregion

		#region Lunch Reminder

		private bool _LunchReminder;

		[DataMember]
		public bool LunchReminder
		{
			get { return _LunchReminder; }
			set
			{
				_LunchReminder = value;
				Save();
				UpdateReminders();
			}
		}

		private DateTime _LunchReminderTime;

		[DataMember]
		public DateTime LunchReminderTime
		{
			get { return _LunchReminderTime; }
			set
			{
				_LunchReminderTime = value;
				Save();
				UpdateReminders();
			}
		}

		#endregion

		#region Dinner Reminder

		private bool _DinnerReminder;

		[DataMember]
		public bool DinnerReminder
		{
			get { return _DinnerReminder; }
			set
			{
				_DinnerReminder = value;
				Save();
				UpdateReminders();
			}
		}

		private DateTime _DinnerReminderTime;

		[DataMember]
		public DateTime DinnerReminderTime
		{
			get { return _DinnerReminderTime; }
			set
			{
				_DinnerReminderTime = value;
				Save();
				UpdateReminders();
			}
		}

		#endregion

		#region Snack Reminder

		private bool _SnackReminder;

		[DataMember]
		public bool SnackReminder
		{
			get { return _SnackReminder; }
			set
			{
				_SnackReminder = value;
				Save();
				UpdateReminders();
			}
		}

		private DateTime _SnackReminderTime;

		[DataMember]
		public DateTime SnackReminderTime
		{
			get { return _SnackReminderTime; }
			set
			{
				_SnackReminderTime = value;
				Save();
				UpdateReminders();
			}
		}

		#endregion

		#region SnackMorningEnabled

		private bool _SnackMorningEnabled;

		[DataMember]
		public bool SnackMorningEnabled
		{
			get { return _SnackMorningEnabled; }
			set
			{
				_SnackMorningEnabled = value;
				Save();
				UpdateMeals();
			}
		}

		#endregion

		#region SnackEarlyAfternoonEnabled

		private bool _SnackEarlyAfternoonEnabled;

		[DataMember]
		public bool SnackEarlyAfternoonEnabled
		{
			get { return _SnackEarlyAfternoonEnabled; }
			set
			{
				_SnackEarlyAfternoonEnabled = value;
				Save();
				UpdateMeals();
			}
		}

		#endregion

		#region SnackAfternoonEnabled

		private bool _SnackAfternoonEnabled;

		[DataMember]
		public bool SnackAfternoonEnabled
		{
			get { return _SnackAfternoonEnabled; }
			set
			{
				_SnackAfternoonEnabled = value;
				Save();
				UpdateMeals();
			}
		}

		#endregion

		#region SnackEveningEnabled

		private bool _SnackEveningEnabled;

		[DataMember]
		public bool SnackEveningEnabled
		{
			get { return _SnackEveningEnabled; }
			set
			{
				_SnackEveningEnabled = value;
				Save();
				UpdateMeals();
			}
		}

		#endregion

		#region SnackMidnightEnabled

		private bool _SnackMidnightEnabled;

		[DataMember]
		public bool SnackMidnightEnabled
		{
			get { return _SnackMidnightEnabled; }
			set
			{
				_SnackMidnightEnabled = value;
				Save();
				UpdateMeals();
			}
		}

		#endregion

		#region EnableLiveTile

		private bool _EnableLiveTile;

		[DataMember(Order = 0)]
		public bool EnableLiveTile
		{
			get { return _EnableLiveTile; }
			set
			{
				_EnableLiveTile = value;
				Save();
				UpdateAgent();
			}
		}


		#endregion

		#region SuppressAskForReview

		private bool suppressAskForReview;

		[DataMember(Order = 1)]
		public bool SuppressAskForReview
		{
			get { return suppressAskForReview; }
			set
			{
				suppressAskForReview = value;
				Save();
			}
		}

		#endregion

		#region SuppressAskForReviewCount

		private int suppressAskForReviewCount;

		[DataMember]
		public int SuppressAskForReviewCount
		{
			get { if (suppressAskForReview && suppressAskForReviewCount == 0) suppressAskForReviewCount = 1; return suppressAskForReviewCount; }
			set
			{
				suppressAskForReviewCount = value;
				Save();
			}
		}

		#endregion

		#region Email

		private string email;

		[DataMember]
		public string Email
		{
			get { return email; }
			set
			{
				email = value;
				Save();
			}
		}

		#endregion

		#region Recent Email

		private string recent_email;

		[DataMember]
		public string RecentEmail
		{
			get { return recent_email; }
			set
			{
				recent_email = value;
				Save();
			}
		}

		#endregion

		#region Goals

		private PropertyDictionary goals;

		private List<Property> selectedProperties;
		public List<Property> SelectedProperties
		{
			get {
				if (selectedProperties == null) selectedProperties = new List<Property> () {StandardProperty.Calories,StandardProperty.TotalFat,StandardProperty.Carbs,StandardProperty.Protein};
				return selectedProperties;
			} set { selectedProperties = value;}				
		}

		[DataMember]
		public string SelectedPropertiesStorage
		{
			get { string result=""; foreach (var p in SelectedProperties) result += p.ID; return result;}
			set { 
				List<Property> NewSet = new List<Property> ();
				for (int i=0;i<value.Length;i+=2)
					try{NewSet.Add(Property.GetProperty(value.Substring(i,2)));} catch (Exception ex) {LittleWatson.ReportException (ex);}
				SelectedProperties = NewSet;				
			}
		}

		public void AddSelectedProperty(Property p){SelectedProperties.Add (p);Save ();}
		public void RemoveSelectedProperty(Property p){SelectedProperties.Remove (p);Save ();}

		[DataMember(Order = 2, Name = "Goals")]
		public string GoalStorage
		{
			get { return goals == null ? null : goals.ToString(); }
			set { if (value == null) { goals = null; } else { goals = new PropertyDictionary(value); } }
		}

		public Single GetGoal(Property property)
		{
			if (goals != null) return goals[property];
			return Single.NaN;
		}

		public void SetGoal(Property property, Single value)
		{
			if (goals == null) goals = new PropertyDictionary(string.Empty);
			goals[property] = value;
			SessionLog.RecordTraceValue(property.ToString() + " goal", value.ToString());
			Save();
		}

		private string goalDescription;

		[DataMember]
		public string GoalDescription
		{
			get { return goalDescription; }
			set{goalDescription = value;Save();}
		}

		private string goalDate;

		[DataMember]
		public string GoalDate
		{
			get { return goalDate; }
			set{goalDate = value;Save();}
		}


		#endregion

		#region CurrentProperty

		[DataMember]
		public string CurrentPropertyID;

		public Property CurrentProperty
		{
			get
			{
				if (CurrentPropertyID == null)
				{
					if (SelectedProperties == null || SelectedProperties.Count == 0) return StandardProperty.Calories;
					return SelectedProperties[0];
				}
				return Property.GetProperty(CurrentPropertyID);
			}
			set
			{
				CurrentPropertyID = value.ID;
				Save();
			}
		}

		#endregion

		#endregion

		#region UpdateAgent

		public void UpdateAgent()
		{
			#if WINDOWS_PHONE
			if (DoneLoading)
			if (_EnableLiveTile)
			ScheduleAgent();
			else
			FoodJournalAgent.ScheduledAgent.RemoveAgent();
			#endif
		}

		public static void ScheduleAgent()
		{

			#if WINDOWS_PHONE
			// Obtain a reference to the period task, if one exists
			var periodicTask = ScheduledActionService.Find(FoodJournalAgent.ScheduledAgent.periodicTaskName) as PeriodicTask;

			// If the task already exists and background agents are enabled for the
			// application, you must remove the task and then add it again to update 
			// the schedule
			if (periodicTask != null)
			FoodJournalAgent.ScheduledAgent.RemoveAgent();

			periodicTask = new PeriodicTask(FoodJournalAgent.ScheduledAgent.periodicTaskName);

			// The description is required for periodic agents. This is the string that the user
			// will see in the background services Settings page on the device.
			periodicTask.Description = AppResources.AgentDescription;

			// Place the call to Add in a try block in case the user has disabled agents.
			try
			{

			// reset count
			FoodJournalAgent.ScheduledAgent.SetCount(0);

			try
			{
			FoodJournal.Model.Cache.ISOSync.WaitOne();
			IsolatedStorageSettings.ApplicationSettings[FoodJournalAgent.ScheduledAgent.scheduledTime] = DateTime.Now;
			IsolatedStorageSettings.ApplicationSettings.Save();
			}
			catch (Exception ex) { LittleWatson.ReportException(ex); }
			finally { FoodJournal.Model.Cache.ISOSync.Set(); }

			ScheduledActionService.Add(periodicTask);

			// If debugging is enabled, use LaunchForTest to launch the agent in one minute.
			#if(DEBUG)
			ScheduledActionService.LaunchForTest(FoodJournalAgent.ScheduledAgent.periodicTaskName, TimeSpan.FromSeconds(3));
			#endif
		}
		catch (InvalidOperationException exception)
		{
			if (exception.Message.Contains("BNS Error: The action is disabled"))
			{
				//MessageBox.Show(strings.AgentsDisabled);
			}

			if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
			{
				// No user action required. The system prompts the user when the hard limit of periodic tasks has been reached.

			}
		}
		catch //(SchedulerServiceException ex)
		{
			// No user action required. (throw to log in the caller)
			throw;
		}
			#endif
	}

	#endregion

	#region UpdateReminders

	private void UpdateReminder(bool enable, string name, DateTime time, string title, string content, string url)
	{

		if (ScheduledActionService.Find(name) != null)
			ScheduledActionService.Remove(name);

		if (!enable) return;

		var begintime = DateTime.Now.SetTime(time);

		// setting dinner reminder at 10am schedules it for today
		// setting breakfast reminder at 2pm schedules it for tomorrow
		// setting breakfast reminder for 10am at 9 am schedules it for tomorrow

		#if !DEBUG
		if (begintime.AddHours(-2) < DateTime.Now)
		begintime = begintime.AddDays(1);
		#endif

		//begintime = DateTime.Now.AddMinutes(5);

		Reminder reminder = new Reminder(name);
		reminder.Title = title;
		reminder.Content = content;
		reminder.BeginTime = begintime;
		reminder.ExpirationTime = begintime.AddDays(7); // rescheduled every time the app opens, if the app isnt opened for 7 days, give up
		reminder.RecurrenceType = RecurrenceInterval.Daily;
		reminder.NavigationUri = new Uri(url, UriKind.Relative);

		#if !WINDOWS_PHONE && !__IOS__

		ScheduledActionService.Add(reminder);

		#endif

	}

	private void UpdateReminders()
	{

		if (!DoneLoading) return;

		try
		{
			foreach (var item in Reminders)
			{
				var reminderPeriod = item.Time.Period();
				if(reminderPeriod == Period.Breakfast)
				{
					_BreakfastReminder = item.Checked;
					_BreakfastReminderTime = item.Time;
					UpdateReminder(_BreakfastReminder, "BreakfastReminder", _BreakfastReminderTime, AppResources.ReminderTitle, AppResources.ReminderContent, "/Views/Splash.xaml?Period=Breakfast");
				}
				else if (reminderPeriod == Period.Lunch)
				{
					_LunchReminder = item.Checked;
					_LunchReminderTime = item.Time;
					UpdateReminder(_LunchReminder, "LunchReminder", _LunchReminderTime, AppResources.ReminderTitle, AppResources.ReminderContent, "/Views/Splash.xaml?Period=Lunch");
				}
				else if (reminderPeriod == Period.Dinner)
				{
					_DinnerReminder = item.Checked;
					_DinnerReminderTime = item.Time;
					UpdateReminder(_DinnerReminder, "DinnerReminder", _DinnerReminderTime, AppResources.ReminderTitle, AppResources.ReminderContent, "/Views/Splash.xaml?Period=Dinner");
				}
				else if (reminderPeriod == Period.Dinner)
				{
					_SnackReminder = item.Checked;
					_SnackReminderTime = item.Time;
					UpdateReminder(_SnackReminder, "SnackReminder", _SnackReminderTime, AppResources.ReminderTitle, AppResources.ReminderContent, "/Views/Splash.xaml?Period=Snack");
				}
				else
				{
					UpdateReminder(item.Checked, "Reminder", item.Time, AppResources.ReminderTitle, AppResources.ReminderContent, "/Views/Splash.xaml?Period=Snack");
				}
			}

		}
		catch (Exception ex) { LittleWatson.ReportException(ex); }

	}

	#endregion

	#region UpdateMeals

	public DateTime MealsLastUpdated = DateTime.Now;

	private List<Period> selectedPeriods;

	public List<Period> Meals
	{
		get
		{
			return SelectedMeals;
			//if (selectedPeriods == null) UpdateMeals();
			//return selectedPeriods;
		}
	}

	public bool SnacksCombined
	{
		get { return !_SnackMorningEnabled && !_SnackEarlyAfternoonEnabled && !_SnackAfternoonEnabled && !_SnackEveningEnabled && !_SnackMidnightEnabled; }
		set
		{
			_SnackMorningEnabled = !value;
			_SnackEarlyAfternoonEnabled = false;
			_SnackAfternoonEnabled = !value;
			_SnackEveningEnabled = !value;
			_SnackMidnightEnabled = false;
			Save();
			UpdateMeals();
		}
	}

	private void UpdateMeals()
	{
		MealsLastUpdated = DateTime.Now;
		selectedPeriods = new List<Period>();

		selectedPeriods.Add(Period.Breakfast);
		if (SnackMorningEnabled) selectedPeriods.Add(Period.SnackMorning);
		selectedPeriods.Add(Period.Lunch);
		if (SnackEarlyAfternoonEnabled) selectedPeriods.Add(Period.SnackEarlyAfternoon);
		if (SnackAfternoonEnabled) selectedPeriods.Add(Period.SnackAfternoon);
		selectedPeriods.Add(Period.Dinner);
		if (SnackEveningEnabled) selectedPeriods.Add(Period.SnackEvening);
		if (SnackMidnightEnabled) selectedPeriods.Add(Period.SnackMidnight);

		if (SnacksCombined) selectedPeriods.Add(Period.Snack);

	}

	#endregion

}
//// Register the reminder with the system.
//ScheduledActionService.Add(reminder);


}
