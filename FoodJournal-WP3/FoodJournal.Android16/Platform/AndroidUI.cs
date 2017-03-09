using System;
using FoodJournal.Values;
using Android.Content.Res ;
using Android.Content;

namespace FoodJournal.Android15
{
	public class AndroidUI
	{
		public static Android.Graphics.Color GetPropertyColor (Context context, int id, Property p)
		{
			
			switch (id) {
			case 0:
				return context.Resources.GetColor (Resource.Color.text_calories);
			case 1:
				return context.Resources.GetColor (Resource.Color.text_fat);
			case 2:
				return context.Resources.GetColor (Resource.Color.text_carbs);
			case 3:
				return context.Resources.GetColor (Resource.Color.text_protein);
			default:
				return context.Resources.GetColor (Resource.Color.text_other);
			}

		}
	}
}

