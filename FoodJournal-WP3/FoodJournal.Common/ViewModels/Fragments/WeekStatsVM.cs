using FoodJournal.Logging;
using FoodJournal.Model;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Values;
using FoodJournal.Parsing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using FoodJournal.AppModel.UI;
using FoodJournal.Resources;

namespace FoodJournal.ViewModels
{
    public class WeekStatsVM : INotifyPropertyChanged
    {

        public struct StatsDayVM //: INotifyPropertyChanged
        {

            public DateTime date;
            public Amount value;

            public string Text { get { return date == DateTime.Now.Date ? AppResources.Today : date.ToString("ddd"); } }

            public int top;

        }

        public readonly Property property;

        private Color color;

        public Brush Brush { get { return new SolidColorBrush(color); } }
        public string Text { get { return property.FullText; } }
        public Visibility GoalVisibility { get { return LastGoal.IsZero ? Visibility.Collapsed : Visibility.Visible; } }

        public Amount LastGoal = Amount.Zero;
        public Amount LastMax = Amount.Zero;

        public int goalTop;
        public int GoalTop { get { return goalTop; } }

        private string highBound;
        private string mediumBound;
        public string HighBound { get { return highBound; } }
        public string MediumBound { get { return mediumBound; } }
        public string LowBound { get { return "0"; } }

        public StatsDayVM[] day = new StatsDayVM[7];
        public StatsDayVM[] Day { get { return day; } }

        public WeekStatsVM(Property property)
        {
            this.property = property;
			#if WINDOWS_PHONE
            this.color = (Color)Application.Current.Resources["PhoneAccentColor"];
			#endif
            //property.GetColor();

            for (int i = 0; i < 7; i++)
            {
                Day[i].date = DateTime.Now.Date.AddDays(i * -1);
            }

            SyncAll();
        }

        private int Top(Amount value, Amount max)
        {
            Single pct = value / max;
            if (pct > 1) pct = 1;
            return (int)(162 + -1 * pct * 150);
        }

        public void SyncAll()
        {

            Single goal = UserSettings.Current.GetGoal(property);

            Amount Goal;

            if (Single.IsNaN(goal) || goal == 0)
                Goal = Amount.Zero;// property.FormatValue("1");// Property.DailyGoal[property];
            else
                Goal = Amount.FromProperty(goal, property);

            Amount Max = Goal;

            for (int i = 0; i < 7; i++)
            {
                Day[i].value = Cache.GetPropertyTotal(Day[i].date, property);
                if (Max < Day[i].value) Max = Day[i].value;
            }

            bool GoalChanged = (LastGoal != Goal) || (LastMax != Max);
            LastGoal = Goal;
            LastMax = Max;

            goalTop = Top(Goal, Max);
            if (GoalChanged)
            {

                highBound = Max.ValueString();
                mediumBound = (Max * .5).ValueString();

                NotifyPropertyChanged("GoalTop");
                NotifyPropertyChanged("HighBound");
                NotifyPropertyChanged("MediumBound");
                NotifyPropertyChanged("GoalVisibility");

            }

            for (int i = 0; i < 7; i++)
            {
                var old = Day[i].top;
                Day[i].top = Top(Day[i].value, Max);
                if (Day[i].top != old) NotifyPropertyChanged("Top" + i.ToString());
            }

        }

        public int Top0 { get { return Day[0].top; } }
        public int Top1 { get { return Day[1].top; } }
        public int Top2 { get { return Day[2].top; } }
        public int Top3 { get { return Day[3].top; } }
        public int Top4 { get { return Day[4].top; } }
        public int Top5 { get { return Day[5].top; } }
        public int Top6 { get { return Day[6].top; } }
        public int Top7 { get { return Day[7].top; } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
