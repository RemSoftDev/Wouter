// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace FoodDiary2.iOS.Controller
{
	[Register ("FDGoalsVC")]
	partial class FDGoalsVC
	{
		[Outlet]
		UIKit.NSLayoutConstraint constraintHeightScrlView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintTopDatePicker { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintTopOfPickerView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintTopOfToolbar { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintViewHeight { get; set; }

		[Outlet]
		UIKit.UILabel lblDailyTarget { get; set; }

		[Outlet]
		UIKit.UILabel lblDescGoalTitle { get; set; }

		[Outlet]
		UIKit.UILabel lblTargetDate { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem leftBarBtn { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem rightBarBtn { get; set; }

		[Outlet]
		UIKit.UIScrollView scrlView { get; set; }

		[Outlet]
		UIKit.UITableView tableView { get; set; }

		[Outlet]
		UIKit.UILabel txtPlaceholder { get; set; }

		[Outlet]
		UIKit.UITextView txtViewDesc { get; set; }

		[Action ("btnDoneTapped:")]
		partial void btnDoneTapped (UIKit.UIButton sender);

		[Action ("btnDoneToolbarTapped:")]
		partial void btnDoneToolbarTapped (UIKit.UIBarButtonItem sender);

		[Action ("btnShowDatePickerTapped:")]
		partial void btnShowDatePickerTapped (UIKit.UIButton sender);

		[Action ("cancelAddTargetBtnTapped:")]
		partial void cancelAddTargetBtnTapped (UIKit.UIButton sender);

		[Action ("menuBtnTapped:")]
		partial void menuBtnTapped (UIKit.UIBarButtonItem sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (tableView != null) {
				tableView.Dispose ();
				tableView = null;
			}

			if (constraintHeightScrlView != null) {
				constraintHeightScrlView.Dispose ();
				constraintHeightScrlView = null;
			}

			if (constraintTopDatePicker != null) {
				constraintTopDatePicker.Dispose ();
				constraintTopDatePicker = null;
			}

			if (constraintTopOfPickerView != null) {
				constraintTopOfPickerView.Dispose ();
				constraintTopOfPickerView = null;
			}

			if (constraintTopOfToolbar != null) {
				constraintTopOfToolbar.Dispose ();
				constraintTopOfToolbar = null;
			}

			if (constraintViewHeight != null) {
				constraintViewHeight.Dispose ();
				constraintViewHeight = null;
			}

			if (lblDailyTarget != null) {
				lblDailyTarget.Dispose ();
				lblDailyTarget = null;
			}

			if (lblDescGoalTitle != null) {
				lblDescGoalTitle.Dispose ();
				lblDescGoalTitle = null;
			}

			if (lblTargetDate != null) {
				lblTargetDate.Dispose ();
				lblTargetDate = null;
			}

			if (leftBarBtn != null) {
				leftBarBtn.Dispose ();
				leftBarBtn = null;
			}

			if (rightBarBtn != null) {
				rightBarBtn.Dispose ();
				rightBarBtn = null;
			}

			if (scrlView != null) {
				scrlView.Dispose ();
				scrlView = null;
			}

			if (txtPlaceholder != null) {
				txtPlaceholder.Dispose ();
				txtPlaceholder = null;
			}

			if (txtViewDesc != null) {
				txtViewDesc.Dispose ();
				txtViewDesc = null;
			}
		}
	}
}
