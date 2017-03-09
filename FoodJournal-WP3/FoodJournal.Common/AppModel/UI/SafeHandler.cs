using FoodJournal.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#if WINDOWS_PHONE
using System.Windows.Controls;
#endif

namespace FoodJournal.AppModel.UI
{
    public class SafeHandler
    {

        private Action action;

        public SafeHandler(Action a) { action = a; }

        public void Invoke(object sender, EventArgs e)
        {
            try { action.Invoke(); }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        public static implicit operator EventHandler<CancelEventArgs>(SafeHandler value) { return new EventHandler<CancelEventArgs>(value.Invoke); }
#if WINDOWS_PHONE
        public static implicit operator TextChangedEventHandler(SafeHandler value) { return new TextChangedEventHandler(value.Invoke); }
        public static implicit operator EventHandler(SafeHandler value) { return new EventHandler(value.Invoke); }
        public static implicit operator RoutedEventHandler(SafeHandler value) { return new RoutedEventHandler(value.Invoke); }
#endif

    }
}
