using System;

using Foundation;
using UIKit;
using FoodDiary2.iOS;
using FoodJournal.ViewModels;
using SWTableViewCells;

namespace FoodDiary2
{
	public partial class NutritionCell : SWTableViewCell
	{
		public static readonly NSString Key = new NSString ("NutritionCell");
		public static readonly UINib Nib;

		static NutritionCell ()
		{
			Nib = UINib.FromName ("NutritionCell", NSBundle.MainBundle);
		}

		public NutritionCell (IntPtr handle) : base (handle)
		{
		}


		public UISwitch swich { get { return _swich; } }
		public UILabel title { get { return _titleLabel; } }

		public void SetTitle(String title) {
			_titleLabel.Text = title;
		}


	}
}
