using FoodJournal.Model;
using FoodJournal.Values;
using System.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Logging;
using FoodJournal.Messages;
using FoodJournal.AppModel;
using FoodJournal.Runtime;
using FoodJournal.Resources;

namespace FoodJournal.ViewModels
{

    public class EntryRowVM : VMBase
    {

        // text, summary
        private Entry _entry;
        private DateTime _entryLastChanged;

        public EntryRowVM(Entry entry)
        {
            _entry = entry;
            _entryLastChanged = entry.LastChanged;
        }

        public string ItemText { get { return _entry.Item.Text; } }
        public string Summary { get { return _entry.Summary; } }

        public List<object> ContextMenuItems { get { return new List<object>() { AppResources.Delete }; } }

        public bool IsForEntry(Entry entry) { return this._entry == entry; }
        
        public void NavigateToEntryDetail() { Navigate.ToEntryDetail(_entry as Entry); }
        public void DeleteEntry() { _entry.Delete(); }

        public void NotifyIfChanged()
        {

            if (_entryLastChanged == _entry.LastChanged) return;

            MessageQueue.Push(new Messages.EntryUpdatedMessage(_entry as Entry));

            NotifyPropertyChanged("ItemText");
            NotifyPropertyChanged("Summary");

            _entryLastChanged = _entry.LastChanged;

        }


    }
}