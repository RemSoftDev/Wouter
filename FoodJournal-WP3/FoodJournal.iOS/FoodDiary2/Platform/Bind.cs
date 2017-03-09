using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using FoodJournal.AppModel;
using System.Linq.Expressions;

//using FoodJournal.Logging;
//using FoodJournal.Parsing;

using System.Collections.ObjectModel;
using UIKit;
using FoodJournal.Logging;
using FoodJournal.AppModel.UI;

namespace FoodDiary2.iOS
{

	public static class Convert
	{
		public delegate object Call(object o);

		public static Call FromVisibility = x => ((Visibility)x) == Visibility.Visible ? true : false ;
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
		protected UIView View;
		// a child of the set.View

		public readonly string Name;
		private Convert.Call convertFrom;
		protected PropertyInfo VMProperty;

		private Convert.Call convertTo;
		private PropertyInfo targetProperty;

		private bool locked = false;

		private static UIView lastSource = null;

		public PropertyBinding(DataContext<T> set, UIView view, MemberExpression expression, string propertyName, Convert.Call convertFrom, Convert.Call convertTo)
		{

			try
			{

				this.Set = set;
				this.View = view;// set.view.ViewWithTag(ResourceId);
				this.VMProperty = (PropertyInfo)expression.Member;
				this.Name = VMProperty.Name;

				Type t = View.GetType();
				targetProperty = t.GetProperty(propertyName);

				this.convertFrom = convertFrom;
				this.convertTo = convertTo;

				//string EventName = propertyName == "Checked" ? "CheckedChange" : propertyName + "Changed";
				string EventName = propertyName == "On" ? "ValueChanged" : propertyName + "Changed";
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
				//LittleWatson.ReportException(ex);
			}
		}

		private string RemoveThousandsIfNeeded(object control, string value)
		{
//			EditText c = control as EditText;
			UITextField c = control as UITextField;


			if (c == null)
				return value;

			if (value == null)
				return null;

			if (AppStats.CultureCommaNotADot)
			if (c.KeyboardType == UIKeyboardType.DecimalPad)
			if (c.Focused)
				return value.Replace(".", "");

			return value;
		}

