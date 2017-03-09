using System;
using Android.Widget;
using System.Linq.Expressions;
using System.Reflection;
using Android.App;
using FoodJournal.AppModel.UI;
using System.Collections.Generic;
using System.ComponentModel;

namespace FoodJournal.Android15
{

	public class NotifySinker
	{
		private Dictionary<string, Action> sink = new Dictionary<string, Action> ();
		public NotifySinker(INotifyPropertyChanged obj) {obj.PropertyChanged += (sender, e) => {
				this.Invoke (e.PropertyName);
			};}
		public void Add(string propertyname, Action action) {sink.Add(propertyname,action);}
		public void Invoke(string propertyname) {			var action = sink [propertyname];			if (action != null)				action.Invoke ();		}
	}

	public class Binding
	{

		public delegate void write(bool value);

		public static void Make<T> (Activity activity, int ResourceId, T obj, Expression<Func<T,String>> property, NotifySinker sink = null)
		{
			var box = activity.FindViewById<EditText> (ResourceId);
			var memberExpression = (MemberExpression)property.Body;
			var propertyInfo = (PropertyInfo)memberExpression.Member;

			box.Text = (String)propertyInfo.GetValue(obj, null);
			box.TextChanged += (sender, e) => {
				propertyInfo.SetValue(obj, box.Text, null);
			};

			if (sink != null)
				sink.Add (propertyInfo.Name, () => {
					box.Text = ((String)propertyInfo.GetValue(obj, null));
				});

		}

		public static void Make<T> (Activity activity, int ResourceId, T obj, Expression<Func<T,DateTime>> property, NotifySinker sink = null)
		{
			var box = activity.FindViewById<EditText> (ResourceId);
			var memberExpression = (MemberExpression)property.Body;
			var propertyInfo = (PropertyInfo)memberExpression.Member;

			box.Text = ((DateTime)propertyInfo.GetValue(obj, null)).ToShortTimeString();
			box.TextChanged += (sender, e) => {
				DateTime time = DateTime.MinValue;
				if (DateTime.TryParse(box.Text,out time))
					propertyInfo.SetValue(obj, time, null);
			};

			if (sink != null)
				sink.Add (propertyInfo.Name, () => {
					box.Text = ((DateTime)propertyInfo.GetValue(obj, null)).ToShortTimeString();
				});

		}

		public static void Make<T> (Activity activity, int ResourceId, T obj, Expression<Func<T,bool>> property, NotifySinker sink = null)
		{
			var box = activity.FindViewById<CheckBox> (ResourceId);
			var memberExpression = (MemberExpression)property.Body;
			var propertyInfo = (PropertyInfo)memberExpression.Member;

			box.Checked = (bool)propertyInfo.GetValue(obj, null);
			box.CheckedChange += (sender, e) => {
				propertyInfo.SetValue(obj, box.Checked, null);
			};

			if (sink != null)
				sink.Add (propertyInfo.Name, () => {
					box.Checked = ((bool)propertyInfo.GetValue(obj, null));
				});

		}

		public static void Make<T> (Activity activity, int ResourceId, T obj, Expression<Func<T,Visibility>> property, NotifySinker sink = null)
		{
			var box = activity.FindViewById (ResourceId);
			var memberExpression = (MemberExpression)property.Body;
			var propertyInfo = (PropertyInfo)memberExpression.Member;

			box.Visibility = ((Visibility)propertyInfo.GetValue(obj, null)) == Visibility.Visible ? Android.Views.ViewStates.Visible : Android.Views.ViewStates.Gone;

			if (sink != null)
				sink.Add (propertyInfo.Name, () => {
					box.Visibility = ((Visibility)propertyInfo.GetValue(obj, null)) == Visibility.Visible ? Android.Views.ViewStates.Visible : Android.Views.ViewStates.Gone;
				});
		}


	}
}

