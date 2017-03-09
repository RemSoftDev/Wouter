using FoodJournal.AppModel;
using FoodJournal.AppModel.UI;
using FoodJournal.Logging;
using FoodJournal.Model;
using FoodJournal.Parsing;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FoodJournal.Resources;

namespace FoodJournal.ViewModels.Fragments
{

    public class AmountVM : VMBase
    {

        private AmountCollectionVM amountCollectionVM;

        public readonly int Id;
        private Amount amount;
        private Single amountScale;
        private Single sliderScale;
        private Single equalsGram;

        public AmountVM(AmountCollectionVM amountCollectionVM, int Id, Amount totalAmount, Amount amount, Amount equalsAmount, bool isselected)
        {
            this.amountCollectionVM = amountCollectionVM;
            this.Id = Id;
            this.amount = amount;
            try
            {
                amountScale = isselected ? totalAmount / amount : 1;
                sliderScale = 1 / amountScale;
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); amountScale = 1; sliderScale = 1; }
            this.equalsGram = equalsAmount / "1 g";
        }

        public Amount Amount { get { return amount; } }
        public Single AmountScale { get { return amountScale; } }
        public Amount EqualsAmount { get { return Amount.FromGram(equalsGram); } }
        public string Gram { get { return AppResources.Unit_Gram.ToLower(); } }
		public string NotCheckedText { get { return IsChecked ? "" : Text; } }

        public bool IsChecked { get { return amountCollectionVM.SelectedAmountId == Id; } set { amountCollectionVM.SelectedAmountId = Id; } }

        public Visibility CheckedVisibility { get { return IsChecked ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility NotCheckedVisibility { get { return IsChecked ? Visibility.Collapsed : Visibility.Visible; } }
        public int TextWidth { get { return amountCollectionVM.DeleteVisible ? 356 : 456; } }
        public Visibility DeleteVisibility { get { return amountCollectionVM.DeleteVisible ? Visibility.Visible : Visibility.Collapsed; } }

        public void OnCheckedChanged()
        {
            EquivalentSectionVisibility = Visibility.Collapsed;
            NotifyPropertyChanged("IsChecked");
            NotifyPropertyChanged("CheckedVisibility");
            NotifyPropertyChanged("NotCheckedVisibility");
            NotifyPropertyChanged("EquivalentNumber");
			NotifyPropertyChanged("SliderValue");
			NotifyPropertyChanged("AndroidSliderValue");
			NotifyPropertyChanged("NotCheckedText");
        }

		public string TextWithGram{
			get{
				if (equalsGram > 0)
					return Text + string.Format (" ({0} {1})", equalsGram.ToUIString (), AppResources.Unit_Gram);
				return Text;
		}}

        public string Text
        {
            get { return (amount * amountScale).ToString(true); }
            set
            {
                Amount newamount = (Amount)value;
                if (newamount.IsValid && amount != newamount)
                {

                    var collection = amountCollectionVM.GetServingSizeCollection();
                    Amount newEquivalent = collection.GetEquivalent(newamount);
                    if (!newEquivalent.IsValid) newEquivalent = EqualsAmount;
                    collection.RenameAmount(amount, newamount, newEquivalent);

                    amount = newamount;
                    sliderScale = 1;
                    amountScale = 1;

                    SetEquivalent(value, newEquivalent);
                    amountCollectionVM.SetSelectedAmountAndScale(value, 1);
                    NotifyPropertyChanged("SliderValue");
					NotifyPropertyChanged ("AndroidSliderValue");

                    EquivalentSectionVisibility = Visibility.Visible; // needed when changing from su to non-su or vice versa

                }
                NotifyPropertyChanged("Text"); // outside of if block bc invalid amounts should be fixed in UI
            }
        }

        public void ScaleSlider(bool up)
        {
            if (up)
                sliderScale = sliderScale * 2;
            else
                sliderScale = sliderScale / 2;

			NotifyPropertyChanged("SliderValue");
			NotifyPropertyChanged("AndroidSliderValue");
        }

		public int AndroidSliderValue{get {return (int)(SliderValue * 24);} set {SliderValue=value / 24.0F;}}

        public Single SliderValue
        {
            get { return amountScale * sliderScale; }
            set
            {
                value = (Single)(Math.Round(value * 12.0) / 12.0);
                if (value < 0.5) value = 0.5F;

                if (amountScale != value / sliderScale)
                {
                    amountScale = value / sliderScale;
                    //NotifyPropertyChanged("SliderValue"); <-- jiggery
                    NotifyPropertyChanged("Text");
                    NotifyPropertyChanged("TextCollapsed");
                    NotifyPropertyChanged("EquivalentNumber");
                    amountCollectionVM.SetSelectedAmountAndScale(amount, amountScale);
                }

            }
        }

        public void SetEquivalent(Amount amount, Amount eqAmount)
        {
#if DEBUG
            if (this.amount != amount) throw new Exception("equivalent should be scaled to amount");
#endif
            this.equalsGram = eqAmount / "1 g";
            NotifyPropertyChanged("EquivalentNumber");
        }

        public string EquivalentNumber
        {
            get { return equalsGram > 0 ? (equalsGram * amountScale).ToUIString() : ""; }
            set
            {

                var val = Floats.ParseUnknown(value, 0);

                if (equalsGram * amountScale != val)
                {
                    var collection = amountCollectionVM.GetServingSizeCollection();
                    if (val == 0)
                        collection.SetConversion(amount, Amount.Empty);
                    else
                        collection.SetConversion(amount, Amount.FromGram(val / amountScale));
                    amountCollectionVM.OnAmountConversionChanged();
                }
            }
        }

        private Visibility equivalentSectionVisibility = Visibility.Collapsed;
        public Visibility EquivalentSectionVisibility
        {
            get { return equivalentSectionVisibility; }
            set
            {
                if (amount.IsConvertable(FoodItem.NutritionScale)) value = Visibility.Collapsed; // Equivalents dont open if value is standard unit
                if (equivalentSectionVisibility != value)
                {
                    equivalentSectionVisibility = value;
                    NotifyPropertyChanged("EquivalentSectionVisibility");
                }
            }
        }

        public void OnTextGotFocus() { EquivalentSectionVisibility = Visibility.Visible; }

        public void DeleteAmount() { amountCollectionVM.DeleteSelectedServingSize(); }

		public void OnSliderLostMouseCapture()
		{
			if (SliderValue <= .5) ScaleSlider(true);
			if (SliderValue >= 2) ScaleSlider(false);
		}

    }


}
