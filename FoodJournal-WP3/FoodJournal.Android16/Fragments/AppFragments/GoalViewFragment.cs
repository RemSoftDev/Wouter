using System;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using FoodJournal.ViewModels;
using FoodJournal.Android15.Adapters;
using FoodJournal.Values;
using FoodJournal.ViewModels.Fragments;
using Android.Widget;
using System.Threading.Tasks;
using Android.Gms.Ads;
using FoodJournal.AppModel;
using Android.Content;
using FoodJournal.Logging;

namespace FoodJournal.Android15.Fragments.AppFragments
{
    public class GoalViewFragment : BaseViewFragment
    {
        TabLayout tabLayout;
        public GoalsVM vm = null;
        LinearLayout contentView=null;
        private LinearLayout advertisement;
        View view;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Title = GetString(Resource.String.Goals);
            view = inflater.Inflate(Resource.Layout.fragment_goals, container, false);

            tabLayout = Activity.FindViewById<TabLayout>(Resource.Id.tabs);
            tabLayout.Visibility = ViewStates.Gone;

            advertisement = view.FindViewById<LinearLayout>(Resource.Id.advertisement);
            FloatingActionButton fab = Activity.FindViewById<FloatingActionButton>(Resource.Id.fab_list);

            fab.Visibility = ViewStates.Gone;

            //Task.Run(() =>
            //{
            //    Task.Delay(700).Wait();
            //    Platform.RunSafeOnUIThread(,() =>
            //{
            //    var contentPanel = view.FindViewById<Android.Widget.LinearLayout>(Resource.Id.contentpanel);
            //    contentPanel.RequestFocus();
            //    var binding = DataContext<GoalsVM>.FromView(contentPanel);
            //    binding.VM = vm;

            //    binding.Add(Resource.Id.editText_description, x => x.Description);
            //    binding.Add(Resource.Id.editDate, x => x.Date);

            //    var editDate = view.FindViewById<Android.Widget.EditText>(Resource.Id.editDate);
            //    editDate.Click += (sender, e) =>
            //    {
            //        DateTime goal;
            //        if (!DateTime.TryParse(vm.Date, out goal)) goal = DateTime.Now.Date;
            //        Android.App.DatePickerDialog dpd = new Android.App.DatePickerDialog(Activity,
            //            (c, d) =>
            //            {
            //                editDate.Text = d.Date.ToShortDateString();
            //            },
            //            goal.Year,
            //            goal.Month,
            //            goal.Day);
            //        dpd.Show();
            //    };

            //    var adapter = new VMListAdapter<GoalLineVM>(Activity, vm.Goals, Resource.Layout.item_goalline, null,
            //        (b, gvm) =>
            //        {
            //            b.Add(Resource.Id.text, x => x.Text);
            //            b.Add(Resource.Id.value, x => x.Value);
            //            b.Add(Resource.Id.image_remove, (a) =>
            //            {
            //                vm.DeleteGoal(a);
            //            });
            //        }
            //    );

            //    contentView = view.FindViewById<Android.Widget.LinearLayout>(Resource.Id.contentView);
            //    for (int i = 0; i < adapter.Count; i++)
            //        contentView.AddView(adapter.GetView(i, null, contentView));

            //    vm.Goals.CollectionChanged += (sender, e) =>
            //    {
            //        contentView.RemoveAllViews();
            //        for (int i = 0; i < adapter.Count; i++)
            //            contentView.AddView(adapter.GetView(i, null, contentView));
            //    };

            //    var text_button = view.FindViewById<Android.Widget.TextView>(Resource.Id.text_button);
            //    text_button.Click += (sender, e) =>
            //    {

            //        List<String> options = vm.NewPropertyOptions;

            //        AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            //        builder.SetTitle(Resource.String.add_target)
            //            .SetItems(options.ToArray(), (s, e2) =>
            //            {

            //                Property result = StandardProperty.none;
            //                String clicked = options[e2.Which];
            //                foreach (var value in Property.All())
            //                    if (value.FullCapitalizedText == clicked)
            //                        result = value;

            //                if (result == StandardProperty.none) return;
            //                vm.AddGoal(result);
            //            });

            //        builder.Create();
            //        builder.Show();
            //    };

            //       });
            //    });

            return view;
        }

#if AMAZON
        public void GrabAd() { }
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

        public override void OnResume()
        {
            base.OnResume();
           // contentView.RemoveAllViews();
            vm = new GoalsVM();
            
            Task.Run(() =>
            {
                Task.Delay(70).Wait();
                Platform.RunSafeOnUIThread("GoalViewFragment.OnResume",() =>
            {
                var contentPanel = view.FindViewById<Android.Widget.LinearLayout>(Resource.Id.contentpanel);
                contentPanel.RequestFocus();
                var binding = DataContext<GoalsVM>.FromView(contentPanel);
                binding.VM = vm;

                binding.Add(Resource.Id.editText_description, x => x.Description);
                binding.Add(Resource.Id.editDate, x => x.Date);

                var editDate = view.FindViewById<Android.Widget.EditText>(Resource.Id.editDate);
                editDate.Click += (sender, e) =>
                {
                    DateTime goal;
                    if (!DateTime.TryParse(vm.Date, out goal)) goal = DateTime.Now.Date;
                    Android.App.DatePickerDialog dpd = new Android.App.DatePickerDialog(Activity,
                        (c, d) =>
                        {
                            editDate.Text = d.Date.ToShortDateString();
                            vm.Date = d.Date.ToShortDateString();
                        },
                        goal.Year,
                        goal.Month,
                        goal.Day);
                    dpd.Show();
                };

                var adapter = new VMListAdapter<GoalLineVM>(Activity, vm.Goals, Resource.Layout.item_goalline, null,
                    (b, gvm) =>
                    {
                        b.Add(Resource.Id.text, x => x.Text);
                        b.Add(Resource.Id.value, x => x.Value);
                        b.Add(Resource.Id.image_remove, (a) =>
                        {
                            vm.DeleteGoal(a);
                        });
                    }
                );

                contentView = view.FindViewById<Android.Widget.LinearLayout>(Resource.Id.contentView);
                contentView.RemoveAllViews();
                for (int i = 0; i < adapter.Count; i++)
                    contentView.AddView(adapter.GetView(i, null, contentView));

                vm.Goals.CollectionChanged += (sender, e) =>
                {
                    contentView.RemoveAllViews();
                    for (int i = 0; i < adapter.Count; i++)
                        contentView.AddView(adapter.GetView(i, null, contentView));
                };

                var text_button = view.FindViewById<Android.Widget.TextView>(Resource.Id.text_button);
                text_button.Click += (sender, e) =>
                {

                    List<String> options = vm.NewPropertyOptions;

                    AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                    builder.SetTitle(Resource.String.add_target)
                        .SetItems(options.ToArray(), (s, e2) =>
                        {

                            Property result = StandardProperty.none;
                            String clicked = options[e2.Which];
                            foreach (var value in Property.All())
                                if (value.FullCapitalizedText == clicked)
                                    result = value;

                            if (result == StandardProperty.none) return;
                            vm.AddGoal(result);
                        });

                    builder.Create();
                    builder.Show();
                };

							SessionLog.EndPerformance("Navigate");

            });
            });
            GrabAd();
        }


    }
}