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
	[Register ("FDSelectItemVC")]
	partial class FDSelectItemVC
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIBarButtonItem leftBarBtn { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UISearchBar searchBar { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UISegmentedControl segmentControl { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView tableview { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView tableview1 { get; set; }

		[Action ("cancelBtnTapped:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void cancelBtnTapped (UIBarButtonItem sender);

		[Action ("segmentedControlValueChanged:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void segmentedControlValueChanged (UISegmentedControl sender);

		[Action ("selectItemBtnTapped:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void selectItemBtnTapped (UIBarButtonItem sender);

		void ReleaseDesignerOutlets ()
		{
			if (leftBarBtn != null) {
				leftBarBtn.Dispose ();
				leftBarBtn = null;
			}
			if (searchBar != null) {
				searchBar.Dispose ();
				searchBar = null;
			}
			if (segmentControl != null) {
				segmentControl.Dispose ();
				segmentControl = null;
			}
			if (tableview != null) {
				tableview.Dispose ();
				tableview = null;
			}
			if (tableview1 != null) {
				tableview1.Dispose ();
				tableview1 = null;
			}
		}
	}
}
