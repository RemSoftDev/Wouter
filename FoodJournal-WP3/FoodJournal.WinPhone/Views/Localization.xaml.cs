using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Logging;
using FoodJournal.ViewModels;
using FoodJournal.Model;
using FoodJournal.AppModel.UI;
using FoodJournal.Runtime;

namespace FoodJournal.Views
{
    public partial class Localization : Screen
    {
        public Localization()
        {
            InitializeComponent();

            ApplicationBar.IsVisible = true;

            ApplicationBarIconButton button = new ApplicationBarIconButton();
            button.IconUri = new Uri("/resources/icons/applicationbar.check.png", UriKind.Relative);
            button.Text = AppResources.Localization_Submit;
            ApplicationBar.Buttons.Add(button);
            button.Click += new EventHandler(Submit_Click);

            DataContext = new LocalizationVM();
        }

        private void Submit_Click(object sender, EventArgs e)
        {
            try
            {
                (DataContext as LocalizationVM).Submit();
                MessageBox.Show(AppResources.Localization_ThankYou);
                Navigate.BackFromOther();
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ((sender as TextBox).DataContext as TranslationRequest).Corrected=(sender as TextBox).Text;
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            try
            {
                ((sender as TextBox).DataContext as LocalizationVM).Comment = (sender as TextBox).Text;
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

    }
}