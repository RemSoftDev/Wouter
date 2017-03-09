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
using FoodJournal.ViewModels.Fragments;

namespace FoodJournal.Views
{

    public partial class ServingSizes : UserControl
    {
        public ServingSizes()
        {
            InitializeComponent();
        }


        private void DeleteAmountTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var x = (sender as Button).DataContext as AmountVM;
                if (x != null) x.DeleteAmount();
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

        }

        private void MoreOptionsTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                (sender as Button).Visibility = System.Windows.Visibility.Collapsed;
                AmountsList.MaxHeight = 99999;
                AmountsList.UpdateLayout();
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

        }

        private void Text_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ((sender as TextBox).DataContext as AmountVM).OnTextGotFocus();
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }


        private void Slider_LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                ((sender as Slider).DataContext as AmountVM).OnSliderLostMouseCapture();
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

    }
}
