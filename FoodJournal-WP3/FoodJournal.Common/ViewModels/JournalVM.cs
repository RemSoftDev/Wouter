using FoodJournal.Logging;
using FoodJournal.Model;
using FoodJournal.Values;
using FoodJournal;
using FoodJournal.WinPhone.Common.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using FoodJournal.AppModel;
using FoodJournal.Runtime;


#if WINDOWS_PHONE
using System.Data.Linq;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Windows.Media;
#endif
using FoodJournal.AppModel.UI;
using FoodJournal.Parsing;
using FoodJournal.Messages;
using FoodJournal.Resources;

namespace FoodJournal.ViewModels
{

    public class JournalViewHeaderVM : INotifyPropertyChanged
    {

        private string header;
        public string Header { get { return header; } set { if (header != value) { header = value; NotifyPropertyChanged("Header"); } } }

        private string subheader;
        public string SubHeader { get { return subheader; } set { if (subheader != value) { subheader = value; NotifyPropertyChanged("SubHeader"); NotifyPropertyChanged("HeaderMargin"); } } }

        private string value;
        public string Value { get { return value; } set { if (this.value != value) { this.value = value; NotifyPropertyChanged("Value"); } } }

		#if WINDOWS_PHONE
        public Thickness HeaderMargin { get { return string.IsNullOrEmpty(SubHeader) ? new Thickness(12, 12, 0, 0) : new Thickness(12, 0, 0, 0); } }
		#endif

        //public JournalViewHeaderVM(string Header, string SubHeader, string Value) { this.Header = Header; this.SubHeader = SubHeader; this.Value = Value; }
        public JournalViewHeaderVM(string Header) { this.Header = Header; }

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }

	public class JournalLineItemVM : VMBase//INotifyPropertyChanged
    {
        public Entry entry;

        private string text;
        public string Text { get { return text; } set { if (text != value) { text = value; NotifyPropertyChanged("Text"); } } }

        private string value;
        public string Value { get { return value; } set { if (this.value != value) { this.value = value; NotifyPropertyChanged("Value"); } } }

        public TranslateTransform ValuesTransform { get; set; }
        public JournalLineItemVM(Entry entry, TranslateTransform transform) { this.entry = entry; ValuesTransform = transform; }

//        #region NotifyPropertyChanged
//
//        public event PropertyChangedEventHandler PropertyChanged;
//        private void NotifyPropertyChanged(String propertyName)
//        {
//            PropertyChangedEventHandler handler = PropertyChanged;
//            if (null != handler)
//            {
//                handler(this, new PropertyChangedEventArgs(propertyName));
//            }
//        }
//
//        #endregion

    }

	public class JournalPeriodVM : VMBase //INotifyPropertyChanged
    {

        public Period Period;

        private string text;
        public string Text { get { return text; } set { if (text != value) { text = value; NotifyPropertyChanged("Text"); } } }

        private string value;
        public string Value { get { return value; } set { if (this.value != value) { this.value = value; NotifyPropertyChanged("Value"); } } }

		private int barwidth;
		public int BarWidth { get {
            var goals = UserSettings.Current.GetGoal(RelatedProperty);    
            return barwidth; 
        
        } set { if (barwidth != value) { barwidth = value; NotifyPropertyChanged("BarWidth"); } } }

		private string note;
		public string Note { get { return note; } set { if (this.note != value) { this.note = value; NotifyPropertyChanged("Note"); NotifyPropertyChanged("NoteVisibility");} } }
		public Visibility NoteVisibility { get { return string.IsNullOrEmpty(note) ? Visibility.Collapsed : Visibility.Visible; } }

		private string time;
		public string Time { get { return time; } set { if (this.time != value) { this.time = value; NotifyPropertyChanged("Value"); NotifyPropertyChanged("TimeVisibility");} } }
		public Visibility TimeVisibility 
        { 
            get {
                Visibility visibility =string.IsNullOrEmpty(time) ? Visibility.Collapsed : Visibility.Visible; 
                return visibility == Visibility.Collapsed ? Visibility.Collapsed : (UserSettings.Current.ShowMealTime ? Visibility.Visible : Visibility.Collapsed);
            } 
        }

