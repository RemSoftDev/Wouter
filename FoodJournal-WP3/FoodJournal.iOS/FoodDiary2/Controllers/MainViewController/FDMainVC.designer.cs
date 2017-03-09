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
	[Register ("FDMainVC")]
	partial class FDMainVC
	{
		[Outlet]
		UIKit.NSLayoutConstraint constraintHeight { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintHeightTxtViewNotes { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintTop { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint constraintTopToolbar { get; set; }

		[Outlet]
		UIKit.UILabel lblCalTime { get; set; }

		[Outlet]
		UIKit.UILabel lblPlaceholder { get; set; }

		[Outlet]
		UIKit.UILabel lblTitleNote { get; set; }

		[Outlet]
		UIKit.UILabel lblTitleTotalCal { get; set; }

		[Outlet]
		UIKit.UILabel lblTotalCal { get; set; }

		[Outlet]
		UIKit.UIScrollView scrlView { get; set; }

		[Outlet]
		UIKit.UITabBar tabbar { get; set; }

		[Outlet]
		UIKit.UITableView tableview { get; set; }

		[Outlet]
		UIKit.UITextView txtViewNotes { get; set; }

		[Action ("btnDoneTapped:")]
		partial void btnDoneTapped (UIKit.UIButton sender);

		[Action ("btnTimePickerTapped:")]
		partial void btnTimePickerTapped (UIKit.UIButton sender);

		[Action ("handleSingleTap:")]
		partial void handleSingleTap (UIKit.UIBarButtonItem sender);

		[Action ("menuBtnTapped:")]
		partial void menuBtnTapped (UIKit.UIBarButtonItem sender);

		[Action ("selectItemBtnTapped:")]
		partial void selectItemBtnTapped (UIKit.UIBarButtonItem sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (constraintHeight != null) {
				constraintHeight.Dispose ();
				constraintHeight = null;
			}

			if (constraintHeightTxtViewNotes != null) {
				constraintHeightTxtViewNotes.Dispose ();
				constraintHeightTxtViewNotes = null;
			}

			if (constraintTop != null) {
				constraintTop.Dispose ();
				constraintTop = null;
			}

			if (constraintTopToolbar != null) {
				constraintTopToolbar.Dispose ();
				constraintTopToolbar = null;
			}

			if (lblCalTime != null) {
				lblCalTime.Dispose ();
				lblCalTime = null;
			}

			if (lblPlaceholder != null) {
				lblPlaceholder.Dispose ();
				lblPlaceholder = null;
			}

			if (lblTitleNote != null) {
				lblTitleNote.Dispose ();
				lblTitleNote = null;
			}

			if (lblTitleTotalCal != null) {
				lblTitleTotalCal.Dispose ();
				lblTitleTotalCal = null;
			}

			if (lblTotalCal != null) {
				lblTotalCal.Dispose ();
				lblTotalCal = null;
			}

			if (scrlView != null) {
				scrlView.Dispose ();
				scrlView = null;
			}

			if (tabbar != null) {
				tabbar.Dispose ();
				tabbar = null;
			}

			if (tableview != null) {
				tableview.Dispose ();
				tableview = null;
			}

			if (txtViewNotes != null) {
				txtViewNotes.Dispose ();
				txtViewNotes = null;
			}
		}
	}
}
