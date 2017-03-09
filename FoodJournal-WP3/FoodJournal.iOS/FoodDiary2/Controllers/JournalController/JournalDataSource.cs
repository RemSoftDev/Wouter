using System;
using UIKit;
using System.Collections.Generic;
using Foundation;
using System.Collections;

namespace FoodDiary2.iOS.Controller
{
	public class JournalDataSource: UITableViewSource
	{

		List<Dictionary<string,string>> Friday = new List<Dictionary<string,string>> {
			new Dictionary<string, string> () {
				{"title","Friday"}, {"date","Sep 15"}, {"value1","2500"}, {"value2","^300"}
			},
			new Dictionary<string, string> () {
				{"title","Breakfast"}, {"time","10:00 AM"}, {"value","354"}, {"desc","Good, fast breakfast"},
				{"quantDesc1","1 cup Coffee with milk"}, {"quantDict1Val","6"},
				{"quantDesc2","1 piece croissant with apple and plum jam"}, {"quantDict2Val","231"},
				{"quantDesc3","250 ml Apple juice"}, {"quantDict3Val","117"},
			},
			new Dictionary<string, string> () {	
				{"title","Lunch"}, {"time","10:00 AM"}, {"value","919"}, {"desc",""},
				{"quantDesc1","1 cup Coffee"}, {"quantDict1Val","2"},
				{"quantDesc2","1 piece Soup"}, {"quantDict2Val","800"}
			},
			new Dictionary<string, string> () {
				{"title","Dinner"}, {"time","10:00 AM"}, {"value","500"}, {"desc",""},
				{"quantDesc1","1 plate Vegetable salad"}, {"quantDict1Val","2"}
			},
		};	

		List<Dictionary<string,string>> Thursday = new List<Dictionary<string,string>> {
			new Dictionary<string, string> () {
				{"title", "Thursday"}, {"date", "Sep 14"}, {"value1", "2500"}, {"value2", "^300"}
			},
			new Dictionary<string, string> () {
				{"title", "Breakfast"}, {"time", "10:00 AM"}, {"value", "354"}, {"desc", "Good, fast breakfast"},
				{"quantDesc1","1 cup Coffee with milk"}, {"quantDict1Val","6"},
				{"quantDesc2","1 piece croissant with apple and plum jam"}, {"quantDict2Val","231"},
				{"quantDesc3","250 ml Apple juice"}, {"quantDict3Val","117"}
			},
			new Dictionary<string, string> () {	
				{"title","Lunch"}, {"time","10:00 AM"}, {"value","919"}, {"desc",""},
				{"quantDesc1","1 cup Coffee"}, {"quantDict1Val","2"},
				{"quantDesc2","1 piece Soup"}, {"quantDict2Val","800"}
			},
			new Dictionary<string, string> () {
				{"title","Dinner"}, {"time","10:00 AM"}, {"value","500"}, {"desc",""},
				{"quantDesc1","1 plate Vegetable salad"}, {"quantDict1Val","2"}
			},
		};	

		List<IList> Journals;

		public JournalDataSource(){
			Journals = new List<IList> (){Friday,Thursday};
			//Journals.Add (Friday);
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			return (nint)Journals.Count;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return 1;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("cellJournal", indexPath);
			return cell;	
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{

		}

		public override nfloat GetHeightForHeader (UITableView tableView, nint section)
		{
			return 70;
		}

		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return 450;	
		}

		public override UIView GetViewForHeader (UITableView tableView, nint section)
		{
			var array = NSBundle.MainBundle.LoadNib ("FDTableViewHeader", this, null);	
			UIView view = array.GetItem<UIView>((nuint)0) as UIView;

			return view;
		}
		
	}
}

