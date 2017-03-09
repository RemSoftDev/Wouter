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

namespace FoodDiary.iOS
{
	[Register ("FDNewItemVC")]
	partial class FDNewItemVC
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIBarButtonItem barBtnDone { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblItemName { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblNutrition { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lblServing { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIScrollView scrlView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView tableview1 { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView tableview2 { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView tableview3 { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField txtFieldItemName { get; set; }

		[Action ("doneBtnTapped:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void doneBtnTapped (UIBarButtonItem sender);

		void ReleaseDesignerOutlets ()
		{
			if (barBtnDone != null) {
				barBtnDone.Dispose ();
				barBtnDone = null;
			}
			if (lblItemName != null) {
				lblItemName.Dispose ();
				lblItemName = null;
			}
			if (lblNutrition != null) {
				lblNutrition.Dispose ();
				lblNutrition = null;
			}
			if (lblServing != null) {
				lblServing.Dispose ();
				lblServing = null;
			}
			if (scrlView != null) {
				scrlView.Dispose ();
				scrlView = null;
			}
			if (tableview1 != null) {
				tableview1.Dispose ();
				tableview1 = null;
			}
			if (tableview2 != null) {
				tableview2.Dispose ();
				tableview2 = null;
			}
			if (tableview3 != null) {
				tableview3.Dispose ();
				tableview3 = null;
			}
			if (txtFieldItemName != null) {
				txtFieldItemName.Dispose ();
				txtFieldItemName = null;
			}
		}
	}
}
