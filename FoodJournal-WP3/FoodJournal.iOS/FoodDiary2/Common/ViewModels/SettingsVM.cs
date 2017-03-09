using FoodJournal.Resources;
using FoodDiary2;
using Mono.CSharp;
using FoodJournal.AppModel.UI;
using FoodJournal.Values;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Logging;
using FoodJournal.Parsing;
using FoodJournal.AppModel;


#if WINDOWS_PHONE
using Microsoft.Phone.Shell;
using System.Data.Linq;
#endif
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;


namespace FoodJournal.ViewModels
{

	public class MealSettingsVM : VMBase
	{

		private SettingsVM parent;
		private Period period;

		public MealSettingsVM(SettingsVM parent,Period period) {this.parent = parent;this.period = period;}

		public Visibility CheckboxVisibility { get { return parent.MealsExpanded ? Visibility.Visible : Visibility.Collapsed; } }
		public bool CheckboxEnabled { get { return UserSettings.Current.SelectedMeals.Count > 1 || !MealSelected; } }

		public string Text { get { return Strings.FromEnum (period); } set{ }}

		public bool MealSelected {get{
				return UserSettings.Current.SelectedMeals.Contains (period); 
			} 
			set {
				if (value) {
					if (!UserSettings.Current.SelectedMeals.Contains (period)) {
						UserSettings.Current.AddSelectedMeal(period);
						if (UserSettings.Current.SelectedMeals.Count == 2)
							parent.RefreshMealCheckboxEnabled ();
					}
				} else if (UserSettings.Current.SelectedMeals.Contains (period)) {
					UserSettings.Current.RemoveSelectedMeal (period);
					if (UserSettings.Current.SelectedMeals.Count == 1)
						parent.RefreshMealCheckboxEnabled ();
				}
			}
		}

		public void NotifyMealCheckboxEnabled () {
			this.NotifyPropertyChanged ("CheckboxEnabled");
		}

	}

	public class ReminderVM : FoodJournal.AppModel.VMBase 
	{

		private SettingsVM parent;
		public ReminderData Reminder;

		public ReminderVM(SettingsVM parent, ReminderData reminder) {this.parent = parent;this.Reminder = reminder;}

		public Visibility DeleteVisibility { get { return parent.Reminders.Count > 1 ? Visibility.Visible : Visibility.Collapsed;} }

		public bool Checked { get { return Reminder.Checked; } set { Reminder.Checked = value; parent.SaveReminders (); } }
		public string Time { get { return Reminder.Time.ToShortTimeString(); } set { try { Reminder.Time = DateTime.Parse(value); } catch {
				} NotifyPropertyChanged("Time"); parent.SaveReminders (); } }

		public void Delete() { parent.RemoveReminder(this);}

		public void NotifyDeleteVisibility() {this.NotifyPropertyChanged ("DeleteVisibility");}
	}

	public class SettingsVM : VMBase
	{

		private UserSettings settings = UserSettings.Current;

		public SettingsVM() {

			Meals = new ObservableCollection<MealSettingsVM> ();
			foreach (var period in settings.SelectedMeals)
				Meals.Add (new MealSettingsVM (this,period));		

			Reminders = new ObservableCollection<ReminderVM> ();
			foreach (var reminder in settings.Reminders)
				Reminders.Add (new ReminderVM (this,reminder));		

		}

		public bool ShowMealTime { get { return settings.ShowMealTime; } set { settings.ShowMealTime = value; } }
		public String SelectedTotal { get { return ((FoodJournal.Values.Property)settings.SelectedTotal).FullCapitalizedText; } set { 
				FoodJournal.Values.Property newvalue = StandardProperty.none; 
				foreach (var option in UserSettings.Current.SelectedProperties)
					if (option.FullCapitalizedText == value)
						newvalue = option;
				settings.SelectedTotal = newvalue;
			} 
		}
		public List<String> TotalOptions {get {List<String> result = new List<string> ();//{ AppResources.None };
				foreach (var option in UserSettings.Current.SelectedProperties)
					result.Add (option.FullCapitalizedText);
				return result;}}
		public bool ShowTotal { get { return settings.ShowTotal; } set { settings.ShowTotal = value; NotifyPropertyChanged ("TotalOptionsVisibility"); } }
		public Visibility TotalOptionsVisibility { get { return ShowTotal ? Visibility.Visible : Visibility.Collapsed; } }

		public bool ShowNotes { get { return settings.ShowNotes; } set { settings.ShowNotes = value; } }

		public bool UnitsIsCalories { get { return settings.UnitsCalories; } set { settings.UnitsCalories = value; NotifyPropertyChanged ("UnitsIsKJ"); } }
		public bool UnitsIsKJ { get { return !settings.UnitsCalories; } set { settings.UnitsCalories = ! value; NotifyPropertyChanged ("UnitsIsCalories"); } }

