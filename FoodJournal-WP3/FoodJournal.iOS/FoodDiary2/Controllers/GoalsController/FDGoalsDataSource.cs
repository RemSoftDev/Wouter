using System;
using System.Collections.Generic;
using UIKit;
using Foundation;

namespace FoodDiary2.iOS.Controller
{
	public class FDGoalsDataSource : UITableViewSource
	{

//		var dictTarget1: NSDictionary = ["item":"Daily_Number", "quantity":""]
//			var dictTarget2: NSDictionary = ["item":"Calories", "quantity":"2500"]
//			var dictTarget3: NSDictionary = ["item":"Fat", "quantity":"300"]
		List<Dictionary<string,string>> Goals = new List<Dictionary<string,string>> {
			new Dictionary<string, string> () {{ "item", "Daily_Number" }, { "quantity", "" }},
			new Dictionary<string, string> (){ { "item", "Calories" }, { "quantity", "2500" }},
			new Dictionary<string, string> () {{ "item", "Fat" }, { "quantity", "300" }},
		};

		public override nint NumberOfSections (UITableView tableView)
		{
			return (nint)Goals.Count;
		}

		public override nint RowsInSection (UITableView tableview, nint section){
			return (nint)Goals[(int)section].Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("cellGoals", indexPath);
			return cell;	
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{

		}

	}
}

