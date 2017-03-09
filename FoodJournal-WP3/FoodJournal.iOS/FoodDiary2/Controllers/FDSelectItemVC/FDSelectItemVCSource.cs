using System;
using UIKit;
using System.Collections.Generic;
using Foundation;
using FoodJournal.ViewModels;
using System.Collections.ObjectModel;
using FoodDiary2.ViewModels;

namespace FoodDiary2.iOS.Controller
{
	public class FDSelectItemVCSource : UITableViewSource
	{

//		var arrayItemForDict: NSArray = ["Croissant", "Yogurt", "Black Coffee", "Unsweetened", "Omelette", "Sandwich with chicken"]
//			var dictionary: NSDictionary = ["items":arrayItemForDict,
//				"title":"Recent Items"]
//
//			var arrayItemForDict1: NSArray = ["Apple", "Apple chips", "Apple jam", "Apple pancake","Apple chips", "Apple jam"]
//			var dictionary1: NSDictionary = ["items":arrayItemForDict1,
//				"title":"Common Items"]

		private static Dictionary<string, List<string>> Products1 = new Dictionary<string, List<string>>() {
			{ "items", new List<string>() {"Croissant", "Yogurt", "Black Coffee", "Unsweetened", "Omelette", "Sandwich with chicken"} },

		};
		private static Dictionary<string, List<string>> Products2 = new Dictionary<string, List<string>>() {
			{ "items", new List<string>() {"Apple", "Apple chips", "Apple jam", "Apple pancake","Apple chips", "Apple jam"} },

		};



		private static List<Dictionary<string, List<string>>> Products = new List<Dictionary<string, List<string>>>(){
			Products1, Products2
		};

		private Dictionary<string, List<string>> dictionaryAlphabetics = new Dictionary<string, List<string>>(){
			{"A" , new List<string>(){"Apple", "Apple chips", "Apple jam", "Apple pancake"}},
			{"B" , new List<string>(){"Bacon", "Bacon fried", "Bacon, fillet, fat 18 g", "Bacon, fillet, fat 18 g", "Bacon, fillet, fat 18 g"}},
			{"C" , new List<string>(){"C", "CC", "CCC", "CCCC", "CCCCC", "CCCCC", "CCCCC", "CCCCC", "CCCCC"}},
			{"D" , new List<string>(){"D", "DD", "DDD", "DDD", "DDD", "DDD", "DDD", "DDD", "DDD"}},
			{"E" , new List<string>(){"E", "EE", "EEE", "EEE", "EEEE", "EEEEE", "EEEEE", "EEEEE", "EEEEE"}},
			{"F" , new List<string>(){"F", "FF", "FFF", "FFF", "FFF", "FFF", "FFF", "FFF", "FFF"}},
			{"G" , new List<string>(){"G", "GG"}},
			{"H" , new List<string>(){"H", "HH", "HHH"}},
			{"I" , new List<string>(){"I", "II"}},
			{"J" , new List<string>(){"J"}},
			{"K" , new List<string>(){"K", "KK", "KKK"}},
			{"L" , new List<string>(){"L", "LL"}},
			{"M" , new List<string>(){"M"}},
			{"N" , new List<string>(){"N", "NN"}},
			{"O" , new List<string>(){"O"}},
			{"P" , new List<string>(){"P"}},
			{"Q" , new List<string>(){"Q", "QQ"}},
			{"R" , new List<string>(){"R", "RR", "RRR"}},
			{"S" , new List<string>(){"S"}},
			{"T" , new List<string>(){"T", "TT"}},
			{"U" , new List<string>(){"U", "UU"}},
			{"V" , new List<string>(){"V", "VV"}},
			{"W" , new List<string>(){"W", "WW", "WWW"}},
			{"X" , new List<string>(){"X", "XX", "XXX", "XXXX"}},
			{"Y" , new List<string>(){"Y","YY", "YYY"}},
			{"Z" , new List<string>(){"Z", "ZZ", "ZZZ", "ZZZZ", "ZZZZZ"}}
		};

		SearchVM _vm;
		private SearchVM _container;
		private PeriodVM _emptyVM;
		private ObservableCollection<SearchResultVM> collection;


		public FDSelectItemVCSource (SearchVM container){
			_container = container;
			_container.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == "Results")
				if (String.IsNullOrEmpty(_container.Query))
					Collection = _emptyVM.ItemList;
				else
					Collection = _container.Results;
			};
			#if !DEBUG1

			#endif
			_emptyVM = new PeriodVM(DateTime.Now.Date, container.Period);
			_emptyVM.PropertyChanged += (sender, e) => { if (e.PropertyName == "ItemList") Collection = _emptyVM.ItemList; };
			_emptyVM.StartRequery();
			Collection = _emptyVM.ItemList;
		}

		private ObservableCollection<SearchResultVM> Collection
		{
			set
			{
				if (collection == value)
					return;
				if (collection != null)
					collection.CollectionChanged -= HandleCollectionChanged;
				collection = value;
				if (collection != null)
					collection.CollectionChanged += HandleCollectionChanged;
//				reloadtable

			}
		}

		void HandleCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			//this.NotifyDataSetChanged();
		}

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
			var cell = tableView.DequeueReusableCell ("cellSelectItem", indexPath);
			return cell;	
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{

		}
	}
}

