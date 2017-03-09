using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using FoodJournal.Logging;

namespace FoodJournal.AppModel
{
	public abstract class VMBase : INotifyPropertyChanged
	{

		#region Notify/PropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null) 
			{
				string propertyName2 = propertyName + ""; 
				FoodJournal.AppModel.Platform.RunSafeOnUIThread ("VMBase.NotifyPropertyChanged", () => {
					try
					{
						PropertyChanged(this, new PropertyChangedEventArgs(propertyName2));
					}
					catch (Exception ex) { LittleWatson.ReportException(ex); }
				});
			}
		}

		#endregion

	}
}
