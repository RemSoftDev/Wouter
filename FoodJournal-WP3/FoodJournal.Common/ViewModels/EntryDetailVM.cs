using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using FoodJournal.Model;
using FoodJournal.Values;
using System.Windows;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Parsing;
using FoodJournal.Logging;
using System.Collections.Generic;
using FoodJournal.Messages;
using FoodJournal.AppModel.UI;
using FoodJournal.ViewModels.Fragments;
using FoodJournal.AppModel;
using FoodJournal.Runtime;

namespace FoodJournal.ViewModels
{

    public class EntryDetailVM : VMBase, IAcceptsSelectedAmount
    {

        private readonly FoodItem item;
        private readonly Entry entry;

		#if WINDOWS_PHONE
		public PicturesVM PicturesVM { get; set; }
		#endif
        public AmountCollectionVM AmountCollectionVM { get; set; }

		private bool is100Gram = false;

        public EntryDetailVM(Entry entry)
        {

			if (!Cache.AllItemsLoaded)
				FoodJournalNoSQL.LoadItems (false, null);

            this.entry = entry;
            this.item = entry.Item;
			#if WINDOWS_PHONE
            PicturesVM = new PicturesVM(item);
			#endif
            AmountCollectionVM = new AmountCollectionVM(this);
            PopulateProperties();
        }

        //public Entry Entry { get { return entry; } }

        public Visibility DescriptionVisibility { get { return !string.IsNullOrEmpty(item.Description) ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility DeleteVisibility { get { return entry.IsNewEntry ? Visibility.Collapsed : Visibility.Visible; } }

        public string Text { get { return item.Text; } 
            set {
                if (value != item.Text) { 
					// 1/14/16 doing this inside the entry instead only item.Text = value;  entry.Text = value; 
					entry.Text = value;
				}
            }
        }
        public string Description { get { return item.Description; } set { if (value != item.Description) item.Description = value; } }

        // IAcceptsSelectedAmount
        public void SetSelectedAmountAndScale(Amount SelectedAmount, float Scale)
        {
            entry.SetAmountAndScale(SelectedAmount, Scale);
            RefreshProperties();
			NotifyPropertyChanged ("SelectedServingText");
        }
        public ServingSizeCollection GetServingSizeCollection() { return entry.Item.ServingSizes; }
        public Amount GetNewDefaultAmount() { return entry.Item.NewEntryAmount; }
        public Amount GetAmountSelected() { return entry.AmountSelected; }
        public Amount GetTotalAmount() { return entry.TotalAmount; }
        public void OnAmountConversionChanged()
        {
            entry.OnAmountConversionChanged();
            RefreshProperties();
        }

		public bool Is100Gram { get { return is100Gram; } set { 
				if (is100Gram == value)
					return;
				is100Gram = value; 
				foreach (var prop in properties)
					(prop as PropertyEntryVM).Is100G = value;
				NotifyPropertyChanged ("Is100Gram"); NotifyPropertyChanged ("IsNot100Gram"); }}
		public bool IsNot100Gram { get { return !is100Gram; } set { Is100Gram = !value; } }	
		public string SelectedServingText { get { return entry.TotalAmountTextWithGram; } }

        private void PopulateProperties()
        {
            var properties = new ObservableCollection<PropertyVM>();
            foreach (var prop in UserSettings.Current.SelectedProperties)
				properties.Add(new PropertyEntryVM(prop, entry, is100Gram));
            this.Properties = properties;
        }

        public void RefreshProperties()
        {
            foreach (var prop in properties)
                prop.Refresh();
        }

        private ObservableCollection<PropertyVM> properties;
        public ObservableCollection<PropertyVM> Properties
        {
            get { return properties; }
            set
            {
                if (value != properties)
                {
                    properties = value;
                    NotifyPropertyChanged("Properties");
                }
            }
        }

        public void CommitNewItem()
        {

            if (!entry.Item.IsNewItem)
            {
                Platform.MessageBox("Item is not new");
                    
            } //throw new ArgumentOutOfRangeException("Item is not new");

            if (Navigate.IAcceptsNewEntry != null)
                if (!Navigate.IAcceptsNewEntry.ShouldSaveNewEntry(entry))
                {
                    Navigate.BackAfterSubmit();
                    return;
                }
        //    entry.Item.NotifyEntries.Remove(entry);
          //  entry.Item.NotifyEntries.Add(entry);
            entry.Save();
            
			#if WINDOWS_PHONE
			PictureCache.Current.UpdateNewItemWithID(item.Text);
			#endif

            MessageQueue.Push(new Messages.EntryUpdatedMessage(entry)); // do this in model?
            Navigate.BackAfterSubmit();

        }

        public void DeleteFromDB()
        {
            entry.Delete();
            Navigate.BackFromEntryDetail();
        }

        public void AddServingSize()
        {
            AmountCollectionVM.AddServingSize();
        }

        public void StartPicture()
        {
			#if WINDOWS_PHONE
             FoodItem item = entry.Item;
             //Navigate.ToCamera(PictureCache.Current.NextFilename(item.Text), item.SourceID);
			#endif
        }

    }
}