        public ObservableCollection<JournalLineItemVM> Lines { get; set; }
        public TranslateTransform ValuesTransform { get; set; }
        public JournalPeriodVM(Period period, string Text, TranslateTransform transform) { this.Period = period; this.Text = Text; Lines = new ObservableCollection<JournalLineItemVM>(); ValuesTransform = transform; }

//        #region NotifyPropertyChanged
//
//        public event PropertyChangedEventHandler PropertyChanged;
//        private void NotifyPropertyChanged(String propertyName)
//        {
//            PropertyChangedEventHandler handler = PropertyChanged;
//            if (null != handler)
//            {
//                handler(this, new PropertyChangedEventArgs(propertyName));
//            }
//        }
//
//        #endregion
//
        public Visibility BarVisibility
        {
            get
            {
                Visibility visibility = string.IsNullOrEmpty(time) ? Visibility.Collapsed : Visibility.Visible;
                return visibility == Visibility.Collapsed ? Visibility.Collapsed : (UserSettings.Current.ShowMealTime ? Visibility.Visible : Visibility.Collapsed);
            }
        }

        public Property RelatedProperty { get; set; }
    }

    public class JournalDayVM : VMBase //INotifyPropertyChanged
    {

        public const int GoalWidth = 500;
        private const int MAXLINESPERMEALINREPORT = 10;

        public TranslateTransform ValuesTransform { get; set; }

        private readonly JournalVM journalVM;
        private DateTime date;

        private Dictionary<Property, JournalViewHeaderVM> headers = new Dictionary<Property, JournalViewHeaderVM>();

        public DateTime Date { get { return date; } }
		public string DateText { get { return (date == DateTime.Now.Date) ? AppResources.Today : date.ToString("dddd"); } }
		public string DateShort { get { return date.ToShortDateString(); } }
		public string SelectedPropertyTotal { get { return GetHeader (journalVM.SelectedProperty).Value; } }
		public string SelectedPropertyDelta { get { return GetHeader (journalVM.SelectedProperty).SubHeader; } }
				
        public ObservableCollection<JournalPeriodVM> Periods { get; set; }

        public JournalDayVM(JournalVM journalVM, DateTime date)
        {
            this.journalVM = journalVM;
            this.date = date.Date;
            Periods = new ObservableCollection<JournalPeriodVM>();
        }

        private Visibility emailSettingsVisibility = Visibility.Collapsed;
        public Visibility EmailSettingsVisibility
        {
            get { return emailSettingsVisibility; }
            set
            {
                emailSettingsVisibility = value;
                NotifyPropertyChanged("EmailSettingsVisibility");
            }
        }

        public string Email { get { return UserSettings.Current.Email; } set { UserSettings.Current.Email = value; } }

        public JournalViewHeaderVM GetHeader(Property prop)
        {
            JournalViewHeaderVM value;
            if (headers.TryGetValue(prop, out value)) return value;
            value = new JournalViewHeaderVM(prop.FullCapitalizedText);
            headers.Add(prop, value);
            return value;
        }

