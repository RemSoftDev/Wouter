using FoodJournal.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_PHONE
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
#endif

namespace FoodJournal.AppModel.UI
{

#if WINDOWS_PHONE
    public class Screen : PhoneApplicationPage
#else
    public class Screen
#endif
    {

        public readonly string Name;

        protected Screen()
        {
            this.Name = this.GetType().Name;

//#if WINDOWS_PHONE
//            SessionLog.CurrentScreen = this;

//            this.LayoutUpdated += PhoneApplicationPage_LayoutUpdated;
//#endif
            
        }

        protected virtual void OnNavigatedLayoutUpdated() { }

#if WINDOWS_PHONE

        private bool _navigatedToCalled;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _navigatedToCalled = true;
            //SessionLog.CurrentScreen = this;
        }

        private void PhoneApplicationPage_LayoutUpdated(object sender, EventArgs e)
        {
#if DEBUG && SCREEONSHOT
            if (ScreenshotVM.InScreenshot)
            {
                if (ScreenshotVM.phase == ScreenshotVM.ScreenshotPhase.Entry)
                    ScreenshotVM.ScreenshotLayoutUpdated(this);
                return;
            }
#endif
            if (_navigatedToCalled)
            {
                _navigatedToCalled = false;
                OnNavigatedLayoutUpdated();
            }
        }

#endif

    }
}
