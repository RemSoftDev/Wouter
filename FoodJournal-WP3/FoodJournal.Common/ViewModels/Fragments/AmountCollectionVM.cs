using FoodJournal.AppModel;
using FoodJournal.AppModel.UI;
using FoodJournal.Values;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FoodJournal.ViewModels.Fragments
{

    public interface IAcceptsSelectedAmount
    {
        void SetSelectedAmountAndScale(Amount SelectedAmount, Single Scale);
        ServingSizeCollection GetServingSizeCollection();
        Amount GetNewDefaultAmount();
        Amount GetAmountSelected();
        Amount GetTotalAmount();
        void OnAmountConversionChanged();
    }

    public class AmountCollectionVM : VMBase, IAcceptsSelectedAmount
    {

        private IAcceptsSelectedAmount amountContainer;
        private ServingSizeCollection servingSizeCollection;

        private int selectedAmountId;
        private ObservableCollection<AmountVM> _Amounts = new ObservableCollection<AmountVM>();

        public AmountCollectionVM(IAcceptsSelectedAmount amountContainer)
        {
            this.amountContainer = amountContainer;
            this.servingSizeCollection = amountContainer.GetServingSizeCollection();
            RequeryAmounts();
        }

        public void SetSelectedAmountAndScale(Amount SelectedAmount, float Scale) { amountContainer.SetSelectedAmountAndScale(SelectedAmount, Scale); }
        public ServingSizeCollection GetServingSizeCollection() { return servingSizeCollection; }
        Amount IAcceptsSelectedAmount.GetNewDefaultAmount() { return amountContainer.GetNewDefaultAmount(); }
        Amount IAcceptsSelectedAmount.GetAmountSelected() { return amountContainer.GetAmountSelected(); }
        Amount IAcceptsSelectedAmount.GetTotalAmount() { return amountContainer.GetTotalAmount(); }
        public void OnAmountConversionChanged()
        {
            foreach (var a in Amounts) 
                a.SetEquivalent(a.Amount, servingSizeCollection.GetEquivalent(a.Amount));
            amountContainer.OnAmountConversionChanged();
        }

        public Visibility MoreOptionsVisibility { get { return Amounts.Count > 4 ? Visibility.Visible : Visibility.Collapsed; } }
        public double MaxHeight { get { return Amounts.Count > 4 ? 320 : 400; } }
        public bool DeleteVisible { get { return Amounts.Count > 1; } }

        public int SelectedAmountId
        {
            get { return selectedAmountId; }
            set
            {
                if (selectedAmountId == value) return;

                int prev = selectedAmountId;
                selectedAmountId = value;

                foreach (var amount in _Amounts)
                    if (amount.Id == prev)
                        amount.OnCheckedChanged(); // hide the details on previous selection
                    else if (amount.Id == value)
                    {
                        amountContainer.SetSelectedAmountAndScale(amount.Amount, amount.AmountScale);
                        amount.OnCheckedChanged();
                    }
            }
        }

        public void DeleteSelectedServingSize()
        {
            servingSizeCollection.Delete(amountContainer.GetAmountSelected());
            Amount newAmount = servingSizeCollection.FirstAmount;
            if (Amounts.Count > selectedAmountId + 1)
                newAmount = Amounts[selectedAmountId + 1].Amount;
            else if (selectedAmountId > 0)
                newAmount = Amounts[selectedAmountId - 1].Amount;
            if (!newAmount.IsValid) newAmount = amountContainer.GetNewDefaultAmount();

            amountContainer.SetSelectedAmountAndScale(newAmount, 1);
            RequeryAmounts();

            NotifyPropertyChanged("MaxHeight");
            NotifyPropertyChanged("MoreOptionsVisibility");
        }

        public void AddServingSize()
        {
            Amount newAmount = amountContainer.GetTotalAmount();
            servingSizeCollection.InsertAt0(newAmount);
            amountContainer.SetSelectedAmountAndScale(newAmount, 1);
            RequeryAmounts();
        }


        public void RequeryAmounts()
        {
            var amounts = new ObservableCollection<AmountVM>();

            var total = amountContainer.GetTotalAmount();
            var selected = amountContainer.GetAmountSelected();

            int selectedId = -1;

            foreach (var ss in servingSizeCollection.Amounts)
                if (ss.amount1.IsValid)
                {
                    if (selectedId == -1)
                    {
                        if (ss.amount1 == selected) selectedId = amounts.Count;
                        if (ss.amount1 == total) selectedId = amounts.Count;
                    }

                    amounts.Add(new AmountVM(this, amounts.Count, total, ss.amount1, ss.amount2, amounts.Count == selectedId));
                }

            if (selectedId == -1) // inserted later for a better delete experience
                amounts.Insert(0, new AmountVM(this, -1, total, total, servingSizeCollection.GetEquivalent(total), false));

            selectedAmountId = selectedId;

            _Amounts = amounts;
            NotifyPropertyChanged("MoreOptionsVisibility");
            NotifyPropertyChanged("MaxHeight");
            NotifyPropertyChanged("Amounts");

        }

        public ObservableCollection<AmountVM> Amounts
        {
            get { return _Amounts; }
            set
            {
                _Amounts = value;
                NotifyPropertyChanged("Amounts");
            }
        }




    }


}
