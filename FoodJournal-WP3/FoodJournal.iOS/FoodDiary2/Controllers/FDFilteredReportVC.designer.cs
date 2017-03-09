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
	[Register ("FDFilteredReportVC")]
	partial class FDFilteredReportVC
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblEmail { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblReport { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField txtFieldEmail { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (lblEmail != null) {
				lblEmail.Dispose ();
				lblEmail = null;
			}
			if (lblReport != null) {
				lblReport.Dispose ();
				lblReport = null;
			}
			if (txtFieldEmail != null) {
				txtFieldEmail.Dispose ();
				txtFieldEmail = null;
			}
		}
	}
}
