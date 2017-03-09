using FoodJournal.Android15.Activities;
using System.Collections.Generic;
using FoodJournal.AppModel;
using FoodJournal.Logging;
using Android.App;
using Android.Content;

#if AMAZON

namespace FoodJournal.Android15
{
    public class BillingWrapper
    {
    }
}

#else


using System;
using FoodJournal.Android15.Activities;
using Xamarin.InAppBilling;
using System.Collections.Generic;
using FoodJournal.AppModel;
using FoodJournal.Logging;
using Android.App;
using Android.Content;

namespace FoodJournal.Android15
{
	public class BillingWrapper
	{

		protected static InAppBillingServiceConnection _serviceConnection;
		protected static IList<Product> _products;
		protected static Product _selectedProduct;

		public bool DoDestroyNow=true; // I'm not sure how this can work?

#if V16
		private const string publicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArzpxxU46ccBCDMBx7imb1EtgBRykq16D7b/g9G7L64xV1xG3d+1zzW1nKG3Qdx3wF1nh5+S/Fi7nidI/8RoYN23vem4lcSK07TuJhLO/1cqFdsW7pxxFhXRnKGmis9C1fDi6JYtjmx34WpV6yLsPCAVy+RF3YJprjeaa96h54vlU/tPhcGt++r8WSkcx36okKzfa652SUWTWcwx35YjR+qVr37HJQF0Iih7XlytmbSoxnx7L4aWFPWodGWS9PoFpbRYKKlG5PdjXPj5WGt2M/iJNmKyEqbVak3mEqpH38g8u6vd3u0eaoUxC9pPZxX6R70IAK4c6xcU6K6J3bxoQYQIDAQAB";
#else
		private const string publicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0fNJmnWaAvxsmpnSEjjfaaO9imV3cxFz6dhWtbvgWDa558lpYV7JEvTBSkOVKLRu4QkiITgwU74143I8TMkWuPn2DQc3ve0xPBem9hXJKHNI2rWjvDjk7ReN9ruaRFc+gndnELfZBAGjfoY7ur6nDFQlxze3IfvN1dzUM2c6R1p2jOV6GC5w6RDy8J/+x7kShYuzBw66doRQ2VAl4iwbLShe6ALdDztiHrFFdwADl8R0i9pNs9DzwWEXuHPa780eYqhKHd3lpB+a8AJh1f4wKlm9jjZrku9TB1sSa1xkSjhtHAUbPtTneELfilfOQy/xNpnVwppH6vbO94R6ylRApQIDAQAB";
#endif

		public OnConnectedCall OnConnected;
		public OnProductPurchasedCall OnProductPurchased;
		public OnProductFoundCall OnProductFound;

		public delegate void OnConnectedCall();
		public delegate void OnProductPurchasedCall(string ProductName);
		public delegate void OnProductFoundCall(string Key, string Title, string Price);

		public EventHandler OnProductConsumed;

        private Activity context { get; set; }
		public BillingWrapper(Activity activity)
		{
            this.context = activity;
		}

        public void Connect()
        {
            try
            {
                // note: serviceconnection is static. it may have already been initialized by another billingactivity
                // (ie, first in "feedback")
                if (_serviceConnection == null && AppStats.Current.InstalledProductKind != AppStats.ProductKind.Paid)
                {// Create a new connection to the Google Play Service
                    _serviceConnection = new InAppBillingServiceConnection(context, publicKey);
                    _serviceConnection.OnConnected += HandleOnConnected;
                    _serviceConnection.OnInAppBillingError += HandleOnInAppBillingError;

                    // Attempt to connect to the service
                    _serviceConnection.Connect();
                }
                else if (AppStats.Current.InstalledProductKind == AppStats.ProductKind.Paid)
                {
                    if (OnProductPurchased != null)
                      OnProductPurchased("");
                }
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
        }

		public void ConsumePurchase()
		{
			if (_serviceConnection == null)
			{
				_serviceConnection = new InAppBillingServiceConnection(context, publicKey);
				_serviceConnection.OnConnected += () => {
					var purchaseToken = "inapp:" + context.PackageName + ":android.test.purchased";
					var response = _serviceConnection.BillingHandler.ConsumePurchase (purchaseToken);

					AppStats.Current.UnRegisterPurchase ();
					if(OnProductConsumed!=null)
					{
						OnProductConsumed(this,EventArgs.Empty);
					}
				};
				_serviceConnection.Connect();
			}
		}

		async void HandleOnConnected ()
		{

			try {
				// Load available products and any purchases

				//Platform.MessageBox ("Connected");
				if (OnConnected != null)
					OnConnected();

				_products = await _serviceConnection.BillingHandler.QueryInventoryAsync (new List<string> {
					"inapp"
				}, ItemType.Product);
				
				var purchases = _serviceConnection.BillingHandler.GetPurchases ("inapp");

				//Platform.MessageBox ("Inventory Queried");

				_serviceConnection.BillingHandler.OnProductPurchased += HandleOnProductPurchased;
				// Were any products returned?
				if (_products == null) {
					// No, abort
					return;
				}

                foreach (var pup in purchases)
                {
                    AppStats.Current.RegisterPurchase(pup.ProductId);
                    if (OnProductPurchased != null)
                    OnProductPurchased("");
                }
				foreach (Product p in _products) {
					if (OnProductFound != null)
						OnProductFound (p.ProductId, p.Title, p.Price);
					_selectedProduct = p;
				}
			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}
		}

		void HandleOnInAppBillingError (InAppBillingErrorType error, string message)
		{
            FoodJournal.Runtime.Navigate.dialogContext = context;
			//Platform.MessageBox ("Billingerror: " + error.ToString () + " - " + message);
		}

		void HandleOnProductPurchased (int response, Purchase purchase, string purchaseData, string purchaseSignature)
		{

			//Platform.MessageBox ("Product purchased");
			try {
				SessionLog.RecordMilestone (purchase.ProductId + " Purchased", AppStats.Current.SessionId.ToString ());

				OnProductPurchased (purchase.ProductId);
			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}

		}

		public void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			//Platform.MessageBox ("ActivityResult: " + resultCode.ToString());
			// Ask the open service connection's billing handler to process this request

			try {
				if (_serviceConnection != null)
					_serviceConnection.BillingHandler.HandleActivityResult (requestCode, resultCode, data);

			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}

			// TODO: Use a call back to update the purchased items
			// or listen to the OnProductPurchased event to
			// handle a successful purchase
		}

		public void OnDestroy ()
		{

			try {
				// Are we attached to the Google Play Service?
				if (DoDestroyNow && _serviceConnection != null) {
					// Yes, disconnect
					_serviceConnection.Disconnect ();
					_serviceConnection = null;
				}
			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}

		}

	}
}

#endif