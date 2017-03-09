using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FoodJournal.ViewModels;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Logging;

namespace FoodJournal.Views
{
    public partial class PicturesView : UserControl
    {

        public static bool typeadded = false;

        public PicturesView()
        {
            InitializeComponent();
            if (!typeadded)
            {
                TiltEffect.TiltableItems.Add(typeof(PicturesView));
                typeadded = true;
            }
        }

        private void LeftButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (DataContext != null)
                (DataContext as PicturesVM).Cycle(-1);
        }

        private void RightButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (DataContext != null)
                (DataContext as PicturesVM).Cycle(1);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            menu.IsOpen = false;
            (DataContext as PicturesVM).DeletePicture();
        }

        private ContextMenu menu;
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            menu = sender as ContextMenu;
            if (menu.Items.Count == 0)
            {
                var mi = new Microsoft.Phone.Controls.MenuItem();
                mi.Header = AppResources.Delete;
                mi.Click += MenuItem_Click;
                menu.Items.Add(mi);
            }
        }

        private void ImageTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                if (this.VisualStateGroup.CurrentState == null || this.VisualStateGroup.CurrentState.Name != "ZoomedIn")
                    VisualStateManager.GoToState(this, "ZoomedIn", true);
                else
                    VisualStateManager.GoToState(this, "Normal", true);
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

    }
}
