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
	[Register ("FDReportVC")]
	partial class FDReportVC
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblBreakfast { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblCalory { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblCarbs { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblDinner { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblLunch { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblSnack1 { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblSnack2 { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblSnack3 { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView viewBreakfast { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView viewDinner { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView viewFirstSnack { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView viewLunch { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView viewSecondSnack { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView viewThirdSnack { get; set; }

		[Action ("menuBtnTapped:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void menuBtnTapped (UIBarButtonItem sender);

		void ReleaseDesignerOutlets ()
		{
			if (lblBreakfast != null) {
				lblBreakfast.Dispose ();
				lblBreakfast = null;
			}
			if (lblCalory != null) {
				lblCalory.Dispose ();
				lblCalory = null;
			}
			if (lblCarbs != null) {
				lblCarbs.Dispose ();
				lblCarbs = null;
			}
			if (lblDinner != null) {
				lblDinner.Dispose ();
				lblDinner = null;
			}
			if (lblLunch != null) {
				lblLunch.Dispose ();
				lblLunch = null;
			}
			if (lblSnack1 != null) {
				lblSnack1.Dispose ();
				lblSnack1 = null;
			}
			if (lblSnack2 != null) {
				lblSnack2.Dispose ();
				lblSnack2 = null;
			}
			if (lblSnack3 != null) {
				lblSnack3.Dispose ();
				lblSnack3 = null;
			}
			if (viewBreakfast != null) {
				viewBreakfast.Dispose ();
				viewBreakfast = null;
			}
			if (viewDinner != null) {
				viewDinner.Dispose ();
				viewDinner = null;
			}
			if (viewFirstSnack != null) {
				viewFirstSnack.Dispose ();
				viewFirstSnack = null;
			}
			if (viewLunch != null) {
				viewLunch.Dispose ();
				viewLunch = null;
			}
			if (viewSecondSnack != null) {
				viewSecondSnack.Dispose ();
				viewSecondSnack = null;
			}
			if (viewThirdSnack != null) {
				viewThirdSnack.Dispose ();
				viewThirdSnack = null;
			}
		}
	}
}
