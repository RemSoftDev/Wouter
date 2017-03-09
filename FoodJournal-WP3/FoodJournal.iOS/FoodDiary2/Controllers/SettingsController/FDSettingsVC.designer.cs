// WARNING
//
// This file has been generated automatically by Xamarin Studio Community to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace FoodDiary2.iOS.Controller
{
	[Register ("FDSettingsVC")]
	partial class FDSettingsVC
	{
		[Outlet]
		UIKit.NSLayoutConstraint constraintTop { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintTopOfPickerView { get; set; }

		[Outlet]
		UIKit.UILabel lblChooseInfo { get; set; }

		[Outlet]
		UIKit.UILabel lblMeals { get; set; }

		[Outlet]
		UIKit.UILabel lblMealTime { get; set; }

		[Outlet]
		UIKit.UILabel lblNotes { get; set; }

		[Outlet]
		UIKit.UILabel lblReminderSubTitle { get; set; }

		[Outlet]
		UIKit.UILabel lblReminderTitle { get; set; }

		[Outlet]
		UIKit.UILabel lblShowTotal { get; set; }

		[Outlet]
		UIKit.UILabel lblUnitMeasure { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem leftBarBtn { get; set; }

		[Outlet]
		UIKit.UIPickerView pickerViewShowTotal { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem rightBarBtn { get; set; }

		[Outlet]
		UIKit.UIScrollView scrlView { get; set; }

		[Outlet]
		UIKit.UITableView tableView { get; set; }

		[Outlet]
		UIKit.UITableView tableview1 { get; set; }

		[Outlet]
		UIKit.UITableView tableview2 { get; set; }

		[Outlet]
		UIKit.UITableView tableviewMeals { get; set; }

		[Outlet]
		UIKit.UITableView tableviewReminders { get; set; }

		[Action ("btnDoneTimerPickerTapped:")]
		partial void btnDoneTimerPickerTapped (UIKit.UIButton sender);

		[Action ("cancelShowTotalBtnTapped:")]
		partial void cancelShowTotalBtnTapped (UIKit.UIButton sender);

		[Action ("doneBtnTapped:")]
		partial void doneBtnTapped (UIKit.UIBarButtonItem sender);

		[Action ("menuBtnTapped:")]
		partial void menuBtnTapped (UIKit.UIBarButtonItem sender);

		[Action ("showTotalBtnTapped:")]
		partial void showTotalBtnTapped (UIKit.UIButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (constraintTop != null) {
				constraintTop.Dispose ();
				constraintTop = null;
			}

			if (constraintTopOfPickerView != null) {
				constraintTopOfPickerView.Dispose ();
				constraintTopOfPickerView = null;
			}

			if (lblChooseInfo != null) {
				lblChooseInfo.Dispose ();
				lblChooseInfo = null;
			}

			if (lblMeals != null) {
				lblMeals.Dispose ();
				lblMeals = null;
			}

			if (lblMealTime != null) {
				lblMealTime.Dispose ();
				lblMealTime = null;
			}

			if (lblNotes != null) {
				lblNotes.Dispose ();
				lblNotes = null;
			}

			if (lblReminderSubTitle != null) {
				lblReminderSubTitle.Dispose ();
				lblReminderSubTitle = null;
			}

			if (lblReminderTitle != null) {
				lblReminderTitle.Dispose ();
				lblReminderTitle = null;
			}

			if (lblShowTotal != null) {
				lblShowTotal.Dispose ();
				lblShowTotal = null;
			}

			if (lblUnitMeasure != null) {
				lblUnitMeasure.Dispose ();
				lblUnitMeasure = null;
			}

			if (leftBarBtn != null) {
				leftBarBtn.Dispose ();
				leftBarBtn = null;
			}

			if (pickerViewShowTotal != null) {
				pickerViewShowTotal.Dispose ();
				pickerViewShowTotal = null;
			}

			if (rightBarBtn != null) {
				rightBarBtn.Dispose ();
				rightBarBtn = null;
			}

			if (scrlView != null) {
				scrlView.Dispose ();
				scrlView = null;
			}

			if (tableView != null) {
				tableView.Dispose ();
				tableView = null;
			}

			if (tableview1 != null) {
				tableview1.Dispose ();
				tableview1 = null;
			}

			if (tableview2 != null) {
				tableview2.Dispose ();
				tableview2 = null;
			}

			if (tableviewMeals != null) {
				tableviewMeals.Dispose ();
				tableviewMeals = null;
			}

			if (tableviewReminders != null) {
				tableviewReminders.Dispose ();
				tableviewReminders = null;
			}
		}
	}
}
