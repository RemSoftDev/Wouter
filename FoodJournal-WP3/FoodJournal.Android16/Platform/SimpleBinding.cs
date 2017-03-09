using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Views;
using FoodJournal.AppModel;
using System.Linq.Expressions;
using FoodJournal.AppModel.UI;
using Android.Widget;

namespace FoodJournal.Android15
{
	#if false

	public abstract class PropertyBinding<T>
	{
		protected ViewBindingSet<T> Set;
		protected View View; // a child of the set.View
		protected PropertyInfo member;
		public readonly string Name;
		private bool locked=false;

		public void FromVM() {locked = true;FromVM(member.GetValue(Set.VM, null));locked = false;}
		protected abstract void FromVM (object value);
		protected void ToVM (object value) {if (!locked) member.SetValue(Set.VM, value, null);}

		public PropertyBinding(ViewBindingSet<T> set, int ResourceId, MemberExpression expression) {
			this.Set = set; 
			this.View = set.view.FindViewById(ResourceId);
			this.member = (PropertyInfo)expression.Member;
			this.Name = member.Name;
		}
	}

	public abstract class ValueConverter
	{
		public delegate object convertDel(object o);
		public static convertDel FromVisibility = x => ((Visibility)x) == Visibility.Visible ? Android.Views.ViewStates.Visible : Android.Views.ViewStates.Gone;
	}

	public class PropertyBindingGeneric<T> : PropertyBinding<T>
	{
		private PropertyInfo targetProperty;
		private ValueConverter.convertDel convert;
		public PropertyBindingGeneric(ViewBindingSet<T> set, int ResourceId,MemberExpression expression, string propertyName, ValueConverter.convertDel convert) : base(set, ResourceId, expression) 
		{ 
			Type t = View.GetType ();
			targetProperty = t.GetProperty (propertyName);
			this.convert = convert;
			//CheckBox.CheckedChange += (sender, e) => {ToVM(CheckBox.Checked);};
		}
		protected override void FromVM(object value) { 
			if (convert != null)
				value = convert (value);
			targetProperty.SetValue(View, value); }
	}

	public class PropertyBindingVisibility<T> : PropertyBinding<T>
	{
		public PropertyBindingVisibility(ViewBindingSet<T> set,int ResourceId,Expression<Func<T,Visibility>> property) : base(set, ResourceId, (MemberExpression)property.Body) {}
		protected override void FromVM(object value) { View.Visibility = ((Visibility)value) == Visibility.Visible ? Android.Views.ViewStates.Visible : Android.Views.ViewStates.Gone; }
	}

	public class PropertyBindingChecked<T> : PropertyBinding<T>
	{
		public CompoundButton CheckBox {get{return View as CompoundButton;}}
		public PropertyBindingChecked(ViewBindingSet<T> set, int ResourceId,Expression<Func<T,bool>> property) : base(set, ResourceId, (MemberExpression)property.Body) 
		{ 
			CheckBox.CheckedChange += (sender, e) => {ToVM(CheckBox.Checked);};
		}
		protected override void FromVM(object value) { CheckBox.Checked = (bool)value; }
	}

	public class PropertyBindingText<T> : PropertyBinding<T>
	{
		public TextView TextBox {get{return View as TextView;}}
		public PropertyBindingText(ViewBindingSet<T> set, int ResourceId,Expression<Func<T,string>> property) : base(set, ResourceId, (MemberExpression)property.Body) 
		{ 
			TextBox.TextChanged += (sender, e) => {ToVM(TextBox.Text);};
		}
		protected override void FromVM(object value) { TextBox.Text = (string)value; }
	}

	public class PropertyBindingDateTime<T> : PropertyBinding<T>
	{
		public TextView TextBox {get{return View as TextView;}}
		public PropertyBindingDateTime(ViewBindingSet<T> set, int ResourceId,Expression<Func<T,DateTime>> property) : base(set, ResourceId, (MemberExpression)property.Body) 
		{ 
			TextBox.TextChanged += (sender, e) => {
				DateTime time = DateTime.MinValue;
				if (DateTime.TryParse (TextBox.Text, out time))
					ToVM (time);
			};
		}
		protected override void FromVM(object value) { TextBox.Text = ((DateTime)value).ToShortTimeString(); }
	}

	public class PropertyBindingValue<T> : PropertyBinding<T>
	{
		public SeekBar Slider {get{return View as SeekBar;}}
		public PropertyBindingValue(ViewBindingSet<T> set, int ResourceId,Expression<Func<T,float>> property) : base(set, ResourceId, (MemberExpression)property.Body) 
		{ 
			Slider.ProgressChanged += (sender, e) => {ToVM(Slider.Progress);};
		}
		protected override void FromVM(object value) { Slider.Progress = (int)value; }
	}

	public class ViewBindingSet<T>
	{
		public View view;
		public List<PropertyBinding<T>> Bindings = new List<PropertyBinding<T>>();

		private VMBase vm = null;

		#region FromView
		private static Dictionary<View, ViewBindingSet<T>> All = new Dictionary<View, ViewBindingSet<T>> ();
		public static ViewBindingSet<T> FromView(View view, VMBase vm){
			ViewBindingSet<T> result = null;
			All.TryGetValue( view,out result);
			if (result == null) {
				result = new ViewBindingSet<T> (view, vm);
				All [view] = result;
			} else {
				result.VM = vm;
			}
			return result;
		}
		#endregion

		public ViewBindingSet(View view, VMBase vm)
		{
			this.view=view;
			this.VM=vm;
		}

		public VMBase VM {
			get { return vm; }
			set {
				if (vm != null)	vm.PropertyChanged -= HandlePropertyChanged;
				vm = value;
				if (vm != null) vm.PropertyChanged += HandlePropertyChanged;
				ApplyAll ();
			}
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e) { Apply (e.PropertyName); }
		void ApplyAll() {Apply (null);}
		void Apply(string propertyname)
		{
			foreach (PropertyBinding<T> binding in Bindings)
				if (propertyname == null || binding.Name==propertyname)
					binding.FromVM ();
		}

		private void Add(PropertyBinding<T> binding) {
			Bindings.Add (binding);
			binding.FromVM ();
		}

		public void Add(int ResourceId, Expression<Func<T,String>> property, string propertyName = null) { Add(new PropertyBindingText<T>(this,ResourceId,property)); }
		public void Add(int ResourceId, Expression<Func<T,DateTime>> property, string propertyName = null) { Add(new PropertyBindingDateTime<T>(this,ResourceId,property)); }
		//public void Add(int ResourceId, Expression<Func<T,Visibility>> property, string propertyName = null) { Add(new PropertyBindingVisibility<T>(this,ResourceId,property)); }
		public void Add(int ResourceId, Expression<Func<T,Visibility>> property, string propertyName = null) { Add(new PropertyBindingGeneric<T>(this,ResourceId, (MemberExpression)property.Body, "Visibility", ValueConverter.FromVisibility)); }
		public void Add(int ResourceId, Expression<Func<T,Boolean>> property, string propertyName = null) { Add(new PropertyBindingChecked<T>(this,ResourceId,property)); }
		public void Add(int ResourceId, Expression<Func<T,float>> property, string propertyName = null) { Add(new PropertyBindingValue<T>(this,ResourceId,property)); }
		//public void Add(int ResourceId, Expression<Func<T,int>> property, string propertyName = null) { Add(new PropertyBindingGeneric<T>(this,ResourceId,(MemberExpression)property.Body,propertyName)); }


	}
	#endif
}

