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
	[Register ("FDGoalsAddTargetVC")]
	partial class FDGoalsAddTargetVC
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIBarButtonItem leftBarBtn { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UINavigationBar navBar { get; set; }

		[Action ("cancelBtnTapped:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void cancelBtnTapped (UIBarButtonItem sender);

		void ReleaseDesignerOutlets ()
		{
			if (leftBarBtn != null) {
				leftBarBtn.Dispose ();
				leftBarBtn = null;
			}
			if (navBar != null) {
				navBar.Dispose ();
				navBar = null;
			}
		}
	}
}
