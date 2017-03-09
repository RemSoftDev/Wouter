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
	[Register ("FDJournalVC")]
	partial class FDJournalVC
	{
		[Outlet]
		UIKit.UITableView _tblView { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem leftBarBtn { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem rightBarBtn { get; set; }

		[Outlet]
		UIKit.UISegmentedControl segmentCntrl { get; set; }

		[Action ("filteredReportBtnTapped:")]
		partial void filteredReportBtnTapped (UIKit.UIBarButtonItem sender);

		[Action ("menuBtnTapped:")]
		partial void menuBtnTapped (UIKit.UIBarButtonItem sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (_tblView != null) {
				_tblView.Dispose ();
				_tblView = null;
			}

			if (leftBarBtn != null) {
				leftBarBtn.Dispose ();
				leftBarBtn = null;
			}

			if (rightBarBtn != null) {
				rightBarBtn.Dispose ();
				rightBarBtn = null;
			}

			if (segmentCntrl != null) {
				segmentCntrl.Dispose ();
				segmentCntrl = null;
			}
		}
	}
}
