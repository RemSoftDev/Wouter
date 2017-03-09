using FoodJournal.AppModel;
using FoodJournal.Logging;
using FoodJournal.Model;
using FoodJournal.ResourceData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if WINDOWS_PHONE
using Windows.ApplicationModel.Store;
#endif

namespace FoodJournal.AppModel
{

	[DataContract]
	public class AppStats
	{

		#if DEBUG //&& WINDOWS_PHONE
		public static string appForceCulture; // if implemnting force: make sure to enalbe setting on threads...




		#if WINDOWS_PHONE
		public static System.Windows.FlowDirection appForceFlow;
		#endif
		#endif

		public static bool CultureCommaNotADot;
		public static string CultureComma;

		#if WINDOWS_PHONE
		public const string email = "daily.journal@outlook.com";




		#else
		public const string email = "daily.journal.app@gmail.com";
		#endif

		public enum ProductKind
		{
			unknown,
			Trial,
			// Ads show
			Paid,
			// Ads don't show
			Free
			// Ads show, until RemoveAdsProduct is purchased
		}

		public const string RemoveAdsProduct = "removead";
		public const string PremiumProduct = "premium";

		private bool canSave = false;

		public AppStats ()
		{
		}

		#region Current

		public static void Reset ()
		{
			current = null;
		}

		private static AppStats current;

		public static AppStats Current {
			get {
				try {
					if (current == null) {
						current = LocalStorage<AppStats>.LoadOrNew ("AppStats");
						current.UpdateStats ();
						current.canSave = true;
						current.Save ();
					}
				} catch (Exception ex) {
					LittleWatson.ReportException (ex);
					if (current == null)
						current = new AppStats ();
					current.UpdateStats ();
				}
				return current;
			}
		}

		#endregion

		private void Save ()
		{
			if (!canSave)
				return;
			LocalStorage<AppStats>.Save ("AppStats", current);
		}

		public void TemporarySuppressAds ()
		{
			SuppressAdsUntil = DateTime.Now.AddDays (1).Date;
			Save ();
		}

		public bool IsTrialExpired {
			get {
				if (InstalledProductKind != ProductKind.Trial)
					return false;
				return TrialDaysRemaining <= 0;
			}
		}

		public bool ShouldShowAds {
			get {
				if (InstalledProductKind != ProductKind.Free)
					return false;
				#if DEBUG
				//SuppressAdsUntil = DateTime.Now.AddDays(-1);
				#endif
				return AdShows ? DateTime.Now > SuppressAdsUntil : false;
			}
		}

		public bool ShouldShowInterstitials
		{
			get {
				if (InstalledProductKind != ProductKind.Free)
					return false;
				return AdShows ? DateTime.Now > SuppressAdsUntil : false;
			}
		}

		#if WINDOWS_PHONE && False
		public bool IncludePremiumItems { get { return InstalledProductKind == ProductKind.Paid; } }

		#else
		public bool IncludePremiumItems { get { return true; } }
		#endif

		public bool PremiumItemsLocked { get { return InstalledProductKind != ProductKind.Paid; } }

		#if !WINDOWS_PHONE
		public void RegisterPurchase (string productid)
		{

			try {

				SessionLog.RecordMilestone (productid + " Purchased", AppStats.Current.SessionId.ToString ());
				InstalledProductKind = ProductKind.Paid;
				AdShows = false;
				MultiMealHidden = false;
				Save ();

				//foreach (var a in FoodJournal.Android15.Adapters.PeriodListAdapter.AllAds)
				//    try {	
				//        a.Visibility = Android.Views.ViewStates.Gone;
				//    } catch (Exception ex) {
				//        LittleWatson.ReportException (ex);
				//    }

			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}

		}
		#endif
		public void UnRegisterPurchase ()
		{

			try {

				//	SessionLog.RecordMilestone (productid + " Purchased", AppStats.Current.SessionId.ToString ());
				InstalledProductKind = ProductKind.Free;
				AdShows = true;
				MultiMealHidden = true;
				Save ();

				//foreach (var a in FoodJournal.Android15.Adapters.PeriodListAdapter.AllAds)
				//    try {	
				//    a.Visibility = Android.Views.ViewStates.Visible;
				//} catch (Exception ex) {
				//    LittleWatson.ReportException (ex);
				//}

			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}

		}

