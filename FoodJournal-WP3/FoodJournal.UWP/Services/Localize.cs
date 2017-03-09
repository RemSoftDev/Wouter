using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//[assembly: Dependency(typeof(FoodJournal.Droid.Services.Localize))]
namespace FoodJournal.UWP.Services
{
    using System;
    using System.Threading;
    using FoodJournal.Common.Services;

    using Xamarin.Forms;

            public class Localize : ILocalize
        {
            public System.Globalization.CultureInfo GetCurrentCultureInfo()
            {

            return null;// new System.Globalization.CultureInfo(netLanguage);
            }

            public void SetLocale()
            {
            }
        }
    
}
