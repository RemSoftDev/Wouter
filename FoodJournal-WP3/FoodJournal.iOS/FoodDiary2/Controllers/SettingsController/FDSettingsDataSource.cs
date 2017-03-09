using System;
using UIKit;
using System.Collections.Generic;
using Foundation;
using FoodJournal.ViewModels;
using FoodJournal.ViewModels.Fragments;
using SWTableViewCells;
using FoodJournal.Resources;
using CoreGraphics;
using FoodJournal;
using System.Linq;

namespace FoodDiary2.iOS.Controller
{
	public class FDSettingsDataSource : UITableViewSource
	{
		private GoalsVM _gvm;
		private SettingsVM _vm;
		FoodDiary2.iOS.Controller.FDSettingsVC.SettingsCellDelegate _cellDelegate;
		public Action _addNutritionDelegate;
		public Action _addMealDelegate;
		public Action _showTotalDelegate;
		public Action _addReminderDelegate;

		private enum Sections {
			GOALS, MEALS, MEALS_OPTION, REMINDERS
		}

		public FDSettingsDataSource (GoalsVM gvm, SettingsVM vm, FoodDiary2.iOS.Controller.FDSettingsVC.SettingsCellDelegate cellDelegate)
		{
			_gvm = gvm;
			_vm = vm;
			_cellDelegate = cellDelegate;

		}



		public override nint RowsInSection (UITableView tableview, nint section)
		{
			switch (section) {
			case (long)Sections.GOALS:
				return _gvm.Goals.Count + 1;
			case (long)Sections.MEALS:
				return _vm.Meals.Count + 1;
			case (long)Sections.MEALS_OPTION:
				return 2;
			case (long)Sections.REMINDERS:
				return _vm.Reminders.Count + 1;
			}
			return 1;
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			return 4;
		}

