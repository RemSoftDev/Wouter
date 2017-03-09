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
	[Register ("NutritionCell")]
	partial class NutritionCell
	{
		[Outlet]
		UIKit.UISwitch _swich { get; set; }

		[Outlet]
		UIKit.UILabel _titleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (_titleLabel != null) {
				_titleLabel.Dispose ();
				_titleLabel = null;
			}

			if (_swich != null) {
				_swich.Dispose ();
				_swich = null;
			}
		}
	}
}
