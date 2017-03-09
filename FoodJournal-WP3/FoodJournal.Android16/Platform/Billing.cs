using System;
using Xamarin.InAppBilling;

namespace FoodJournal.Android15
{
	public class Billing
	{

//		private InAppBillingServiceConnection _serviceConnection;
//		private IList<Product> _products;
//
//		public Billing ()
//		{
//		
//				// Create a new connection to the Google Play Service
//				_serviceConnection = new InAppBillingServiceConnection (this, publicKey);
//			_serviceConnection.OnConnected += () => {
//				// Load available products and any purchases
//					
//					_products = await _serviceConnection.BillingHandler.QueryInventoryAsync (new List<string> {
//						ReservedTestProductIDs.Purchased,
//						ReservedTestProductIDs.Canceled,
//						ReservedTestProductIDs.Refunded,
//						ReservedTestProductIDs.Unavailable
//					}, ItemType.Product);
//
//				// Were any products returned?
//				if (_products == null) {
//					// No, abort
//					return;
//				}
//				};
//
//			// Attempt to connect to the service
//			_serviceConnection.Connect ();
//		}
//
//		public void CheckPurchase() {
//
//			// Ask the open connection's billing handler to get any purchases
//			var purchases = _serviceConnection.BillingHandler.GetPurchases (ItemType.Product);
//		}
//
//		public void Buy()
//		{
//			// Ask the open connection's billing handler to purchase the selected product
//			_serviceConnection.BillingHandler.BuyProduct(_selectedProduct);
//		}
//
//		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
//		{
//			// Ask the open service connection's billing handler to process this request
//			_serviceConnection.BillingHandler.HandleActivityResult (requestCode, resultCode, data);
//
//			// TODO: Use a call back to update the purchased items
//			// or listen to the OnProductPurchased event to
//			// handle a successful purchase
//		}
//
//
//		protected override void OnDestroy () {
//
//			// Are we attached to the Google Play Service?
//			if (_serviceConnection != null) {
//				// Yes, disconnect
//				_serviceConnection.Disconnect ();
//			}
//
//			// Call base method
//			base.OnDestroy ();
//		}
			

	}
}

