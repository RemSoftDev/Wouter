using System.Linq; 
using System.Text; 
using Android.App; 
using Android.Content; 
using Android.OS; 
using Android.Runtime; 
using Android.Views; 
using Android.Widget; 
using Android.Util;
using FoodJournal.Android15.Activities;

namespace FoodJournal.Android15
{ 

//	[BroadcastReceiver] 
//	public class NotificationAlertReceiver : BroadcastReceiver 
//	{
//
//		public NotificationAlertReceiver()
//		{
//
//		}
//
//		public override void OnReceive(Context context, Intent intent)
//		{
//			PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);
//			PowerManager.WakeLock w1 = pm.NewWakeLock (WakeLockFlags.Partial, "NotificationReceiver");
//			w1.Acquire ();
//			var nMgr = (NotificationManager)context.GetSystemService (Context.NotificationService);
//			var notification = new Notification (Resource.Drawable.loadIcon, "Arrival");
//			var pendingIntent = PendingIntent.GetActivity (context, 0, new Intent (context, typeof(Splash)), 0);
//			notification.SetLatestEventInfo (context, "Arrival", "Arrival", pendingIntent);
//			nMgr.Notify (0, notification);
//			w1.Release ();
//		}
//
//		public static void CancelAlarm()
//		{ 
//			Context context = Application.Context;
//			Intent intent = new Intent(context, this.Class);
//			PendingIntent sender = PendingIntent.GetBroadcast (context, 0, intent, 0);
//			AlarmManager alarmManager = (AlarmManager) context.GetSystemService (Context.AlarmService);
//			alarmManager.Cancel (sender);
//		}
//
//		public void SetAlarm(int alertTime)
//		{
//			Context context = Application.Context;
//			long now = SystemClock.CurrentThreadTimeMillis();
//			AlarmManager am = (AlarmManager) context.GetSystemService (Context.AlarmService);
//			Intent intent = new Intent(context, this.Class);
//			PendingIntent pi = PendingIntent.GetBroadcast (context, 0, intent, 0);
//			am.Set (AlarmType.ElapsedRealtimeWakeup, now + ((long)(alertTime*10000)), pi);
//			//SystemClock.ElapsedRealtime() 
//		}
//	}

}
