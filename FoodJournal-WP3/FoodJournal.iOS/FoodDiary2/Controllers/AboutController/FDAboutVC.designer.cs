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
	[Register ("FDAboutVC")]
	partial class FDAboutVC
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblAbout { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblPremium { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIBarButtonItem leftBarBtn { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView viewLogo { get; set; }

		[Action ("menuBtnTapped:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void menuBtnTapped (UIBarButtonItem sender);

		void ReleaseDesignerOutlets ()
		{
			if (lblAbout != null) {
				lblAbout.Dispose ();
				lblAbout = null;
			}
			if (lblPremium != null) {
				lblPremium.Dispose ();
				lblPremium = null;
			}
			if (leftBarBtn != null) {
				leftBarBtn.Dispose ();
				leftBarBtn = null;
			}
			if (viewLogo != null) {
				viewLogo.Dispose ();
				viewLogo = null;
			}
		}
	}
}
