using FoodJournal.Logging;
#if WINDOWS_PHONE
using Microsoft.Phone.Shell;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournal.AppModel.UI
{

#if WINDOWS_PHONE
    public class MenuButton : ApplicationBarIconButton
    {

        private readonly Action action;

        public MenuButton(string iconUrl, string Text, Action a) : base(new Uri(iconUrl, UriKind.Relative)) 
        {
            this.Text = Text;
            this.Click += MenuLink_Click;
            this.action = a; 
        }

        void MenuLink_Click(object sender, EventArgs e)
        {
            try { action.Invoke(); }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

    }


    public class MenuLink : ApplicationBarMenuItem
    {

        private readonly Action action;

        public MenuLink(string Text, Action a) : base(Text) 
        {
            this.Click += MenuLink_Click;
            this.action = a; 
        }

        void MenuLink_Click(object sender, EventArgs e)
        {
            try { action.Invoke(); }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

    }
#endif
}
