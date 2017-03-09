using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Threading;
using System.Globalization;
using FoodJournal.Logging;

namespace FoodJournal.AppModel
{
	public static class BackgroundTask
	{

		public static void Start(int delayinms, Action action)
		{

			var bw = new System.ComponentModel.BackgroundWorker();
			bw.DoWork += (s, a) =>
			{

				System.Threading.Thread.Sleep(delayinms);

				#if DEBUG && WINDOWS_PHONE
				try
				{
				if (!String.IsNullOrWhiteSpace(AppStats.appForceCulture))
				{
				Thread.CurrentThread.CurrentCulture = new CultureInfo(AppStats.appForceCulture);
				Thread.CurrentThread.CurrentUICulture = new CultureInfo(AppStats.appForceCulture);
				}
				}
				catch (Exception ex) { LittleWatson.ReportException(ex); }
				#endif

				try
				{
					action.Invoke();
				}
				catch (Exception ex) { LittleWatson.ReportException(ex); }
			};

			bw.RunWorkerAsync();

		}

	}
}