		public override string TitleForHeader (UITableView tableView, nint section)
		{
			switch (section) {
			case (long)Sections.GOALS:
				return "Nutrition";
			case (long)Sections.MEALS:
				return "Meals";
			case (long)Sections.MEALS_OPTION:
				return "Meal Options";
			case (long)Sections.REMINDERS:
				return "Reminders";
			}	

			return "";
		}


		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			if (indexPath.Section == (long)Sections.GOALS) {
				if (indexPath.Row < _gvm.Goals.Count ) { 
					var cell = tableView.DequeueReusableCell ("NutritionCell", indexPath) as NutritionCell;
					cell.SetTitle (_gvm.Goals[indexPath.Row].Text);
					cell.Delegate = _cellDelegate;
					cell.SetRightUtilityButtons (RightButtons (), 100.0f);

					return cell;
				} else {
					var cell = tableView.DequeueReusableCell ("addSettingsCell", indexPath) as AddSettingsItemCell;

					cell.buttonAdd.SetTitle (AppResources.AddNutrition, UIControlState.Normal);
					cell.buttonAdd.Frame = new CoreGraphics.CGRect (cell.buttonAdd.Frame.X, cell.buttonAdd.Frame.Y, 150, cell.buttonAdd.Frame.Height);
					cell.buttonAdd.SizeToFit ();
					if (cell.buttonAdd.Tag != 1) {
						cell.buttonAdd.TouchUpInside += (object sender, EventArgs e) => {
							//var controller = new AddNutritionController();
							_addNutritionDelegate ();
						};
					}
					cell.buttonAdd.Tag = 1;
					return cell;
				}
			} else if (indexPath.Section == (long)Sections.MEALS) {
				if (indexPath.Row < _vm.Meals.Count ) { 
					var cell = tableView.DequeueReusableCell ("MealCell", indexPath) as MealCell;

					MealSettingsVM vm = _vm.Meals [indexPath.Row];
					var binding = DataContext<MealSettingsVM>.FromView (cell);
					binding.VM = vm;

					binding.Add(cell.title, x => x.Text);
					binding.Add(cell.swich, x => x.MealSelected);

					return cell;
				} else {
					var cell = tableView.DequeueReusableCell ("addSettingsCell", indexPath) as AddSettingsItemCell;

					cell.buttonAdd.SetTitle (AppResources.AddMeal, UIControlState.Normal);
					cell.buttonAdd.Frame = new CoreGraphics.CGRect (cell.buttonAdd.Frame.X, cell.buttonAdd.Frame.Y, 150, cell.buttonAdd.Frame.Height);
					cell.buttonAdd.SizeToFit ();
					if (cell.buttonAdd.Tag != 1) {
						cell.buttonAdd.TouchUpInside += (object sender, EventArgs e) => {
							//var controller = new AddNutritionController();
							_addMealDelegate ();
						};
					}
					cell.buttonAdd.Tag = 1;
					return cell;
				}
			} else if (indexPath.Section == (long)Sections.MEALS_OPTION) {
				if (indexPath.Row == 0) { 
					var cell = tableView.DequeueReusableCell ("mealShowCell", indexPath) as MealShowCell;

					var binding = DataContext<SettingsVM>.FromView (cell);
					binding.VM = _vm;

					//binding.Add(cell.NutritionTitle, x => x.ShowTotal);
					cell.NutritionTitle.Text = NSBundle.MainBundle.LocalizedString ("Show a nutrition target", null); 

					return cell;

				} else {
					var cell = tableView.DequeueReusableCell ("MealCell", indexPath) as MealCell;
					cell.title.Text = AppResources.EnterMealTime;

					var binding = DataContext<SettingsVM>.FromView (cell);
					binding.VM = _vm;

					if (UserSettings.Current.SelectedProperties.Any())
					{
						cell.swich.On = true;
					}
					binding.Add(cell.swich, x => x.ShowTotal);
					//cell.Delegate = _cellDelegate;
					return cell;
				}
			} else if (indexPath.Section == (long)Sections.REMINDERS) {
				if (indexPath.Row < _vm.Reminders.Count ) { 
					var cell = tableView.DequeueReusableCell ("NutritionCell", indexPath) as NutritionCell;
					cell.swich.Hidden = false;
					cell.Delegate = _cellDelegate;

					ReminderVM vm = _vm.Reminders [indexPath.Row];
					var binding = DataContext<ReminderVM>.FromView (cell);
					binding.VM = vm;
					binding.Add(cell.title, x => x.Time);
					binding.Add(cell.swich, x => x.Checked);

					return cell;
				} else {
					var cell = tableView.DequeueReusableCell ("addSettingsCell", indexPath)  as AddSettingsItemCell;
					cell.buttonAdd.SetTitle (AppResources.AddReminder, UIControlState.Normal);
					cell.buttonAdd.Frame = new CoreGraphics.CGRect (cell.buttonAdd.Frame.X, cell.buttonAdd.Frame.Y, 150, cell.buttonAdd.Frame.Height);
					cell.buttonAdd.SizeToFit ();
					if (cell.buttonAdd.Tag != 1) {
						cell.buttonAdd.TouchUpInside += (object sender, EventArgs e) => {
							//var controller = new AddNutritionController();
							_addMealDelegate ();
						};
					}
					cell.buttonAdd.Tag = 1;
					return cell;
					return cell;
				}
			}
			return null;	
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath){
			if (indexPath.Section == (long)Sections.MEALS_OPTION) {
				_showTotalDelegate ();
			} else if (indexPath.Section == (long)Sections.REMINDERS) {
				_addReminderDelegate ();
			}

		}


		static UIButton[] RightButtons ()
		{
			NSMutableArray rightUtilityButtons = new NSMutableArray ();
			//				rightUtilityButtons.AddUtilityButton (UIColor.FromRGBA (1.0f, 0.231f, 0.188f, 1.0f), "Удалить");
			rightUtilityButtons.AddUtilityButton (UIColor.FromRGBA (1.0f, 0.231f, 0.188f, 1.0f), "Delete" );
			return NSArray.FromArray<UIButton> (rightUtilityButtons);
		}
	}


}