		public void CalculateTrialDays()
		{

			if (cultureSettings == null)
				TrialDaysRemaining = 0;
			else
				TrialDaysRemaining = 0;//cultureSettings.FreeTrialDays - DaysSinceInstall;

			if (TrialDaysRemaining > 0 && InstalledProductKind == ProductKind.Free)
				InstalledProductKind = ProductKind.Trial;
			if (TrialDaysRemaining <= 0 && InstalledProductKind == ProductKind.Trial)
				InstalledProductKind = ProductKind.Free;

		}

		public void CheckPurchases ()
		{
			#if WINDOWS_PHONE
			try
			{
			//                if (InstalledProductKind == ProductKind.unknown) return;

			#if !DEBUG
			if (InstalledProductKind == ProductKind.Trial)
			{
			var licenseInfo = new Microsoft.Phone.Marketplace.LicenseInformation();
			if (!licenseInfo.IsTrial())
			{
			SessionLog.RecordMilestone("Converted from Trial to Paid", SessionId.ToString());
			InstalledProductKind = ProductKind.Paid;
			Save();
			}
			}
			#endif

			if (InstalledProductKind == ProductKind.Free && AdShows)
			{

			var productLicenses = CurrentApp.LicenseInformation.ProductLicenses;
			foreach (var license in productLicenses.Values)
			{
			if (license.IsActive && license.ProductId == RemoveAdsProduct)
			{
			SessionLog.RecordMilestone("RemoveAdsProduct Purchased", AppStats.Current.SessionId.ToString());
			InstalledProductKind = ProductKind.Paid;
			AdShows = false;
			Save();
			}

			if (license.IsActive && license.ProductId.StartsWith(PremiumProduct))
			{
			SessionLog.RecordMilestone(license.ProductId + " Purchased", AppStats.Current.SessionId.ToString());
			InstalledProductKind = ProductKind.Paid;
			AdShows = false;
			MultiMealHidden = false;
			Save();
			}
			}
			}

			}
			catch (Exception ex) { LittleWatson.ReportException(ex); }
			#endif
		}

		private void UpdateStats ()
		{

			if (AppInstance == null) {
				// new install
				#if DEBUG
				//AppInstance = "DEBUGINSTANCE";
				AppInstance = (new Guid (0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3)).ToString();
				#else
				AppInstance = Guid.NewGuid ().ToString ();
				#endif
				Culture = Thread.CurrentThread.CurrentUICulture.Name;
				InstalledProductKind = ProductKind.unknown;
				InstallDate = DateTime.Now.Date;
				PreviousSessionDate = InstallDate;
				UserId = GetWindowsLiveAnonymousID ();
				DeviceInfo = GetDeviceInfo ();
				DaysInStreak = 1;
				LastTrialReminder = DateTime.MinValue; //DateTime.Now.Date;
				AdShows = true;
				DeDupeDone = true;

				Version = new AssemblyName (Assembly.GetExecutingAssembly ().FullName).Version.ToString ();
				SessionLog.RecordMilestone ("New Install", Version);

			}

			// new or existing install

			if (Culture != Thread.CurrentThread.CurrentUICulture.Name) {
				SessionLog.RecordTraceValue ("Culture changed", string.Format ("From {0} to {1}", Culture, Thread.CurrentThread.CurrentUICulture.Name));
				Culture = Thread.CurrentThread.CurrentUICulture.Name;
			}

			SessionId = SessionId + 1;

			if (SessionId > 1)
				DeviceInfo = "";

			if (PreviousSessionDate < DateTime.Now.Date) {
				if (PreviousSessionDate == DateTime.Now.Date.AddDays (-1))
					DaysInStreak += 1;
				else
					DaysInStreak = 1;
				PreviousSessionDate = DateTime.Now.Date;
			}

			DaysSinceInstall = DateTime.Now.Date.Subtract (InstallDate).Days;

			if (InstalledProductKind == ProductKind.unknown)
				GetInstalledProductKind ();

			CalculateTrialDays ();

			CheckPurchases ();

			if (LastUpgrade == DateTime.MinValue)
				LastUpgrade = DateTime.Now;

			var version = new AssemblyName (Assembly.GetExecutingAssembly ().FullName).Version.ToString ();
			if (Version != version) {
				SessionLog.RecordMilestone ("Upgraded", string.Format ("from {0} to {1}", Version, version));
				Version = version;
				LastUpgrade = DateTime.Now;

				if (string.IsNullOrEmpty (UserId)) {
					UserId = GetWindowsLiveAnonymousID ();
					if (!string.IsNullOrEmpty (UserId))
						SessionLog.RecordMilestone ("UID", UserId);
				}
			}

			RecordPerf = ((new System.Random ()).Next (10) == 0);

		}