		public bool IsMetric { get { return settings.IsMetric; } set { settings.IsMetric = value; NotifyPropertyChanged ("IsNotMetric"); } }
		public bool IsNotMetric { get { return !settings.IsMetric; } set { settings.IsMetric = ! value; NotifyPropertyChanged ("IsMetric"); } }

		public Visibility MealsLockedVisibility { get { return AppStats.Current.PremiumItemsLocked ? Visibility.Visible : Visibility.Collapsed; } }
		public ObservableCollection<MealSettingsVM> Meals { get; set; }
		public void RefreshMealCheckboxEnabled() {
			foreach (MealSettingsVM meal in Meals)
				meal.NotifyMealCheckboxEnabled ();
		}
		private bool mealsExpanded = false;
		public bool MealsExpanded{ get { return mealsExpanded; } set { 
				mealsExpanded = true;
				Meals.Clear ();
				foreach (Period period in PeriodList.All)
					Meals.Add (new MealSettingsVM (this, period));
				NotifyPropertyChanged("Meals");
			}
		}

		public ObservableCollection<ReminderVM> Reminders { get; set; }
		public void SaveReminders ()
		{
			List<ReminderData> settings = new List<ReminderData>();
			foreach (ReminderVM vm in Reminders)
				settings.Add (vm.Reminder);
			UserSettings.Current.Reminders = settings;
		}
		public void AddReminder(DateTime time)
		{
			Reminders.Add(new ReminderVM(this,new ReminderData(){Time = time, Checked=true}));
			SaveReminders ();
			foreach (ReminderVM other in Reminders)
				other.NotifyDeleteVisibility ();
		}
		public void RemoveReminder(ReminderVM vm) {
			Reminders.Remove (vm);
			SaveReminders ();
			foreach (ReminderVM other in Reminders)
				other.NotifyDeleteVisibility ();
		}

	}

	public class SettingsVMOld : INotifyPropertyChanged
	{

		public SettingsVMOld() { }

		public Visibility PinToStartVisibility { get { return IsPinned ? Visibility.Collapsed : Visibility.Visible; } }

		public bool IsPinned { get { 
				#if WINDOWS_PHONE
				return ShellTile.ActiveTiles.Count() > 1;
				#else
				return true;
				#endif
			} }

		public bool BreakfastReminder { get { return UserSettings.Current.BreakfastReminder; } set { UserSettings.Current.BreakfastReminder = value; } }
		public bool LunchReminder { get { return UserSettings.Current.LunchReminder; } set { UserSettings.Current.LunchReminder = value; } }
		public bool DinnerReminder { get { return UserSettings.Current.DinnerReminder; } set { UserSettings.Current.DinnerReminder = value; } }
		public bool SnackReminder { get { return UserSettings.Current.SnackReminder; } set { UserSettings.Current.SnackReminder = value; } }

		public DateTime BreakfastReminderTime { get { return UserSettings.Current.BreakfastReminderTime; } set { UserSettings.Current.BreakfastReminderTime = value; } }
		public DateTime LunchReminderTime { get { return UserSettings.Current.LunchReminderTime; } set { UserSettings.Current.LunchReminderTime = value; } }
		public DateTime DinnerReminderTime { get { return UserSettings.Current.DinnerReminderTime; } set { UserSettings.Current.DinnerReminderTime = value; } }
		public DateTime SnackReminderTime { get { return UserSettings.Current.SnackReminderTime; } set { UserSettings.Current.SnackReminderTime = value; } }

		public bool SnacksCombined { get { return UserSettings.Current.SnacksCombined; } set { UserSettings.Current.SnacksCombined = value; RefreshMeals(); } }
		public bool SnackMorningEnabled { get { return UserSettings.Current.SnackMorningEnabled; } set { UserSettings.Current.SnackMorningEnabled = value; RefreshMeals(); } }
		public bool SnackEarlyAfternoonEnabled { get { return UserSettings.Current.SnackEarlyAfternoonEnabled; } set { UserSettings.Current.SnackEarlyAfternoonEnabled = value; RefreshMeals(); } }
		public bool SnackAfternoonEnabled { get { return UserSettings.Current.SnackAfternoonEnabled; } set { UserSettings.Current.SnackAfternoonEnabled = value; RefreshMeals(); } }
		public bool SnackEveningEnabled { get { return UserSettings.Current.SnackEveningEnabled; } set { UserSettings.Current.SnackEveningEnabled = value; RefreshMeals(); } }
		public bool SnackMidnightEnabled { get { return UserSettings.Current.SnackMidnightEnabled; } set { UserSettings.Current.SnackMidnightEnabled = value; RefreshMeals(); } }

