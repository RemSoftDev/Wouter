using System;

using UIKit;

namespace FoodDiary2.iOS.Controller
{
	public partial class SlideBarController : UIViewController
	{
		public SlideBarController (IntPtr handle) : base (handle)
		{
		}


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.
		
			_tableView.Source = new SideBarDataSource ();
			_tableView.ReloadData ();
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}


