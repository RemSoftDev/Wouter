using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FoodJournal.Logging;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.AppModel.UI;
using FoodJournal.Runtime;
using FoodJournal.AppModel;
using FoodJournal.Model;

namespace FoodJournal.Views
{
    public partial class BuyNow : Screen
    {

        private Sale sale;

        public BuyNow()
        {
            InitializeComponent();
            Price2Grid.Visibility = System.Windows.Visibility.Collapsed;
            Price.Visibility = System.Windows.Visibility.Collapsed;
            UpdatePrice();
        }

        private async void UpdatePrice()
        {

            try
            {

                sale = AppStats.Current.CurrentSale;

                if (sale != null)
                {

                    Price.Text = sale.Title;
                    Price.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
                    Price.Visibility = System.Windows.Visibility.Visible;

                }
                
#if DEBUG
                    if (Price.Visibility == System.Windows.Visibility.Collapsed || sale != null)
                        return;
#endif

                    var listing = await Windows.ApplicationModel.Store.CurrentApp.LoadListingInformationByProductIdsAsync(new string[] { AppStats.PremiumProduct });

                    var hasprice = false;
                    foreach (char c in listing.FormattedPrice)
                        if (char.IsNumber(c) && c != '0')
                            hasprice = true;

                    if (hasprice)
                    {
                        if (sale == null)
                        {
                            Price.Text = string.Format(AppResources.Price, listing.FormattedPrice);
                            Price.Visibility = System.Windows.Visibility.Visible;
                        } else
                        {
                            Price2.Text = string.Format(AppResources.Price, listing.FormattedPrice);
                            Price2Grid.Visibility = System.Windows.Visibility.Visible;
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid price in product listing: " + listing.FormattedPrice);
                    }

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        private void BuyNowTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                if (sale != null)
                    Navigate.StartBuyProduct(sale.productId);
                else
                    Navigate.StartBuyPremium();
                Navigate.BackFromOther();
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }
    }
}