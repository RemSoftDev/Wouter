
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;

namespace FoodJournal.Android15.Activities
{
	[Activity (Theme = "@style/AppTheme")]
	public class Localization : StandardActivity
	{

		public Localization(): base(true) {}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.Localization);
		}

		protected override List<MenuLink> GetMenuItems ()
		{
			return null;
		}
	}
}