		#region Properties

		[DataMember (Order = 0)]
		public string AppInstance { get; set; }

		[DataMember (Order = 1)]
		public int SessionId { get; set; }

		[DataMember (Order = 2)]
		public string LicensedProducts { get; set; }

		[DataMember (Order = 3)]
		public string Culture { get; set; }

		[DataMember (Order = 5)]
		public string ExperimentationSKU { get; set; }

		[DataMember (Order = 6)]
		public DateTime InstallDate { get; set; }

		[DataMember (Order = 4)]
		public int DaysSinceInstall { get; set; }

		[DataMember (Order = 7)]
		public DateTime PreviousSessionDate { get; set; }

		[DataMember (Order = 8)]
		public int DaysInStreak { get; set; }

		[DataMember (Order = 9)]
		public int TrialDaysRemaining { get; set; }

		[DataMember (Order = 10)]
		public string UserId { get; set; }

		[DataMember (Order = 11)]
		public string DeviceInfo { get; set; }

		private int exceptionCount;

		[DataMember (Order = 12)]
		public int ExceptionCount {
			get { return exceptionCount; }
			set {
				exceptionCount = value;
				Save ();
			}
		}

		private int reviewRequests;

		[DataMember (Order = 13)]
		public int ReviewRequests {
			get { return reviewRequests; }
			set {
				reviewRequests = value;
				Save ();
			}
		}

		[DataMember (Order = 14)]
		public string Version { get; set; }

		[DataMember (Order = 15)]
		public DateTime LastUpgrade { get; set; }

		[DataMember (Order = 16)]
		public bool AdShows { get; set; }

		[DataMember (Order = 17)]
		public DateTime SuppressAdsUntil { get; set; }

		[DataMember (Order = 18)]
		public bool RecordPerf { get; set; }

		#if DEBUG
		public void ResetCulture() { _databaseCulture = null; }// used for screenshots
		#endif

		private string _databaseCulture;

		[DataMember (Order = 19)]
		public string DatabaseCulture {
			get {
				if (_databaseCulture == null) {
					_databaseCulture = Culture.ToLower ();

					#if WINDOWS_PHONE
					if (_databaseCulture == "nb-no") _databaseCulture = "no";
					if (_databaseCulture == "zh-tw") _databaseCulture = "zt";

					if (_databaseCulture == "en-gb") _databaseCulture = "gb";
					if (_databaseCulture == "es-mx") _databaseCulture = "mx";
					if (_databaseCulture == "pt-br") _databaseCulture = "br";
					#endif

					if (!ResourceDatabase2.SupportedCultures2.Contains (_databaseCulture))
						_databaseCulture = _databaseCulture.Substring (0, 2);

					if (!ResourceDatabase2.SupportedCultures2.Contains (_databaseCulture))
						_databaseCulture = "en";

				}
				return _databaseCulture;
			}
			set { }
		}

		[DataMember (Order = 20)]
		public ProductKind InstalledProductKind { get; set; }

		public DateTime lastTrialReminder;

		[DataMember (Order = 21)]
		public DateTime LastTrialReminder {
			get { return lastTrialReminder; }
			set {
				lastTrialReminder = value;
				Save ();
			}
		}

		[DataMember (Order = 22)]
		public bool MultiMealHidden { get; set; }

		private CultureSettings cultureSettings;

		[DataMember (Order = 23)]
		public CultureSettings CultureSettings {
			get { return cultureSettings; }
			set {
				cultureSettings = value;
				CalculateTrialDays ();
				if (canSave) {

					#if DEBUG
					if (!canSave){
						CultureSettings.Test();
					}
					#endif

					CultureSettingsLastUpdated = DateTime.Now;
				}
				Save ();
			}
		}

