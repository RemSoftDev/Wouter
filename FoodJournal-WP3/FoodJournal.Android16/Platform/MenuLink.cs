using System;
using FoodJournal.Logging;

namespace FoodJournal.Android15
{

	public class MenuLink
	{

		public readonly string Text;
		private readonly Action action;

		public MenuLink(string Text, Action a)  
		{
			this.Text = Text;
			this.action = a; 
		}

		public void Invoke()
		{
			try { action.Invoke(); }
			catch (Exception ex) { LittleWatson.ReportException(ex); }
		}

	}
}