		public Visibility MultiMealVisibility { get { return AppStats.Current.MultiMealHidden ? Visibility.Collapsed : Visibility.Visible; } }

		private bool MealsAllVisible = false;
		public Visibility SnacksCombinedVisibility { get { return MealsAllVisible || UserSettings.Current.SnacksCombined ? Visibility.Visible : Visibility.Collapsed; } }
		public Visibility SnackMorningVisibility { get { return MealsAllVisible || UserSettings.Current.SnackMorningEnabled ? Visibility.Visible : Visibility.Collapsed; } }
		public Visibility SnackEarlyAfternoonVisibility { get { return MealsAllVisible || UserSettings.Current.SnackEarlyAfternoonEnabled ? Visibility.Visible : Visibility.Collapsed; } }
		public Visibility SnackAfternoonVisibility { get { return MealsAllVisible || UserSettings.Current.SnackAfternoonEnabled ? Visibility.Visible : Visibility.Collapsed; } }
		public Visibility SnackEveningVisibility { get { return MealsAllVisible || UserSettings.Current.SnackEveningEnabled ? Visibility.Visible : Visibility.Collapsed; } }
		public Visibility SnackMidnightVisibility { get { return MealsAllVisible || UserSettings.Current.SnackMidnightEnabled ? Visibility.Visible : Visibility.Collapsed; } }

		//public List<string> SelectableMeals { get { return new List<String>() { AppResources.Meals_4perday, AppResources.Meals_6perday }; } }
		//public string SelectedMeals { get { return AppResources.Meals_4perday; } set { } }

		public string CaloriesTitle { get { return ((FoodJournal.Values.Property)StandardProperty.Calories).FullText; } }
		public string TotalFatTitle { get { return ((FoodJournal.Values.Property)StandardProperty.TotalFat).FullText; } }
		public string CarbsTitle { get { return ((FoodJournal.Values.Property)StandardProperty.Carbs).FullText; } }
		public string ProteinTitle { get { return ((FoodJournal.Values.Property)StandardProperty.Protein).FullText; } }

		public bool EnableLiveTile
		{
			get
			{
				return UserSettings.Current.EnableLiveTile;
			}
			set
			{
				if (UserSettings.Current.EnableLiveTile != value)
				{
					SessionLog.RecordTraceValue("Enable Live Tile", value.ToString());
					try
					{
						UserSettings.Current.EnableLiveTile = value;
					}
					catch (Exception ex) { LittleWatson.ReportException(ex); }
					NotifyPropertyChanged("EnableLiveTile");
				}
			}
		}

		private static string GoalString(Single value)
		{
			if (Single.IsNaN(value) || value == 0) return "";//AppResources.NoGoalSet;
			return value.ToUIString();
		}

		public string CalorieGoal
		{
			get { return GoalString(UserSettings.Current.GetGoal(StandardProperty.Calories)); }
			set { UserSettings.Current.SetGoal(StandardProperty.Calories, Floats.ParseUI(value, Single.NaN)); }
		}

		public string TotalFatGoal
		{
			get { return GoalString(UserSettings.Current.GetGoal(StandardProperty.TotalFat)); }
			set { UserSettings.Current.SetGoal(StandardProperty.TotalFat, Floats.ParseUI(value, Single.NaN)); }
		}

		public string ProteinGoal
		{
			get { return GoalString(UserSettings.Current.GetGoal(StandardProperty.Protein)); }
			set { UserSettings.Current.SetGoal(StandardProperty.Protein, Floats.ParseUI(value, Single.NaN)); }
		}

		public string CarbsGoal
		{
			get { return GoalString(UserSettings.Current.GetGoal(StandardProperty.Carbs)); }
			set { UserSettings.Current.SetGoal(StandardProperty.Carbs, Floats.ParseUI(value, Single.NaN)); }
		}

		public void RefreshMeals()
		{
			NotifyPropertyChanged("SnacksCombined");
			NotifyPropertyChanged("SnackMorningEnabled");
			NotifyPropertyChanged("SnackEarlyAfternoonEnabled");
			NotifyPropertyChanged("SnackAfternoonEnabled");
			NotifyPropertyChanged("SnackEveningEnabled");
			NotifyPropertyChanged("SnackMidnightEnabled");
			MealsAllVisible = true;
			NotifyPropertyChanged("SnacksCombinedVisibility");
			NotifyPropertyChanged("SnackMorningVisibility");
			NotifyPropertyChanged("SnackEarlyAfternoonVisibility");
			NotifyPropertyChanged("SnackAfternoonVisibility");
			NotifyPropertyChanged("SnackEveningVisibility");
			NotifyPropertyChanged("SnackMidnightVisibility");
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (null != handler)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

	}
}