		[DataMember (Order = 24)]
		public DateTime CultureSettingsLastUpdated { get; set; }

		public bool HasTranslationRequests { get { return cultureSettings != null && cultureSettings.TranslationRequests != null && cultureSettings.TranslationRequests.Length > 0; } }

		public Sale CurrentSale {
			get {

				#if DEBUG
				//CultureSettings.Test();
				#endif

				if (cultureSettings == null || cultureSettings.Sales2 == null || cultureSettings.Sales2.Length == 0)
					return null;
				if (InstalledProductKind == ProductKind.Paid)
					return null;

				foreach (Sale sale in cultureSettings.Sales2) {
					if (sale.SessionIDStart > 0 || sale.SessionIDEnd > 0)
					if (this.SessionId >= sale.SessionIDStart && this.SessionId <= sale.SessionIDEnd)
						return sale;

					if (sale.DayIDStart > 0 || sale.DayIDEnd > 0)
					if (this.DaysSinceInstall >= sale.DayIDStart && this.DaysSinceInstall <= sale.DayIDEnd)
						return sale;

					if (sale.DateStart <= DateTime.Now && sale.DateEnd >= DateTime.Now)
						return sale;
				}

				return null;
			}
		}

		public FeedbackSettings FeedbackSettings {
			get {
				if (cultureSettings == null || cultureSettings.FeedbackSettings == null || cultureSettings.FeedbackSettings.Length == 0)
					return FeedbackSettings.Default;

				foreach (FeedbackSettings feedback in cultureSettings.FeedbackSettings)
					if (feedback.DaysTillStart < this.DaysSinceInstall)
						return feedback;

				return FeedbackSettings.Default;
			}
		}

		public bool UseAlternativeAds {
			get {
				if (cultureSettings == null || cultureSettings.AdProviders == null || cultureSettings.AdProviders.Length == 0)
					return false;
				foreach (AdProvider provider in cultureSettings.AdProviders)
					if (provider == AdProvider.Secondary)
						return true;
				return false;
			}
		}

		public string ExpiredVersion {
			get {
				if (cultureSettings == null || cultureSettings.ExpiredVersion == null)
					return new AssemblyName (Assembly.GetExecutingAssembly ().FullName).Version.ToString ();
				return cultureSettings.ExpiredVersion;
			}
		}

		public int ExpiredMonths {
			get {
				if (cultureSettings == null || cultureSettings.ExpiredMonths == 0)
					return 6;
				return cultureSettings.ExpiredMonths;
			}
		}


		private bool deDupeDone;

		[DataMember (Order = 25)]
		public bool DeDupeDone {
			get { return deDupeDone; }
			set {
				deDupeDone = value;
				Save ();
			}
		}

		#endregion

		#region UserUniqueID, DeviceInfo

