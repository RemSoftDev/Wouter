
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using FoodJournal.ViewModels;
using FoodJournal.Values;
using FoodJournal.Model;
using FoodJournal.Android15.Adapters;
using System.Collections.ObjectModel;
using FoodJournal.Logging;
using FoodJournal.Runtime;
using Android.Graphics;
using FoodJournal.Resources;
using System.Threading.Tasks;
using FoodJournal.Extensions;
using Android.Gms.Ads;
using FoodJournal.AppModel;

namespace FoodJournal.Android15
{

    public class ReportFragment : Fragment
    {
        LinearLayout items;

        private LinearLayout advertisement;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var view = inflater.Inflate(Resource.Layout.fragment_report, container, false);

                Activity.FindViewById<TabLayout>(Resource.Id.tabs).Visibility = ViewStates.Gone;
                Activity.FindViewById<FloatingActionButton>(Resource.Id.fab_list).Visibility = ViewStates.Gone;
                advertisement = view.FindViewById<LinearLayout>(Resource.Id.advertisement);
                Task.Run(() =>
                {
                    Task.Delay(50).Wait();
                    Platform.RunSafeOnUIThread("ReportFragment.OnCreateView",() =>
                    {
                        items = view.FindViewById<LinearLayout>(Resource.Id.reportitems);
                        int z = 0;
                        foreach (Property p in UserSettings.Current.SelectedProperties)
                        {
                            var chart = inflater.Inflate(Resource.Layout.item_chart, container, false);
                            var titleTextView = chart.FindViewById<TextView>(Resource.Id.chart_title);

                            var propertyView = chart.FindViewById<WeekGraph>(Resource.Id.chart1);
                            p.Color = AndroidUI.GetPropertyColor(Activity, z, p);
                            titleTextView.SetTextColor(p.Color);
                            titleTextView.Text = p.TextOnly;
                            propertyView.property = p;
                            propertyView.BarClick += (sender, e) =>
                            {
                                var date = e;
                                Intent i = new Intent(Activity, typeof(FoodJournal.Android15.Activities.ViewDayActivity));
                                i.PutExtra("date", date.ToStorageStringDate());
                                i.PutExtra("dateText", (date == DateTime.Now.Date) ? AppResources.Today : date.ToString("dddd"));
                                i.PutExtra("period", (int)Period.Breakfast);
                                StartActivity(i);
                            };

                            items.AddView(chart);

                            z++;
                        } 
								SessionLog.EndPerformance("Navigate");

							});
                });
                

                return view;
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
                return null;
            }

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

                //				var newAd = new AdView(context);
                //				newAd.AdSize = AdSize.SmartBanner;
                //				newAd.AdUnitId = "ca-app-pub-3167302081266616/3848015885";
                //				var rb = new AdRequest.Builder().Build();
                //				newAd.LoadAd(rb);

                //                if (ad != null && ad.Context != context)
                //                {
                //                    if (ad.Parent != null)
                //                        (ad.Parent as LinearLayout).RemoveView(ad);
                //                    ad = null;
                //                }

                //if (ad == null)
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

                //                if (ad.Parent != null)
                //                {
                //
                //                    if (ad.Parent == adbox)
                //                        return;
                //
                //                    (ad.Parent as LinearLayout).RemoveView(ad);
                //                }
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
            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.Report);
            GrabAd();
        }


        private void LoadChart(View view, LayoutInflater inflater, ViewGroup container)
        {
        }

    }
}


