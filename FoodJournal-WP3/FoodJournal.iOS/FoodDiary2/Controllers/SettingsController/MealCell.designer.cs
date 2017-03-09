// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace FoodDiary2
{
	[Register ("MealCell")]
	partial class MealCell
	{
		[Outlet]
		UIKit.UISwitch _switch { get; set; }

		[Outlet]
		UIKit.UILabel _title { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (_switch != null) {
				_switch.Dispose ();
				_switch = null;
			}

			if (_title != null) {
				_title.Dispose ();
				_title = null;
			}
		}
	}
}