		public static string GetWindowsLiveAnonymousID ()
		{
			string result = string.Empty;
			#if WINDOWS_PHONE
			object anid;
			if (Microsoft.Phone.Info.UserExtendedProperties.TryGetValue("ANID2", out anid))
			if (anid != null)
			return anid.ToString();
			#elif ANDROID
			try {
				result = Android.OS.Build.Serial + "-" + Android.Provider.Settings.Secure.GetString (Android.App.Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
			} catch {
			}
			;
			#endif
			return result;
		}

		public bool IsDarkTheme {
			get {
				#if WINDOWS_PHONE
				return (FoodJournal.WinPhone.App.Current.Resources["PhoneBackgroundColor"]).ToString().EndsWith("00");
				#else
				return false;
				#endif
			}
		}

		public static string GetDeviceInfo ()
		{
			try {
				StringBuilder info = new StringBuilder ();
				info.AppendLine ("Device:");
				info.AppendLine ("  Platform:" + Environment.OSVersion.Platform.ToString ());
				info.AppendLine ("  Version:" + Environment.OSVersion.Version);
				#if WINDOWS_PHONE
				info.AppendLine("  ApplicationCurrentMemoryUsage:" + Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage);
				info.AppendLine("  ApplicationMemoryUsageLimit:" + Microsoft.Phone.Info.DeviceStatus.ApplicationMemoryUsageLimit);
				info.AppendLine("  ApplicationPeakMemoryUsage:" + Microsoft.Phone.Info.DeviceStatus.ApplicationPeakMemoryUsage);
				info.AppendLine("  DeviceFirmwareVersion:" + Microsoft.Phone.Info.DeviceStatus.DeviceFirmwareVersion);
				info.AppendLine("  DeviceHardwareVersion:" + Microsoft.Phone.Info.DeviceStatus.DeviceHardwareVersion);
				info.AppendLine("  DeviceManufacturer:" + Microsoft.Phone.Info.DeviceStatus.DeviceManufacturer);
				info.AppendLine("  DeviceName:" + Microsoft.Phone.Info.DeviceStatus.DeviceName);
				info.AppendLine("  DeviceTotalMemory:" + Microsoft.Phone.Info.DeviceStatus.DeviceTotalMemory);
				//info.AppendLine("  ProcessorCount:" + Environment.ProcessorCount);
				info.AppendLine("  IsKeyboardDeployed:" + Microsoft.Phone.Info.DeviceStatus.IsKeyboardDeployed);
				info.AppendLine("  IsKeyboardPresent:" + Microsoft.Phone.Info.DeviceStatus.IsKeyboardPresent);
				info.AppendLine("  PowerSource:" + Microsoft.Phone.Info.DeviceStatus.PowerSource);
				#endif
				info.AppendLine ("Locale:");
				info.AppendLine ("  EnglishName:" + Thread.CurrentThread.CurrentCulture.EnglishName);
				info.AppendLine ("  DisplayName:" + Thread.CurrentThread.CurrentCulture.DisplayName);
				info.AppendLine ("  Name:" + Thread.CurrentThread.CurrentCulture.Name);
				info.AppendLine ("  NativeName:" + Thread.CurrentThread.CurrentCulture.NativeName);
				info.AppendLine ("  TwoLetterISOLanguageName:" + Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
				info.AppendLine ("  UICulture.Name:" + Thread.CurrentThread.CurrentUICulture.Name);
				#if WINDOWS_PHONE
				info.AppendLine("  Timezone:" + TimeZoneInfo.Local.DisplayName);
				info.AppendLine("Theme:");
				info.AppendLine("  BackColor:" + (FoodJournal.WinPhone.App.Current.Resources["PhoneBackgroundColor"]).ToString());
				info.AppendLine("  AccentColor:" + (FoodJournal.WinPhone.App.Current.Resources["PhoneAccentColor"]).ToString());
				#endif
				return info.ToString ();
			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
				return "";
			}
		}

		#endregion

		private void GetInstalledProductKind ()
		{


			#if DEBUG
			InstalledProductKind = ProductKind.Free;//.Trial;
			#endif

			if (InstalledProductKind != ProductKind.unknown)
				return;

			#if SUPPORTTRIAL

			if (LicensedProducts != null && LicensedProducts.Length > 0) // legacy, free with ads
			{
			InstalledProductKind = ProductKind.Free;
			return;
			}

			try
			{
			var licenseInfo = new Microsoft.Phone.Marketplace.LicenseInformation();
			if (licenseInfo.IsTrial())
			{
			InstalledProductKind = ProductKind.Trial;
			}
			else
			{
			// if paid, we need to figure out if it was free (ads supported), or paid for (remove all ads) 

			var listing = await Windows.ApplicationModel.Store.CurrentApp.LoadListingInformationAsync();
			foreach (var letter in listing.FormattedPrice)
			if (letter != '0' && char.IsDigit(letter))
			{
			InstalledProductKind = ProductKind.Paid;
			SessionLog.CurrentScreen.RecordMilestone("Paid w/o trial", listing.FormattedPrice);
			}

			if (InstalledProductKind == ProductKind.unknown)
			InstalledProductKind = ProductKind.Free;

			}
			try
			{
			Save();
			}
			catch (Exception ex) { LittleWatson.ReportException(ex); }
			CheckPurchases();
			}
			catch (Exception ex)
			{
			LittleWatson.ReportException(ex);
			SessionLog.CurrentScreen.RecordMilestone("GetInstalled Product Exception: giveaway", ex.Message);
			InstalledProductKind = ProductKind.Paid;
			}
			#else
			InstalledProductKind = ProductKind.Free;
			#endif
		}

	}

}