		public void OnFocusChanged(object sender, EventArgs e)
		{
			UITextField c = sender as UITextField;
			if (c == null)
				return;

			try
			{

				// 3/26/15: the android keyboard for cutures with a , still uses .
				// to fix this, weĺl take out any thousands separators before editing, and replace any . with , after editing
				if (AppStats.CultureCommaNotADot)
				if (c.KeyboardType == UIKeyboardType.DecimalPad)
				{
					if (c.Focused)
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
//				LittleWatson.ReportException(ex);
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
//				LittleWatson.ReportException(ex);
			}
		}

		#region Transfer

		public virtual void FromVM()
		{

			if (locked)
				return;

			try
			{

				if (!(View is UISwitch ) && (View is UILabel))
				if (View == lastSource) // 3/26/15: this lastsource thing is actually kind of cool; it prevents updatnig the "properties" when selected (and dragging the bar), but nort equivalent for some reason
				if (View is UILabel && (View as UITextView).Focused)
					return;

				locked = true;

				object value = VMProperty.GetValue(Set.VM, null);

				if (convertFrom != null)
					value = convertFrom(value);

				if (value is string)
				if (View is UITextView)
					value = RemoveThousandsIfNeeded(View, value as string);

				//SessionLog.Debug(string.Format("FromVM: {0} ({1})", Name, value));

				if (targetProperty!=null)
					targetProperty.SetValue(View, value);

			}
			catch (Exception ex)
			{
				//LittleWatson.ReportException(ex);
			}

			locked = false;
		}

		public void ToVM()
		{
			if (locked)
				return;

			try
			{

				if (!(View is UISwitch) && (View is UITextView))
				if (View != lastSource)
				if (!(View as UITextView).Focused)
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

//		private VMListAdapter<U> adapter;
		private ObservableCollection<U> list;

		public PropertyCollectionBinding(DataContext<T> set, UIView view, Expression<Func<T, ObservableCollection<U>>> property
//			int ItemLayoutID, VMListAdapter<U>.BindEvents BindEvents, VMListAdapter<U>.BindViewModel BindViewModel
		)
			: base(set, view, (MemberExpression)property.Body, "ChildCount", null, null)
		{

//			list = VMProperty.GetValue(Set.VM, null) as ObservableCollection<U>;
//			if (list == null) throw new ArgumentOutOfRangeException("Property");
//			if (!(View is ViewGroup)) throw new ArgumentOutOfRangeException("ViewGroupLayoutID");
//
//			list.CollectionChanged += (sender, e) => { FromVM(); };
//
//			adapter = new VMListAdapter<U>(context, list, ItemLayoutID, BindEvents, BindViewModel);

		}

		public override void FromVM()
		{

			try
			{

				ObservableCollection<U> value = VMProperty.GetValue(Set.VM, null) as ObservableCollection<U>;
				if (value != list) throw new Exception("Changing the list reference is not supported yet");

//				ViewGroup view = View as ViewGroup;
//
//				view.RemoveAllViews();
//				for (int i = 0; i < adapter.Count; i++)
//					view.AddView(adapter.GetView(i, null, view));

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
		protected UIView View;
		// a child of the set.View

		public readonly string Name;
		private Action<T> action;

		public EventBinding(DataContext<T> set, UIView view, Action<T> action, string EventName)
		{

			try
			{

				this.Set = set;
				//				this.View = set.view.FindViewById(ResourceId);
				this.View = view;//set.view.ViewWithTag(tag);
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
				//LittleWatson.ReportException(ex);
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
				//				LittleWatson.ReportException(ex);
			}
		}

	}

	public class DataContext<T> where T : VMBase
	{
		public UIView view;
		public List<PropertyBinding<T>> Bindings = new List<PropertyBinding<T>>();
		public List<EventBinding<T>> EventBindings = new List<EventBinding<T>>();

		private T vm = null;

		#region FromView

		// TODO: Should put this on the Activity instead.
		private static Dictionary<UIView, DataContext<T>> All = new Dictionary<UIView, DataContext<T>>();

		public static DataContext<T> FromView(UIView view)
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

		private DataContext(UIView view)
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

		public void Add(UIView view, Expression<Func<T, String>> property, string propertyName = null)
		{
			Add(new PropertyBinding<T>(this, view, (MemberExpression)property.Body, propertyName ?? "Text", null, null));
		}

		public void Add(UIView view, Expression<Func<T, DateTime>> property, string propertyName = null)
		{
			Add(new PropertyBinding<T>(this, view, (MemberExpression)property.Body, propertyName ?? "Text", Convert.FromDateTimeToTimeString, Convert.FromTimeStringToDateTime));
		}

		public void Add(UIView view, Expression<Func<T, Visibility>> property, string propertyName = null)
		{
			Add(new PropertyBinding<T>(this, view, (MemberExpression)property.Body, propertyName ?? "Visibility", Convert.FromVisibility, null));
		}

		public void Add(UIView view, Expression<Func<T, Boolean>> property, string propertyName = null)
		{
			Add(new PropertyBinding<T>(this, view, (MemberExpression)property.Body, propertyName ?? "On", null, null));
		}

		public void Add(UIView view, Expression<Func<T, float>> property, string propertyName = null)
		{
			Add(new PropertyBinding<T>(this, view, (MemberExpression)property.Body, propertyName ?? "Value", null, null));
		}

		public void Add(UIView view, Expression<Func<T, int>> property, string propertyName = null)
		{
			Add(new PropertyBinding<T>(this, view, (MemberExpression)property.Body, propertyName ?? "Value", null, null));
		}

		public void Add(UIView view, Action<T> action, string eventName = "Click")
		{
			EventBindings.Add(new EventBinding<T>(this, view, action, eventName));
		}

	}
}