        private void SyncHeader(Property prop)
        {

            try
            {

                Single goal = UserSettings.Current.GetGoal(prop);
                Amount total = Cache.GetPropertyTotal(date, prop);

                JournalViewHeaderVM header = GetHeader(prop);
                header.Value = total.ValueString();

                if (Single.IsNaN(goal) || goal == 0)
                    header.SubHeader = "";
                else
                {
                    var remaining = goal - total.ToSingle();
                    if (remaining < 0)
                        header.SubHeader = string.Format(AppResources.GoalOver, Floats.ToUIString(-remaining));
                    else
                        header.SubHeader = string.Format(AppResources.GoalUnder, Floats.ToUIString(remaining));
                }

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        public void Sync(DateTime date)
        {

            try
            {

                this.date = date;
                var prop = journalVM.SelectedProperty;
                Single goal = UserSettings.Current.GetGoal(prop);
                Amount GoalAmount = Amount.FromProperty(goal, prop);
                var cache = Cache.GetEntryCache(date);

                SyncHeader(prop);
                SyncHeader(journalVM.NextProperty());

                int pi = 0;
				foreach (Period p in Cache.GetDatePeriods(date))
                {
                    Amount value = Cache.GetPeriodPropertyValue(date, p, prop);
                    int width = (int)(GoalWidth * (value / GoalAmount));
					if (GoalAmount.IsZero) width=0;
                    width = width <= 0 ? 0 : width;

                    JournalPeriodVM pvm = null;
                    while (Periods.Count > pi && Periods[pi].Period != p)
                        Periods.RemoveAt(pi);

                    if (Periods.Count > pi)
                        pvm = Periods[pi];
                    else
                    {
                        pvm = new JournalPeriodVM(p, Strings.FromEnum(p), ValuesTransform);
                        Periods.Add(pvm);
                    }

                    pvm.Value = value.ValueString();
                    pvm.BarWidth = width;
					pvm.Note = Cache.GetPeriodNote(date,p);
					pvm.Time = Cache.GetPeriodTime(date,p);

                    int i = 0;
                    JournalLineItemVM line;
                    foreach (var entry in cache[(int)p])
                    {
                        while (pvm.Lines.Count > i && pvm.Lines[i].entry != entry)
                            pvm.Lines.RemoveAt(i);
                        if (pvm.Lines.Count > i)
                            line = pvm.Lines[i];
                        else
                        {
                            line = new JournalLineItemVM(entry, ValuesTransform);
                            pvm.Lines.Add(line);
                        }
                        line.Text = entry.EntryText;
                        line.Value = entry.GetPropertyValue(prop).ValueString();
                        i++;
                    }

                    if (pvm.Lines.Count == 0)
                        pvm.Lines.Add(new JournalLineItemVM(null, ValuesTransform) { Text = AppResources.EmptyEntryList });

                    pi++;
                }
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        private string MakeEmailReportHtml()
        {

            StringBuilder output = new StringBuilder();

            string style = @"    <style>
        body {
            font-family: 'Segoe UI Light', Tahoma, Geneva, Verdana, sans-serif;
        }

        h1 {
            font-size: xx-large;
            color: #248;
        }

        th {
            background-color: #36C;
            color: #FFFFFF;
        }

        td {
            text-align: right;
            width: 200px;
        }

            td.text {
                text-align: left;
                width: 800px;
            }

        tr {
            font-size: small;
            color: #666;
        }

        .period, .total, .total2 {
            font-size: medium;
            background-color: #EEE;
            color: #000;
        }

        .total {
            font-size: large;
            font-weight: bold;
        }

        .total2 {
            font-size: small;
        }

        .footer {
            font-size: smaller;
        }

        .note {
            font-size: xx-small;
        }
    </style>
";

            style = style.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');
            while (style.Contains("  "))
                style = style.Replace("  ", " ");

            output.Append(style);

            output.Append("<h1>");
            output.Append(AppResources.ApplicationTitle);
            output.Append("</h1>");

            output.Append("<h2>");
            output.Append(date.ToLongDateString());
            output.Append("</h2>");

            output.Append("<table>");

            // header row
            output.Append("<tr>");
            output.Append("<th>");
            output.Append("</th>");
            foreach (var prop in UserSettings.Current.SelectedProperties)
            {
                output.Append("<th>");
                output.Append(prop.FullText);
                output.Append("</th>");
            }
            output.Append("</tr>");

            foreach (var period in Periods)
            {
                output.Append("<tr class=\"period\">");
                output.Append("<td class=\"text\">");
                output.Append(period.Text);
                output.Append("</td>");

                foreach (var prop in UserSettings.Current.SelectedProperties)
                {
                    output.Append("<td>");
                    output.Append(Cache.GetPeriodPropertyValue(date, period.Period, prop).ValueString());
                    output.Append("</td>");
                }

                output.Append("</tr>");

                int i = 0;
                foreach (var line in period.Lines)
                {

                    output.Append("<tr>");
                    output.Append("<td class=\"text\">");
                    output.Append(line.Text);
                    output.Append("</td>");

                    if (line.entry != null)
                    {
                        foreach (var prop in UserSettings.Current.SelectedProperties)
                        {
                            output.Append("<td>");
                            output.Append(line.entry.GetPropertyValue(prop).ValueString());
                            output.Append("</td>");
                        }
                    }

                    output.Append("</tr>");

                    if (i++ > MAXLINESPERMEALINREPORT) break;
                }

            }

            output.Append("<tr class=\"total\">");
            output.Append("<td class=\"text\">");
            output.Append(AppResources.Total);
            output.Append("</td>");

            foreach (var prop in UserSettings.Current.SelectedProperties)
            {
                output.Append("<td>");
                output.Append(Cache.GetPropertyTotal(date, prop).ValueString());
                output.Append("</td>");
            }

            output.Append("</tr>");

            output.Append("<tr class=\"total2\">");
            output.Append("<td></td>");

            foreach (var prop in UserSettings.Current.SelectedProperties)
            {
                output.Append("<td>");

                Single goal = UserSettings.Current.GetGoal(prop);
                if (!Single.IsNaN(goal) && goal != 0)
                {
                    string text;
                    Amount total = Cache.GetPropertyTotal(date, prop);
                    var remaining = goal - total.ToSingle();
                    if (remaining < 0)
                        text = string.Format(AppResources.GoalOver, Floats.ToUIString(-remaining));
                    else
                        text = string.Format(AppResources.GoalUnder, Floats.ToUIString(remaining));
                    output.Append(text);
                }

                output.Append("</td>");
            }

            output.Append("</tr>");

            output.Append("</table>");

            output.Append("<p class=\"footer\">");
			#if WINDOWS_PHONE
            var aaa = @"<a href=""http://www.windowsphone.com/s?appid=2f44a06e-3d7c-4e11-b74d-9135949a1889"">" + AppResources.AppTitle + "</a><br />";
			#else
			var aaa = @"<a href=""https://play.google.com/store/apps/details?id=" + Navigate.navigationContext.ApplicationContext.PackageName + @""">" + AppResources.AppTitle + "</a><br />";
			//var aaa = @"<a href=""https://play.google.com/store/apps/details?id=app.dailybits.foodjournal"">" + AppResources.AppTitle + "</a><br />";
			#endif
			var bbb = @"<a href=""mailto:" +AppStats.email + @""">"+ AppStats.email + "</a>";

            if (!string.IsNullOrEmpty(AppResources.EmailReport_Footer1a))
            {
                output.Append(AppResources.EmailReport_Footer1a.Replace("AAA", aaa));
                output.Append("<br />");
                output.Append(AppResources.EmailReport_Footer1b.Replace("BBB", bbb));
            }
            else
            {
                output.Append(AppResources.EmailReport_Footer1.Replace("AAA", aaa).Replace("BBB", bbb));
            }
            output.Append("</p>");

            output.Append("<p class=\"note\">");
            output.Append(AppResources.EmailReport_Footer2);
            output.Append("</p>");

            return output.ToString();

        }

        public void SendEmailReport(string email)
        {
            try
            {
                string to = email;

                if (string.IsNullOrEmpty(to) || !to.Contains('@'))
                {
					Platform.MessageBox(AppResources.EmailReport_InvalidEmailAddress);
                }
                else
                {

                    var Subject = string.Format(AppResources.EmailReport_SubjectTemplate, date.ToLongDateString());
                    var Body = MakeEmailReportHtml();

					MessageQueue.SendEmail(email, Subject, Body);
                    EmailSettingsVisibility = Visibility.Collapsed;

                    UserSettings.Current.RecentEmail = email;
                }
            }
            catch (Exception ex)
            {
				Platform.MessageBox(AppResources.EmailReport_Exception);
                LittleWatson.ReportException(ex);
            }
        }

//        #region NotifyPropertyChanged
//
//        public event PropertyChangedEventHandler PropertyChanged;
//        private void NotifyPropertyChanged(String propertyName)
//        {
//            PropertyChangedEventHandler handler = PropertyChanged;
//            if (null != handler)
//            {
//                handler(this, new PropertyChangedEventArgs(propertyName));
//            }
//        }
//
//        #endregion

    }

    public class JournalVM : INotifyPropertyChanged
    {

        private JournalDayVM selectedDay;
        private Property selectedProperty;

        public JournalVM()
        {
            try
            {
                selectedProperty = UserSettings.Current.SelectedProperties[0];

                Property current = UserSettings.Current.CurrentProperty;
                foreach (var property in UserSettings.Current.SelectedProperties)
                    if (property == current) selectedProperty = current;

            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
                selectedProperty = StandardProperty.Calories;
            }
        }

        public string LongDateText
        {
            get
            {
                if (selectedDay == null) return AppResources.Today;
                return (selectedDay.Date == DateTime.Now.Date) ? AppResources.Today : selectedDay.Date.ToLongDateString();
            }
        }
        public string PageTitle { get { return LongDateText.ToUpper(); } }

        public Property SelectedProperty
        {
            get { return selectedProperty; }
            set
            {
                selectedProperty = value;
                UserSettings.Current.CurrentProperty = value;
            }
        }

        public Property NextProperty()
        {
            try
            {
                List<Property> sel = UserSettings.Current.SelectedProperties;
                for (int i = 0; i < sel.Count - 1; i++)
                    if (sel[i] == selectedProperty) return sel[i + 1];
                return sel[0];
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
            return StandardProperty.Calories;
        }

        public JournalDayVM SelectedDay
        {
            get { return selectedDay; }
            set
            {
                selectedDay = value;
                NotifyPropertyChanged("PageTitle");
            }
        }

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }

}
