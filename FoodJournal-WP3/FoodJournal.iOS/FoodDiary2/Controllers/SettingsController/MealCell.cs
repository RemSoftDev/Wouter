using System;

using Foundation;
using UIKit;
using FoodDiary2.iOS;
using FoodJournal.ViewModels;

namespace FoodDiary2
{
	public partial class MealCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString ("MealCell");
		public static readonly UINib Nib;

		static MealCell ()
		{
			Nib = UINib.FromName ("MealCell", NSBundle.MainBundle);
		}

		public MealCell (IntPtr handle) : base (handle)
		{
		}

		public UISwitch swich { get { return _switch; } }
		public UILabel title { get { return _title; } }



		public void BindProperty(DataContext<SettingsVM> binding, int tag){
			_switch.Tag = tag;
			binding.Add (swich, x => x.ShowTotal, "ValueChanged");
			binding.Add (swich,  (a) => { 
				Console.Write("test");
			}, "ValueChanged");
		}
	}
}
