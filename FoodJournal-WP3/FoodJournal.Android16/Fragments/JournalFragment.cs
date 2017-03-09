using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Support.V4.App;
using FoodJournal.ViewModels;
using FoodJournal.Values;
using FoodJournal.Android15.Adapters;
using System.Collections.ObjectModel;
using FoodJournal.Logging;
using Android.Support.V7.App;
using System.Text.RegularExpressions;
using Android.Graphics;
using System.Threading.Tasks;
using FoodJournal.Extensions;
using Android.Support.Design.Widget;
using System.Globalization;
using Android.Gms.Ads;
using FoodJournal.AppModel;
namespace FoodJournal.Android15
{
    public class ScrollListener : RecyclerView.OnScrollListener
    {
        public JournalCardAdapter JournalCardAdapter;
        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);
            JournalCardAdapter.ScrollIfNeeded();
        }
    }

    public class JournalCardAdapter : RecyclerView.Adapter
    {

        private enum ScrollNeed { none, up, down }

        private const int PageSize = 7;

        private Dictionary<DateTime, JournalDayVM> vms = new Dictionary<DateTime, JournalDayVM>();

        protected Context context = null;
        private JournalVM jvm;
		public int CurrentMin = 0;
        public int CurrentMax = PageSize + 3;

        private ScrollNeed scrollNeed = ScrollNeed.none;
        public EventHandler<JournalDayVM> OnCardItemClick { get; set; }
        public EventHandler<JournalDayVM> OnSendReportClick { get; set; }
       
        public JournalCardAdapter(JournalVM jvm, Context Context)
            : base()
        {
            this.jvm = jvm;
            this.context = Context;
        }

        private DateTime day(int position) { return DateTime.Now.AddDays(-position - CurrentMin); }
        public JournalDayVM this[int position]
        {
            get
            {
                DateTime date = day(position); JournalDayVM result;
                if (!vms.TryGetValue(date, out result))
                {
                    result = new JournalDayVM(jvm, date);
                    vms[date] = result;
                }
                return result;
            }
        }

        public override long GetItemId(int position) { return position; }
        public override int ItemCount { get { return CurrentMax - CurrentMin; } }
        public override int GetItemViewType(int position) { return 1; }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            VMViewHolder<JournalDayVM> result;
            try
            {
                result = new VMViewHolder<JournalDayVM>(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_journal_card, parent, false));
            }
            catch (Exception e)
            {
                throw;
            }
           
            return result;
        }
       
        public void ScrollIfNeeded()
        {

			if (scrollNeed == ScrollNeed.up && false)
            {				
                CurrentMin -= PageSize;
                this.NotifyItemRangeInserted(0, PageSize);
            }

            if (scrollNeed == ScrollNeed.down)
            {
                CurrentMax += PageSize;
                this.NotifyItemRangeInserted(CurrentMax - PageSize, PageSize);
            }

            scrollNeed = ScrollNeed.none;

        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {

            JournalDayVM vm = this[position];
            vm.Sync(vm.Date);

            if (scrollNeed == ScrollNeed.none)
            {
				if (position == 0 && false) //12/29/15 - disabling future scrolling untill we get to FJA-95 
                    scrollNeed = ScrollNeed.up;

                if (position > CurrentMax - CurrentMin - PageSize)
                    scrollNeed = ScrollNeed.down;
            }

            View view = holder.ItemView;
            ((VMViewHolder<JournalDayVM>)holder).ViewModel = vm;

            var binding = DataContext<JournalDayVM>.FromView(view);
            binding.VM = vm;

            if (binding.Bindings.Count == 0)
            {
                binding.Add(Resource.Id.text_journal_day, x => x.DateText);
                binding.Add(Resource.Id.text_journal_total, x => x.SelectedPropertyTotal);
                binding.Add(Resource.Id.text_journal_delta, x => x.SelectedPropertyDelta);
                binding.Add(Resource.Id.text_journal_date, x => x.DateShort);
            }

            LinearLayout content_periods = view.FindViewById<LinearLayout>(Resource.Id.content_periods);
            var btnSendReport = view.FindViewById<ImageView>(Resource.Id.image_send_report);
            var btnEditToday = view.FindViewById<ImageView>(Resource.Id.image_go_today);


            btnSendReport.Click += (sender, e) =>
            {
                if (OnSendReportClick != null)
                {
                    OnSendReportClick(sender, vm);
                }
            };

            btnEditToday.Click += (sender, e) =>
            {
                if (OnCardItemClick != null)
                {
                    OnCardItemClick(DateTime.Now.Period(), vm);
                }
            };

            // Add Periods as needed
            var periods = vm.Periods;
            while (content_periods.ChildCount < periods.Count)
            {
                View pview = LayoutInflater.From(view.Context).Inflate(Resource.Layout.item_journal_period, content_periods, false);
                content_periods.AddView(pview);
            }

            Paint paint = new Paint { Color = Color.Rgb(25, 118, 210), StrokeWidth = 12 };
            var goalAmount = UserSettings.Current.GetGoal(jvm.SelectedProperty);
            float lastDrawn = 0;
            // sync periods
            for (int i = 0; i < content_periods.ChildCount; i++)
            {
                LinearLayout periodview = content_periods.GetChildAt(i) as LinearLayout;

                // unneeded periods are hidden
                periodview.Visibility = (i < periods.Count) ? ViewStates.Visible : ViewStates.Gone;

                if (i < periods.Count)
                {
                    var b = DataContext<JournalPeriodVM>.FromView(periodview);
                    b.VM = periods[i];
                    b.VM.RelatedProperty = jvm.SelectedProperty;
                    var p = periods[i].Period;

                    Task.Run(() =>
                    {
                        Platform.RunSafeOnUIThread("JournalFragment.OnResume",() =>
                        {
                            //  var width = periodview.LayoutParameters.Width;
                            var width = periodview.Width;
                            var pBar = periodview.FindViewById<ImageView>(Resource.Id.progressBar_period);

                            Bitmap bitmap = Bitmap.CreateBitmap(width, 8, Bitmap.Config.Argb8888);
                            Canvas canvas = new Canvas(bitmap);

                            Func<string, float> getNumber = delegate(string val)
                            {
                                try
                                {
                                    var raw = string.IsNullOrEmpty(val) ? "0" : val;
                                    if (raw.Contains("/"))
                                    {
                                        var tempArr = raw.Split(new char[] { '/' });
                                        return (float)(System.Convert.ToDecimal(tempArr[0]) / System.Convert.ToDecimal(tempArr[1]));
                                    }
                                    else
                                    {
                                        return (float)(System.Convert.ToDecimal(raw));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LittleWatson.ReportException(ex);
                                    return 0;
                                }
                            };

                            float total = (float.IsNaN(goalAmount) || goalAmount == 0) ? getNumber(vm.SelectedPropertyTotal)
                                : goalAmount;

                            if (total != 0)
                            {
                                var thisContribution = getNumber(b.VM.Value);
                                var startx = lastDrawn;
                                var endx = (float)Math.Ceiling((thisContribution / total) * width) + startx;
                                pBar.SetImageBitmap(bitmap);
                                if (endx > 0)
                                {
                                    lastDrawn = endx;
                                    canvas.DrawLine(startx, 0, endx, 0, paint);
                                    // System.Diagnostics.Debug.WriteLine(jvm.SelectedProperty + "- drawing for " + p.ToString() + " from:" + startx + " to:" + endx);
                                }
                            }
                        });
                    });

                    periodview.Click += (sender, e) =>
                    {
                        if (OnCardItemClick != null)
                        {
                            OnCardItemClick(p, vm);
                        }
                    };

                    if (b.Bindings.Count == 0)
                    {
                        // Period:
                        b.Add(Resource.Id.text_journal_period_text, x => x.Text);
                        b.Add(Resource.Id.text_journal_period_value, x => x.Value);
                        b.Add(Resource.Id.text_journal_period_time, x => x.Time);
                        b.Add(Resource.Id.text_journal_period_time, x => x.TimeVisibility);
                        b.Add(Resource.Id.text_journal_period_note, x => x.Note);
                        b.Add(Resource.Id.text_journal_period_note, x => x.NoteVisibility);
                    }

                    LinearLayout content_entries = periodview.FindViewById<LinearLayout>(Resource.Id.content_entries);

                    // Add entry views as needed
                    var lines = b.VM.Lines;
                    while (content_entries.ChildCount < lines.Count)
                    {
                        View eview = LayoutInflater.From(view.Context).Inflate(Resource.Layout.item_journal_entry, content_periods, false);
                        content_entries.AddView(eview);
                    }

                    // sync entries
                    for (int j = 0; j < content_entries.ChildCount; j++)
                    {

                        LinearLayout entryview = content_entries.GetChildAt(j) as LinearLayout;

                        // unneeded periods are hidden
                        entryview.Visibility = (j < lines.Count) ? ViewStates.Visible : ViewStates.Gone;

                        if (j < lines.Count)
                        {
                            // Line
                            entryview.FindViewById<TextView>(Resource.Id.text_journal_text).Text = lines[j].Text;
                            entryview.FindViewById<TextView>(Resource.Id.text_journal_value).Text = lines[j].Value;
                        }

                    }

                }

            }

        }
    }

    public class JournalFragment : Fragment
    {
        public DateTime date;
        public Property property;

        private JournalVM jvm;
        private JournalDayVM vm;
        View view;
        private LinearLayout advertisement;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Bundle bundle = savedInstanceState;
			DateTime date = Arguments.GetString("date").ToDateTime();
            property = Property.GetProperty(Arguments.GetString("property"));

        }

        private void MakeSureVM()
        {
            if (vm == null)
            {

				DateTime date = Arguments.GetString("date").ToDateTime();

                SessionLog.Debug(string.Format("New Journal: {0}", date));

                jvm = new JournalVM();
                vm = new JournalDayVM(jvm, date);
                jvm.SelectedProperty = property;
                jvm.SelectedDay = vm;

            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            try
            {

                Bundle bundle = savedInstanceState;
				DateTime date = Arguments.GetString("date").ToDateTime();

                property = Property.GetProperty(Arguments.GetString("property"));

                view = inflater.Inflate(Resource.Layout.fragment_journal, container, false);

                advertisement = view.FindViewById<LinearLayout>(Resource.Id.advertisement);
                var recycler = view.FindViewById<RecyclerView>(Resource.Id.recycler);

                MakeSureVM();
                recycler.HasFixedSize = false;

                LinearLayoutManager mLayoutManager = new LinearLayoutManager(Activity);
                recycler.SetLayoutManager(mLayoutManager);

                var vms = new ObservableCollection<JournalDayVM>() { vm, vm, vm };

                var todayAdapter = new JournalCardAdapter(jvm, this.Activity);
                todayAdapter.OnCardItemClick += OnCardItemClick;
                todayAdapter.OnSendReportClick += OnSendReportClick;
                recycler.AddOnScrollListener(new ScrollListener() { JournalCardAdapter = todayAdapter });

                mLayoutManager.ScrollToPosition(-todayAdapter.CurrentMin);	
                recycler.SetAdapter(todayAdapter);

#if DEBUG
                //AndroidDebug.SetViewBorders (view);
#endif


                return view;

            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
                return null;
            }
        }
        public override void OnResume()
        {
            base.OnResume();
            GrabAd();
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

        private void OnSendReportClick(object sender, JournalDayVM e)
        {
            var email = UserSettings.Current.Email;
            var recentEmail = UserSettings.Current.RecentEmail;
            ShowDialog(e, recentEmail ?? email, null);
        }

        private void OnCardItemClick(object sender, JournalDayVM e)
        {
            Intent i = new Intent(Activity, typeof(FoodJournal.Android15.Activities.ViewDayActivity));
            i.PutExtra("date", FoodJournal.Extensions.DateTimeExtensions.ToStorageStringDate(e.Date));
            i.PutExtra("dateText", e.DateText);
            i.PutExtra("period", (int)((Period)sender));
            StartActivity(i);
        }

        public void ShowDialog(JournalDayVM vm, string email, string error = null)
        {
            View dialogContent = View.Inflate(Activity, Resource.Layout.layout_email_dialog, null);
            var editText = dialogContent.FindViewById<EditText>(Resource.Id.edtEmail);
            editText.Text = email ?? "";
            editText.SetSelection(editText.Text.Length);
            var builder =
             new AlertDialog.Builder(Activity, Resource.Style.Dialog);
            builder.SetTitle(Resource.String.SendReportTitle);

            builder.SetView(dialogContent);
            builder.SetCancelable(false);
            builder.SetPositiveButton(Resource.String.Ok, (s, e) =>
            {
                email = editText.Text;
                if (string.IsNullOrEmpty((email ?? "").Trim()))
                {
                    ShowDialog(vm, email, GetString(Resource.String.InvalidEmailError));
                }
                else
                {
                    bool isEmail = Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
                    if (!isEmail)
                    {
                        ShowDialog(vm, email, GetString(Resource.String.InvalidEmailError));
                    }
                    else
                    {
                        vm.SendEmailReport(email);
                    }
                }
            });

            builder.SetNegativeButton(Resource.String.Cancel, (s, e) => { });
            builder.Show();

            if (error != null)
            {
                editText.SetError(error, null);
            }
        }

        public void Refresh()
        {
        }
    }
}

