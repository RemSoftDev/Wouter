using System;

using Foundation;
using UIKit;

namespace FoodDiary2.iOS.Controller
{
	public partial class AddSettingsItemCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString ("AddSettingsItemCell");
		public static readonly UINib Nib;


		public UIButton buttonAdd { get { return addButton; } }

		static AddSettingsItemCell ()
		{
			Nib = UINib.FromName ("AddSettingsItemCell", NSBundle.MainBundle);
		}

		public AddSettingsItemCell (IntPtr handle) : base (handle)
		{
		}
	}
}
