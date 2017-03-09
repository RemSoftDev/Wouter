// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace FoodDiary2.iOS.Controller
{
	[Register ("FDMenuVC")]
	partial class FDMenuVC
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView tableview1 { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView tableview2 { get; set; }

		[Action ("btnTodayTapped:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void btnTodayTapped (UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (tableview1 != null) {
				tableview1.Dispose ();
				tableview1 = null;
			}
			if (tableview2 != null) {
				tableview2.Dispose ();
				tableview2 = null;
			}
		}
	}
}
