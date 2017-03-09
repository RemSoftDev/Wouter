using System;
using Android.Views;
using Android.Widget;

namespace FoodJournal.Android15
{
	public static class AndroidDebug
	{
		#if DEBUG
		public static void SetViewBorders(View root)
		{

//			ViewGroup viewGroup = root as ViewGroup;
//
//			if (viewGroup != null)
//				for (int i = 0; i < viewGroup.ChildCount; i++)
//					SetViewBorders (viewGroup.GetChildAt(i));
//
//			LinearLayout layout = root as LinearLayout;
//			if (layout != null)
//				layout.SetBackgroundResource (Resource.Drawable.border);
//
//			TextView edt = root as TextView;
//			if (edt != null)
//				edt.SetBackgroundResource (Resource.Drawable.border);
			
		}
		#endif

	}
}

