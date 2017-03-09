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
using FoodJournal.Parsing;
using FoodJournal.AppModel;
using FoodJournal.Resources;

namespace FoodJournal.ViewModels
{

    public class LocalizationVM : INotifyPropertyChanged
    {

        public LocalizationVM()
        {


            if (!AppStats.Current.HasTranslationRequests) return;


            foreach (var rq in AppStats.Current.CultureSettings.TranslationRequests)
            {
                rq.Description = rq.Description.Replace("{Original}", AppResources.Localization_Original).Replace("{Description}", AppResources.Localization_Description);
                rq.Corrected = rq.AutoTranslation;
            }

            TranslationRequests = AppStats.Current.CultureSettings.TranslationRequests;

        }


        public string Comment { get; set; }

        public TranslationRequest[] TranslationRequests { get; set; }

        public void Submit()
        {
            Messages.MessageQueue.Push(new Messages.TranslationsMessage(Comment, TranslationRequests.ToArray()));
            AppStats.Current.CultureSettings=null;
        }


        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }

}
