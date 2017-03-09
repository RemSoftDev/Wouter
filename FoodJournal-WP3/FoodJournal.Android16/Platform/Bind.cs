using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Views;
using FoodJournal.AppModel;
using System.Linq.Expressions;
using FoodJournal.AppModel.UI;
using Android.Widget;
using FoodJournal.Logging;
using FoodJournal.Parsing;
using FoodJournal.Android15.Adapters;
using System.Collections.ObjectModel;

namespace FoodJournal.Android15
{

    public static class Convert
    {
        public delegate object Call(object o);

        public static Call FromVisibility = x => ((Visibility)x) == Visibility.Visible ? Android.Views.ViewStates.Visible : Android.Views.ViewStates.Gone;
        public static Call FromDateTimeToTimeString = x => ((DateTime)x).ToShortTimeString();
        public static Call FromTimeStringToDateTime = x =>
        {
            DateTime value;
            DateTime.TryParse((string)x, out value);
            return value;
        };
    }

    public class PropertyBinding<T> where T : VMBase
    {

        protected DataContext<T> Set;
        protected View View;
        // a child of the set.View

        public readonly string Name;
        private Convert.Call convertFrom;
        protected PropertyInfo VMProperty;

        private Convert.Call convertTo;
        private PropertyInfo targetProperty;

        private bool locked = false;

        private static View lastSource = null;

