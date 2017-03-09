
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using FoodJournal.ViewModels;
using FoodJournal.Resources;
using FoodJournal.Runtime;
using FoodJournal.Logging;
using FoodJournal.AppModel;

namespace FoodJournal.Android15.Activities
{
	[Activity (Theme = "@style/AppTheme", Label="@string/PageTitleSettings")]
	public class Settings : StandardActivity
	{

		private Button pinbutton;

		public Settings(): base(false) {}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.Settings);
			SetSupportActionBar (FindViewById<Android.Support.V7.Widget.Toolbar> (Resource.Id.toolbar));
				
			this.SupportActionBar.Title = AppResources.PageTitleSettings;//.ToUpper ();

			#if DEBUG
			screenshotview=FindViewById (Resource.Id.contentpanel);
			#endif

			SettingsVM vm = new SettingsVM ();
			NotifySinker sink = new NotifySinker(vm);

			Binding.Make(this, Resource.Id.chk_breakfast, vm, x => x.BreakfastReminder);
			Binding.Make(this, Resource.Id.chk_lunch, vm, x => x.LunchReminder);
			Binding.Make(this, Resource.Id.chk_dinner, vm, x => x.DinnerReminder);
			Binding.Make(this, Resource.Id.chk_endofday, vm, x => x.SnackReminder);

			Binding.Make(this, Resource.Id.edt_breakfast, vm, x => x.BreakfastReminderTime);
			Binding.Make(this, Resource.Id.edt_lunch, vm, x => x.LunchReminderTime);
			Binding.Make(this, Resource.Id.edt_dinner, vm, x => x.DinnerReminderTime);
			Binding.Make(this, Resource.Id.edt_endofday, vm, x => x.SnackReminderTime);

			pinbutton = this.FindViewById<Button> (Resource.Id.PinToStart);
			pinbutton.Click += PinToStart;
			//Binding.Make(this, Resource.Id.PinToStart, vm, x => {PinToStart();});

			this.FindViewById<TextView> (Resource.Id.caloriesTitle).Text=vm.CaloriesTitle;
			Binding.Make (this, Resource.Id.caloriesGoal, vm, x => x.CalorieGoal);
			this.FindViewById<TextView> (Resource.Id.totalFatTitle).Text=vm.TotalFatTitle;
			Binding.Make (this, Resource.Id.totalFatGoal, vm, x => x.TotalFatGoal);
			this.FindViewById<TextView> (Resource.Id.carbsTitle).Text=vm.CarbsTitle;
			Binding.Make (this, Resource.Id.carbsGoal, vm, x => x.CarbsGoal);
			this.FindViewById<TextView> (Resource.Id.proteinTitle).Text=vm.ProteinTitle;
			Binding.Make (this, Resource.Id.proteinGoal, vm, x => x.ProteinGoal);

			Binding.Make(this, Resource.Id.chk_snacksCombined, vm, x => x.SnacksCombined,sink);
			Binding.Make(this, Resource.Id.chk_snacksCombined, vm, x => x.SnacksCombinedVisibility,sink);
			Binding.Make(this, Resource.Id.chk_snackMorningEnabled, vm, x => x.SnackMorningEnabled,sink);
			Binding.Make(this, Resource.Id.chk_snackMorningEnabled, vm, x => x.SnackMorningVisibility,sink);
			Binding.Make(this, Resource.Id.chk_snackEarlyAfternoonEnabled, vm, x => x.SnackEarlyAfternoonEnabled,sink);
			Binding.Make(this, Resource.Id.chk_snackEarlyAfternoonEnabled, vm, x => x.SnackEarlyAfternoonVisibility,sink);
			Binding.Make(this, Resource.Id.chk_snackAfternoonEnabled, vm, x => x.SnackAfternoonEnabled,sink);
			Binding.Make(this, Resource.Id.chk_snackAfternoonEnabled, vm, x => x.SnackAfternoonVisibility,sink);
			Binding.Make(this, Resource.Id.chk_snackEveningEnabled, vm, x => x.SnackEveningEnabled,sink);
			Binding.Make(this, Resource.Id.chk_snackEveningEnabled, vm, x => x.SnackEveningVisibility,sink);
			Binding.Make(this, Resource.Id.chk_snackMidnightEnabled, vm, x => x.SnackMidnightEnabled,sink);
			Binding.Make(this, Resource.Id.chk_snackMidnightEnabled, vm, x => x.SnackMidnightVisibility,sink);

		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
		}

		protected override List<MenuLink> GetMenuItems ()
		{
			List<MenuLink> list = new List<MenuLink> ();
			list.Add(new MenuLink(AppResources.SendFeedback, Navigate.ToFeedback));
			return list;
		}

		protected void PinToStart(object sender, EventArgs args) {

			try
			{
				SessionLog.RecordMilestone("Pin To Start", AppStats.Current.SessionId.ToString());

				var shortcutIntent = new Intent(this, typeof (Splash));
				shortcutIntent.SetAction(Intent.ActionMain);

				var iconResource = Intent.ShortcutIconResource.FromContext(
					this, Resource.Drawable.ic_launcher);

				var intent = new Intent();
				intent.PutExtra(Intent.ExtraShortcutIntent, shortcutIntent);
				intent.PutExtra(Intent.ExtraShortcutName, AppResources.AppTitle);
				intent.PutExtra(Intent.ExtraShortcutIconResource, iconResource);
				intent.PutExtra("duplicate", false);
				intent.SetAction("com.android.launcher.action.INSTALL_SHORTCUT");
				SendBroadcast(intent);

				(sender as Button).Visibility = ViewStates.Gone;

			}
			catch (Exception ex) { LittleWatson.ReportException(ex); }

		}

//		private void RemoveShortcut()
//		{
//			var shortcutIntent = new Intent(this, typeof (MyAwesomeActivity));
//			shortcutIntent.SetAction(Intent.ActionMain);
//
//			var intent = new Intent();
//			intent.PutExtra(Intent.ExtraShortcutIntent, shortcutIntent);
//			intent.PutExtra(Intent.ExtraShortcutName, "My Awesome App!");
//			intent.SetAction("com.android.launcher.action.UNINSTALL_SHORTCUT");
//			SendBroadcast(intent);
//		}
	}
}

