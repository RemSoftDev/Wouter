using System;

using UIKit;
using FoodJournal.Resources;
using Foundation;
using FoodJournal.ViewModels;
using System.Collections.Generic;
using FoodJournal.Values;

namespace FoodDiary2.iOS.Controller
{
	public partial class AddNutritionController : UIViewController
	{
		public GoalsVM _gvm { get; set;}

		public AddNutritionController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.

			NavigationItem.Title = AppResources.AddNutrition;

			tableView.Source = new AddNutritionDataSource (_gvm.NewPropertyOptions, (s, clicked) => { 
				Property result = StandardProperty.none;

				foreach (var value in Property.All())
					if (value.FullCapitalizedText == clicked)
						result = value;
				if (result == StandardProperty.none) return;

				_gvm.AddGoal(result);
				NavigationController.PopViewController(true);
			} );
			tableView.ReloadData ();
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
	}

	public class AddNutritionDataSource : UITableViewSource
	{
		
		IList<string> _nutritionItems;
	    event EventHandler<string> _valueChanged;


		public AddNutritionDataSource (List<string> nutritionItems, EventHandler<string> valueChanged)
		{
			_nutritionItems = nutritionItems;
			_valueChanged = valueChanged;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return _nutritionItems.Count;
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			_valueChanged (this, _nutritionItems [indexPath.Row]);	
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell("TableCell", indexPath);
			cell.TextLabel.Text = _nutritionItems [indexPath.Row]; 
			return cell;
				
		}

	}
}


