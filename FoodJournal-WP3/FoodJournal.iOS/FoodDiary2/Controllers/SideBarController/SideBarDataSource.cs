using System;
using System.Collections.Generic;
using UIKit;
using Foundation;
using CoreGraphics;

namespace FoodDiary2.iOS.Controller
{
	public class SideBarDataSource : UITableViewSource
	{
		List<List<string>> Titles = new List<List<string>>  { 
			new List<string> {"Today", "Journal", "Report", "Goals"},
			new List<string> {"Settings", "About_And_Info"},	
		};

		List<List<string>> Icons  = new List<List<string>>  { 
			new List<string> {"today", "journal", "report", "goals"},
			new List<string> {"settings", "about"},	
		};

		public override nint NumberOfSections (UITableView tableView)
		{
			return Titles.Count;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return Titles[(int)section].Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("cellMenu", indexPath);
			cell.TextLabel.Text = NSBundle.MainBundle.LocalizedString (Titles[(int)indexPath.Section][(int)indexPath.Row], null); 
			cell.ImageView.Image = UIImage.FromBundle (Icons[(int)indexPath.Section][(int)indexPath.Row]);
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			return cell;	
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			if (Titles [(int)indexPath.Section] [(int)indexPath.Row].Equals ("Today")) {
				//AppDelegate.NavigationController.
				var navigationController = new UINavigationController (AppDelegate.Storyboard.InstantiateViewController ("FDMainVC"));
				navigationController.NavigationBar.Translucent = false;
				AppDelegate.SidebarController.ChangeContentView(navigationController);
			} else if (Titles [(int)indexPath.Section] [(int)indexPath.Row].Equals ("Journal")){
				var navigationController = new UINavigationController (AppDelegate.Storyboard.InstantiateViewController ("FDJournalVC"));
				navigationController.NavigationBar.Translucent = false;
				AppDelegate.SidebarController.ChangeContentView(navigationController);
			} else if (Titles [(int)indexPath.Section] [(int)indexPath.Row].Equals ("Report")){
				var navigationController = new UINavigationController (AppDelegate.Storyboard.InstantiateViewController ("FDReportVC"));
				navigationController.NavigationBar.Translucent = false;
				AppDelegate.SidebarController.ChangeContentView(navigationController);
			} else if (Titles [(int)indexPath.Section] [(int)indexPath.Row].Equals ("Goals")){
				var navigationController = new UINavigationController (AppDelegate.Storyboard.InstantiateViewController ("FDGoalsVC"));
				navigationController.NavigationBar.Translucent = false;
				AppDelegate.SidebarController.ChangeContentView(navigationController);
			} else if (Titles [(int)indexPath.Section] [(int)indexPath.Row].Equals ("Settings")){
				var navigationController = new UINavigationController (AppDelegate.Storyboard.InstantiateViewController ("FDSettingsVC"));
				navigationController.NavigationBar.Translucent = false;
				AppDelegate.SidebarController.ChangeContentView(navigationController);
			} else if (Titles [(int)indexPath.Section] [(int)indexPath.Row].Equals ("About_And_Info")){
				var navigationController = new UINavigationController (AppDelegate.Storyboard.InstantiateViewController ("FDAboutVC"));
				navigationController.NavigationBar.Translucent = false;
				AppDelegate.SidebarController.ChangeContentView(navigationController);
			}
		}

		public override UIView GetViewForHeader (UITableView tableView, nint section)
		{
			var view = new UIView (new CGRect (0,0, 320, 64f));
			view.BackgroundColor = UIColor.FromRGB (210.0f / 255.0f, 210.0f / 255.0f, 210.0f / 255.0f);
			return 	view;
		}

		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return 44f;
		}
	}
}

