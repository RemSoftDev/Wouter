using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.View;
using Android.Support.V4.App;
using FoodJournal.Values;
using FoodJournal.Runtime;
using FoodJournal.Extensions;
using FoodJournal.WinPhone.Common.Resources;

namespace FoodJournal.Android15.Adapters
{

	public interface ISwipingPagerAdapter<T>
	{

		T GetItemFromId (int id);
		int GetIdFromItem (T item);

		int NextItemId (bool inc, int currentid);
		string GetPageSubTitle(int position);

	}

}