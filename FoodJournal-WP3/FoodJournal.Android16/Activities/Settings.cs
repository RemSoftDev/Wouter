using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoodJournal.ViewModels;
using FoodJournal.Resources;
using FoodJournal.Runtime;
using FoodJournal.Logging;
using FoodJournal.AppModel;
using Android.Support.V7.App;
using FoodJournal.Android15.Adapters;
using FoodJournal.Extensions;
using Android.Widget;
using FoodJournal.ViewModels.Fragments;
using Android.App;
using FoodJournal.Values;
#if !AMAZON
using Android.Gms.Ads;
#endif
using System.Threading.Tasks;
using Android.Content;

namespace FoodJournal.Android15.Activities
{
    [Android.App.Activity(Theme = "@style/AppTheme", Label = "@string/PageTitleSettings")]
    public class Settings : AppCompatActivity
    {

        private SettingsVM vm = new SettingsVM();
		public GoalsVM gvm = new GoalsVM();
        private LinearLayout advertisement;
        protected override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_settings);
            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar));
            this.SupportActionBar.Title = AppResources.PageTitleSettings;
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            advertisement = FindViewById<LinearLayout>(Resource.Id.advertisement);
            GrabAd();
            SettingsVM vm = new SettingsVM();
            var contentPanel = FindViewById<Android.Widget.LinearLayout>(Resource.Id.contentpanel);
            var binding = DataContext<SettingsVM>.FromView(contentPanel);
            binding.VM = vm;
            var chkBoxShowTotal = FindViewById<Android.Widget.CheckBox>(Resource.Id.checkbox_showtotal);
            chkBoxShowTotal.Enabled = false;

                if (UserSettings.Current.SelectedProperties.Any())
                {
                    chkBoxShowTotal.Enabled = true;
                }

            binding.Add(Resource.Id.checkbox_showtotal, x => x.ShowTotal);
            binding.Add(Resource.Id.spinner, x => x.TotalOptionsVisibility);
            var spinnerAdapter = new Android.Widget.ArrayAdapter<String>(this, Resource.Layout.item_settings_property, Resource.Id.text, vm.TotalOptions);
            var spinner = FindViewById<Android.Widget.Spinner>(Resource.Id.spinner);
            spinner.Adapter = spinnerAdapter;
            spinner.SetSelection(spinnerAdapter.GetPosition(vm.SelectedTotal));
            spinner.ItemSelected += (sender, e) => { vm.SelectedTotal = spinner.SelectedItem.ToString(); };
            binding.Add(Resource.Id.checkbox_add_time, x => x.ShowMealTime);

            var mealAdapter = new VMListAdapter<MealSettingsVM>(this, vm.Meals, Resource.Layout.item_settings_meal, null, (b, cvm) =>
            {
                b.Add(Resource.Id.text, x => x.Text);                
            });
			
            var mealspanel = FindViewById<Android.Widget.LinearLayout>(Resource.Id.layout_meals);
            for (int i = 0; i < mealAdapter.Count; i++)
                mealspanel.AddView(mealAdapter.GetView(i, null, mealspanel));

			var nutitionAdapter = new VMListAdapter<GoalLineVM>(this, gvm.Goals, Resource.Layout.item_settings_nutrition, null, (b, cvm) =>
				{
					b.Add(Resource.Id.text, x => x.Text);
					b.Add(Resource.Id.image_remove, (a) => { gvm.DeleteGoal(a); });
				});
			var nutitionspanel = FindViewById<Android.Widget.LinearLayout>(Resource.Id.layout_nutirition);
			for (int i = 0; i < nutitionAdapter.Count; i++)
				nutitionspanel.AddView(nutitionAdapter.GetView(i, null, nutitionspanel));

			gvm.Goals.CollectionChanged += (sender, e) =>
			{
				nutitionspanel.RemoveAllViews();
				for (int i = 0; i < nutitionAdapter.Count; i++)
					nutitionspanel.AddView(nutitionAdapter.GetView(i, null, nutitionspanel));
			};

            var buttonLine1 = FindViewById<Android.Widget.LinearLayout>(Resource.Id.nutrition_button);
            var button2 = buttonLine1.FindViewById<Android.Widget.TextView>(Resource.Id.text_button1);
            var buttonLine = FindViewById<Android.Widget.LinearLayout>(Resource.Id.meal_button);
            var button = buttonLine.FindViewById<Android.Widget.TextView>(Resource.Id.text_button);

			button2.Click += (sender, e) =>
			{
				List<String> options = gvm.NewPropertyOptions;
				var builder = new Android.Support.V7.App.AlertDialog.Builder(this);
				builder.SetTitle(Resource.String.add_target)
					.SetItems(options.ToArray(), (s, e2) =>
						{
							Property result = StandardProperty.none;
							String clicked = options[e2.Which];
							foreach (var value in Property.All())
								if (value.FullCapitalizedText == clicked)
									result = value;
							if (result == StandardProperty.none) return;
							gvm.AddGoal(result);
						});
				builder.Create();
				builder.Show();
			};

            button.Click += (sender, e) =>
            {

                vm.MealsExpanded = true;
                mealspanel.RemoveAllViews();
                buttonLine.Visibility = Android.Views.ViewStates.Gone;

                var meala = new VMListAdapter<MealSettingsVM>(this, vm.Meals, Resource.Layout.item_settings_meal_edit, null, (b, cvm) =>
                {
                    b.Add(Resource.Id.checkbox, x => x.MealSelected);
                    b.Add(Resource.Id.checkbox, x => x.CheckboxVisibility);
                    b.Add(Resource.Id.checkbox, x => x.CheckboxEnabled, "Enabled");
                    b.Add(Resource.Id.edit_meal_name, x => x.Text);
                });

                for (int i = 0; i < meala.Count; i++)
                    mealspanel.AddView(meala.GetView(i, null, mealspanel));
            };

            contentPanel.AddView(Android.Views.View.Inflate(this, Resource.Layout.item_settings_reminder_top, null));

            var reminderAdapter = new VMListAdapter<ReminderVM>(this, vm.Reminders, Resource.Layout.item_settings_reminder, null, (b, cvm) =>
            {
                b.Add(Resource.Id.checkbox, x => x.Checked);
                b.Add(Resource.Id.settings_set_time, x => x.Time);
                b.Add(Resource.Id.settings_set_time, (a) =>
                {
                    DateTime current;
                    if (!DateTime.TryParse(a.Time, out current)) current = DateTime.Now;
                    var tpd = new Android.App.TimePickerDialog(this,
                        (s, e) => { a.Time = DateTime.Now.SetTime(e.HourOfDay, e.Minute, 0).ToShortTimeString(); }
                        , current.Hour, current.Minute, true);
                    tpd.Show();
                });
                b.Add(Resource.Id.image_remove, (a) => { a.Delete(); });
                b.Add(Resource.Id.image_remove, x => x.DeleteVisibility);
            });

            var reminderspanel = FindViewById<Android.Widget.LinearLayout>(Resource.Id.layout_reminders);
            for (int i = 0; i < reminderAdapter.Count; i++)
                reminderspanel.AddView(reminderAdapter.GetView(i, null, reminderspanel));

            var buttonLine2 = Android.Views.View.Inflate(this, Resource.Layout.item_settings_add_reminder, null);
            button = buttonLine2.FindViewById<Android.Widget.TextView>(Resource.Id.text_button);
            contentPanel.AddView(buttonLine2);
            
            button.Click += (sender, e) =>
            {
                var tpd = new Android.App.TimePickerDialog(this,
                    (s, e2) => { vm.AddReminder(DateTime.Now.SetTime(e2.HourOfDay, e2.Minute, 0)); }
                    , DateTime.Now.Hour, 0, true);
                tpd.Show();
            };

            vm.Reminders.CollectionChanged += (sender, e) =>
            {
                reminderspanel.RemoveAllViews();
                for (int i = 0; i < reminderAdapter.Count; i++)
                    reminderspanel.AddView(reminderAdapter.GetView(i, null, reminderspanel));
            };

#if DEBUG
            AndroidDebug.SetViewBorders(contentPanel);
#endif
#if SCREENSHOT
            Navigate.screenshotScreen = this;
#endif
        }

        public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                   
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

#if AMAZON
        public void GrabAd(){}
#else

        private static AdView ad;
        public async void GrabAd()
        {
            ActivateAd(advertisement);
        }

        public async Task ActivateAd(LinearLayout adbox)
        {
            try
            {
                if (!AppStats.Current.AdShows) return;
                Context context = adbox.Context;
                {
                    ad = new AdView(context);
                    ad.AdSize = AdSize.SmartBanner;
#if V16
                    ad.AdUnitId = "ca-app-pub-3167302081266616/8994125881";
#else
                    ad.AdUnitId = "ca-app-pub-3167302081266616/3848015885";
#endif
                    var requestbuilder = new AdRequest.Builder();
                    ad.LoadAd(requestbuilder.Build());
                }
                adbox.AddView(ad);
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
        }
#endif
    }
}