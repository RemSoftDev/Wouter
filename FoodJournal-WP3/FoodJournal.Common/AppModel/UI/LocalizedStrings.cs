using FoodJournal.WinPhone.Common.Resources;
#if !WINDOWS_PHONE
using FoodJournal.Resources;
#endif

namespace FoodJournal.AppModel.UI
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static AppResources _localizedResources = new AppResources();
        public AppResources LocalizedResources { get { return _localizedResources; } }
        public AppResources Strings { get { return _localizedResources; } }
    }
}