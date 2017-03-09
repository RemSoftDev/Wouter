using FoodJournal.AppModel;
using FoodJournal.Logging;
using FoodJournal.Messages;
using FoodJournal.Model;
using FoodJournal.ResourceData;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Runtime;
using FoodJournal.Search;
//using FoodJournal.Services;
using FoodJournal.Values;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using FoodJournal.AppModel.UI;

namespace FoodJournal.ViewModels
{

	public delegate void DeleteModeChangedEventHandler(object sender, bool DeleteMode);

	public class PeriodDeleteVM : VMBase
	{

		public Period Period;

		public event DeleteModeChangedEventHandler DeleteModeChanged;

		private HashSet<Entry> selected = new HashSet<Entry>();

		private bool Enabling = false;

		private bool inDeleteMode = false;
		public bool InDeleteMode {
			get { return inDeleteMode; }
			set {				

				if (inDeleteMode == value)return;
				if (Enabling)return;

				Enabling = true;

				inDeleteMode = value;
				NotifyPropertyChanged ("InDeleteMode");
				if (!inDeleteMode)
					selected.Clear ();
				
				if (DeleteModeChanged != null)
					DeleteModeChanged.Invoke (this, inDeleteMode);

				Enabling = false;
			}
		}

		public bool IsSelected(Entry entry) {
			return selected.Contains (entry);
		}

		public void SetSelected(Entry entry, bool selected)
		{			
			if (selected && !this.selected.Contains(entry)) this.selected.Add(entry);
			if (!selected && this.selected.Contains (entry)) this.selected.Remove (entry);
		}

		public void DeleteAll(){
			foreach (var entry in selected)
				entry.Delete ();
			InDeleteMode = false;
		}

    }
}
