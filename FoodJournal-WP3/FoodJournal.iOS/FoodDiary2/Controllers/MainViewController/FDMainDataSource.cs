using System;
using UIKit;
using System.Collections.Generic;
using Foundation;

namespace FoodDiary2.iOS.Controller
{
	public class FDMainDataSource : UITableViewSource
	{

		List<Dictionary<string,string>> Products = new List<Dictionary<string,string>> {
			new Dictionary<string, string> () {
				{ "itemTitle" , "Coffee with milk" },
				{ "quantity" , "1 small cup" },
				{ "itemDetails" , "Calories - 235, Fat - 10.15, Carbs - 13.84, Protein - 12.33" }
			},
			new Dictionary<string, string> () {
				{ "itemTitle" , "Croissant with apple and plum jam" },
				{ "quantity" , "1 piece" },
				{ "itemDetails" , "Calories - 231, Fat - 11.65, Carbs - 26.84, Protein - 15.33" }
			},
			new Dictionary<string, string> () {	
				{ "itemTitle" , "Apple juice" },
				{ "quantity" , "250 ml" },
				{ "itemDetails" , "Calories - 211, Fat - 12.15, Carbs - 14.84, Protein - 23.33" }
			},
			new Dictionary<string, string> () {
				{ "itemTitle" , "Yogurt" },
				{ "quantity" , "150 ml" },
				{ "itemDetails" , "Calories - 121, Fat - 13.15, Carbs - 16.84, Protein - 11.33" }
			},
		};	



		public override nint NumberOfSections (UITableView tableView)
		{
			return (nint)Products.Count;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return (nint)Products[(int)section].Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("cellToday", indexPath);
			return cell;	
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{

		}



	}
}