        public PropertyBinding(DataContext<T> set, int ResourceId, MemberExpression expression, string propertyName, Convert.Call convertFrom, Convert.Call convertTo)
        {

            try
            {

                this.Set = set;
                this.View = set.view.FindViewById(ResourceId);
                this.VMProperty = (PropertyInfo)expression.Member;
                this.Name = VMProperty.Name;

                Type t = View.GetType();
                targetProperty = t.GetProperty(propertyName);

                this.convertFrom = convertFrom;
                this.convertTo = convertTo;

                string EventName = propertyName == "Checked" ? "CheckedChange" : propertyName + "Changed";
                //if (EventName== "TextChanged") EventName = "FocusChange";

                EventInfo e = t.GetEvent(EventName);
                if (e != null)
                {

                    MethodInfo mi = this.GetType().GetMethod("EventHandler", BindingFlags.Public | BindingFlags.Instance);
                    Delegate del = Delegate.CreateDelegate(e.EventHandlerType, this, mi);
                    //e.AddEventHandler (View, del);
                    //Delegate del = new EventHandler(this.EventHandler);
                    e.AddEventHandler(View, del);
                    //e.AddEventHandler (View, this.EventHandler);
                }

                if (EventName == "TextChanged")
                {
                    e = t.GetEvent("FocusChange");
                    if (e != null)
                    {

                        MethodInfo mi = this.GetType().GetMethod("OnFocusChanged", BindingFlags.Public | BindingFlags.Instance);
                        Delegate del = Delegate.CreateDelegate(e.EventHandlerType, this, mi);
                        e.AddEventHandler(View, del);
                    }
                }

            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
        }

        private string RemoveThousandsIfNeeded(object control, string value)
        {
            EditText c = control as EditText;
            if (c == null)
                return value;

            if (value == null)
                return null;

            if (AppStats.CultureCommaNotADot)
                if ((c.InputType & Android.Text.InputTypes.NumberFlagDecimal) > 0)
                    if (c.HasFocus)
                        return value.Replace(".", "");

            return value;
        }

        public void OnFocusChanged(object sender, Android.Views.View.FocusChangeEventArgs e)
        {
            EditText c = sender as EditText;
            if (c == null)
                return;

            try
            {

                // 3/26/15: the android keyboard for cutures with a , still uses .
                // to fix this, weĺl take out any thousands separators before editing, and replace any . with , after editing
                if (AppStats.CultureCommaNotADot)
                    if ((c.InputType & Android.Text.InputTypes.NumberFlagDecimal) > 0)
                    {
                        if (e.HasFocus)
                        {
                            c.Text = RemoveThousandsIfNeeded(c, c.Text);
                        }
                        else
                        {
                            c.Text = c.Text.Replace('.', ',');
                        }
                    }

                //SessionLog.Debug("ToVM Event: " + Name);
                ToVM();
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }

        }

        public void EventHandler(object sender, EventArgs e)
        {
            try
            {

                //SessionLog.Debug("ToVM Event: " + Name);
                //Action a = () => {ToVM();};
                //a.BeginInvoke(null, null);
                ToVM();
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
        }

        #region Transfer

        public virtual void FromVM()
        {

            if (locked)
                return;

            try
            {

                if (!(View is RadioButton) && (View is TextView))
                    if (View == lastSource) // 3/26/15: this lastsource thing is actually kind of cool; it prevents updatnig the "properties" when selected (and dragging the bar), but nort equivalent for some reason
                        if (View is TextView && (View as TextView).HasFocus)
                            return;

                locked = true;

                object value = VMProperty.GetValue(Set.VM, null);

                if (convertFrom != null)
                    value = convertFrom(value);

                if (value is string)
                    if (View is EditText)
                        value = RemoveThousandsIfNeeded(View, value as string);

                //SessionLog.Debug(string.Format("FromVM: {0} ({1})", Name, value));

                if (targetProperty!=null)
                targetProperty.SetValue(View, value);

            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }

            locked = false;
        }

        public void ToVM()
        {
            if (locked)
                return;

            try
            {

                if (!(View is RadioButton || View is CheckBox) && (View is TextView))
                    if (View != lastSource)
                        if (!(View as TextView).HasFocus)
                            return;

                locked = true;

                object value = targetProperty.GetValue(View);

                if (convertTo != null)
                    value = convertTo(value);

                lastSource = View;

                //SessionLog.Debug(string.Format("ToVM: {0} ({1})", Name, value));

                VMProperty.SetValue(Set.VM, value, null);

            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }

            locked = false;
        }

        #endregion

    }

    public class PropertyCollectionBinding<T, U> : PropertyBinding<T>
        where T : VMBase
        where U : VMBase
    {

        private VMListAdapter<U> adapter;
        private ObservableCollection<U> list;

        public PropertyCollectionBinding(DataContext<T> set, int ViewGroupLayoutID, Android.Content.Context context, Expression<Func<T, ObservableCollection<U>>> property, int ItemLayoutID, VMListAdapter<U>.BindEvents BindEvents, VMListAdapter<U>.BindViewModel BindViewModel)
            : base(set, ViewGroupLayoutID, (MemberExpression)property.Body, "ChildCount", null, null)
        {

            list = VMProperty.GetValue(Set.VM, null) as ObservableCollection<U>;
            if (list == null) throw new ArgumentOutOfRangeException("Property");
            if (!(View is ViewGroup)) throw new ArgumentOutOfRangeException("ViewGroupLayoutID");

            list.CollectionChanged += (sender, e) => { FromVM(); };

            adapter = new VMListAdapter<U>(context, list, ItemLayoutID, BindEvents, BindViewModel);

        }

        public override void FromVM()
        {

            try
            {

                ObservableCollection<U> value = VMProperty.GetValue(Set.VM, null) as ObservableCollection<U>;
                if (value != list) throw new Exception("Changing the list reference is not supported yet");

                ViewGroup view = View as ViewGroup;

                view.RemoveAllViews();
                for (int i = 0; i < adapter.Count; i++)
                    view.AddView(adapter.GetView(i, null, view));

            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }

        }

    }

    public class EventBinding<T> where T : VMBase
    {

        protected DataContext<T> Set;
        protected View View;
        // a child of the set.View

        public readonly string Name;
        private Action<T> action;

        public EventBinding(DataContext<T> set, int ResourceId, Action<T> action, string EventName)
        {

            try
            {

                this.Set = set;
                this.View = set.view.FindViewById(ResourceId);
                this.action = action;

                Type t = View.GetType();
                EventInfo e = t.GetEvent(EventName);
                if (e != null)
                {

                    MethodInfo mi = this.GetType().GetMethod("EventHandler", BindingFlags.Public | BindingFlags.Instance);
                    Delegate del = Delegate.CreateDelegate(e.EventHandlerType, this, mi);
                    //					e.AddEventHandler (View, del);
                    //Delegate del = new EventHandler(this.EventHandler);
                    e.AddEventHandler(View, del);

                    //e.AddEventHandler (View, this.EventHandler);
                }

            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
        }

        public void EventHandler(object sender, EventArgs e)
        {
            try
            {

                //SessionLog.Debug(string.Format("EventHandler: {0}", Name));

                action.Invoke(Set.VM);

            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
        }

    }

    public class DataContext<T> where T : VMBase
    {
        public View view;
        public List<PropertyBinding<T>> Bindings = new List<PropertyBinding<T>>();
        public List<EventBinding<T>> EventBindings = new List<EventBinding<T>>();

        private T vm = null;

        #region FromView

        // TODO: Should put this on the Activity instead.
        private static Dictionary<View, DataContext<T>> All = new Dictionary<View, DataContext<T>>();

        public static DataContext<T> FromView(View view)
        {
            DataContext<T> result = null;
            All.TryGetValue(view, out result);
            if (result == null)
            {
                result = new DataContext<T>(view);
                All[view] = result;
            }
            return result;
        }

        #endregion

        private DataContext(View view)
        {
            this.view = view;
        }

        public T VM
        {
            get { return vm; }
            set
            {
                if (vm != null)
                    vm.PropertyChanged -= HandlePropertyChanged;
                vm = value;
                if (vm != null)
                    vm.PropertyChanged += HandlePropertyChanged;
                ApplyAll();
            }
        }

        void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Apply(e.PropertyName);
        }

        void ApplyAll()
        {
            Apply(null);
        }

        void Apply(string propertyname)
        {
            foreach (PropertyBinding<T> binding in Bindings)
                if (propertyname == null || binding.Name == propertyname)
                    binding.FromVM();
        }

        public void Add(PropertyBinding<T> binding)
        {
            Bindings.Add(binding);
            binding.FromVM();
        }

        public void Add(int ResourceId, Expression<Func<T, String>> property, string propertyName = null)
        {
            Add(new PropertyBinding<T>(this, ResourceId, (MemberExpression)property.Body, propertyName ?? "Text", null, null));
        }

        public void Add(int ResourceId, Expression<Func<T, DateTime>> property, string propertyName = null)
        {
            Add(new PropertyBinding<T>(this, ResourceId, (MemberExpression)property.Body, propertyName ?? "Text", Convert.FromDateTimeToTimeString, Convert.FromTimeStringToDateTime));
        }

        public void Add(int ResourceId, Expression<Func<T, Visibility>> property, string propertyName = null)
        {
            Add(new PropertyBinding<T>(this, ResourceId, (MemberExpression)property.Body, propertyName ?? "Visibility", Convert.FromVisibility, null));
        }

        public void Add(int ResourceId, Expression<Func<T, Boolean>> property, string propertyName = null)
        {
            Add(new PropertyBinding<T>(this, ResourceId, (MemberExpression)property.Body, propertyName ?? "Checked", null, null));
        }

        public void Add(int ResourceId, Expression<Func<T, float>> property, string propertyName = null)
        {
            Add(new PropertyBinding<T>(this, ResourceId, (MemberExpression)property.Body, propertyName ?? "Value", null, null));
        }

        public void Add(int ResourceId, Expression<Func<T, int>> property, string propertyName = null)
        {
            Add(new PropertyBinding<T>(this, ResourceId, (MemberExpression)property.Body, propertyName ?? "Value", null, null));
        }

        public void Add(int ResourceId, Action<T> action, string eventName = "Click")
        {
            EventBindings.Add(new EventBinding<T>(this, ResourceId, action, eventName));
        }

    }
}

