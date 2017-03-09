#if WINDOWS_PHONE
using FoodJournal.Logging;
using FoodJournal.Model;
using FoodJournal.Values;
using FoodJournal;
using FoodJournal.WinPhone.Common.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using FoodJournal.Messages;
#if WINDOWS_PHONE
using System.Windows.Media.Imaging;
#endif
using FoodJournal.AppModel.UI;
using FoodJournal.AppModel;

namespace FoodJournal.ViewModels
{
    public class PicturesVM : VMBase
    {

        public bool IsPeriod;

        private DateTime date;
        private Period period;

        private FoodItem item;

        private List<string> filenames;

        private int i = 0;
        private string currentfilename;

        private BitmapImage picture;
        public BitmapImage Picture { get { return picture; } }

        public Visibility ControlVisibility { get { return filenames.Count > 0 ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility ButtonVisibility { get { return filenames.Count > 1 ? Visibility.Visible : Visibility.Collapsed; } }

        public PicturesVM(DateTime date, Period period)
        {
            this.IsPeriod = true;
            this.date = date.Date;
            this.period = period;
            Refresh();
            PictureCache.Current.AddVM(this);
        }

        public PicturesVM(FoodItem item)
        {
            this.IsPeriod = false;
            this.item = item;
            Refresh();
            PictureCache.Current.AddVM(this);
        }

        public void Cycle(int direction)
        {
            if (filenames.Count <= 1) return;
            i += direction;
            if (i < 0) i = filenames.Count - 1;
            if (i >= filenames.Count) i = 0;
            Load(filenames[i]);
        }

        public void DeletePicture()
        {
            if (currentfilename != null)
                PictureCache.Current.Delete(currentfilename);
        }

        public void Refresh()
        {
            var oldcount = filenames == null ? 0 : filenames.Count;
            if (IsPeriod)
                filenames = PictureCache.Current.GetPeriodPictures(date, period);
            else
                filenames = PictureCache.Current.GetFoodItemPictures(item.Text);

            if (filenames.Count != oldcount)
            {
                if (filenames.Count > 0)
                {
                    i = filenames.Count - 1;
                    Load(filenames[i]);
                }
                NotifyPropertyChanged("ControlVisibility");
                NotifyPropertyChanged("ButtonVisibility");
            }
        }

        private void Load(string filename)
        {
            if (currentfilename == filename) return;
            try
            {
                picture = PictureCache.Current.LoadPicture(filename);
                currentfilename = filename;
                NotifyPropertyChanged("Picture");
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

    }
}
#endif