using System;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Widget;
using Android.Views;
using Android.App;
using FoodJournal.Android15.Activities;
using FoodJournal.Model;
using FoodJournal.Runtime;
using Android.Views.InputMethods;
using FoodJournal.ViewModels;
using Android.Text;
using FoodJournal.Values;
using Android.Text.Style;
using FoodJournal.Logging;

namespace FoodJournal.Android15
{
	public class TodayAdapter : RecyclerView.Adapter
	{

		public List<Entry> cardList;
		private Context context;
		public PeriodDeleteVM periodDeleteVM;

		public TodayAdapter (Context context, List<Entry> contactList, PeriodDeleteVM periodDeleteVM)
		{
			this.cardList = new List<Entry> ();
			cardList.AddRange (contactList);
			this.context = context;
			this.periodDeleteVM = periodDeleteVM;
		}

		public Entry getItem (int position)
		{
			return cardList [position];
		}

		public override int ItemCount { get { return cardList.Count; } }

		public void remove (int position)
		{
			cardList.RemoveAt (position);
			NotifyItemRemoved (position);
		}

		public void add (Entry cardModel, int position)
		{
			cardList.Insert (position, cardModel);
			NotifyItemInserted (position);
		}

		public int getSelectedCount ()
		{
			return cardList.Count;
		}


		public override void OnBindViewHolder (RecyclerView.ViewHolder holder, int position)
		{

            try {
                ContactViewHolder contactViewHolder = (ContactViewHolder)holder;
                /*CardModel ci = cardList.get(i);
                contactViewHolder.title.setText(ci.test);*/

                Entry t = cardList[position];
                contactViewHolder.entry = t;
                contactViewHolder.title.SetText(t.Text, TextView.BufferType.Normal);
                contactViewHolder.size.SetText(t.TotalAmountText, TextView.BufferType.Normal);
                contactViewHolder.fab.Visibility = (periodDeleteVM.IsSelected(t)) ? ViewStates.Visible : ViewStates.Gone;

                var text = MakeColoredText(t);
                contactViewHolder.calories.SetText(text, TextView.BufferType.Spannable);
                contactViewHolder.calories.Visibility = (text.Length() == 0) ? ViewStates.Gone : ViewStates.Visible;
            } catch (Exception ex) { LittleWatson.ReportException(ex); }
		}


		public SpannableStringBuilder MakeColoredText (Entry entry)
		{

			SpannableStringBuilder spannable = new SpannableStringBuilder ();

			int i = 0;
			foreach (Property p in UserSettings.Current.SelectedProperties) {

				try {
					Amount a = entry.GetPropertyValue (p);
					string value = a.ValueString ();

					if (!string.IsNullOrEmpty (value)) {
						if (i > 0)
							spannable.Append (", ");

						spannable.Append (p.TextOnly);

						spannable.Append (" - ");

						var span = new ForegroundColorSpan (AndroidUI.GetPropertyColor (context, i, p));
						spannable.Append (value);

						spannable.SetSpan (span, spannable.Length () - value.Length, spannable.Length (), SpanTypes.ExclusiveExclusive);
					}

					i++;
				} catch (Exception ex) {
					LittleWatson.ReportException (ex);
				}
			}
			
			return spannable;
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder (Android.Views.ViewGroup parent, int viewType)
		{
			View itemView = LayoutInflater.From (parent.Context).Inflate (Resource.Layout.item_today_card, parent, false);

			var contactViewHolder = new ContactViewHolder (itemView);
			contactViewHolder.fab.Click += (sender, e) => {
				Entry t = contactViewHolder.entry;
				periodDeleteVM.SetSelected (t, false);
				contactViewHolder.fab.Visibility = ViewStates.Gone;
			};

			contactViewHolder.linearLayout.Click += (object sender, EventArgs e) => {
				Entry t = contactViewHolder.entry;
				if (periodDeleteVM.InDeleteMode) {
					periodDeleteVM.SetSelected (t, !periodDeleteVM.IsSelected (t));
					contactViewHolder.fab.Visibility = (periodDeleteVM.IsSelected (t)) ? ViewStates.Visible : ViewStates.Gone;					
				} else
					Navigate.ToEntryDetail (t);
			};

			//			//handle long click listener
			contactViewHolder.linearLayout.LongClick += (sender, e) => {
				Entry t = contactViewHolder.entry;
				periodDeleteVM.Period = t.Period;
				periodDeleteVM.InDeleteMode = true;
				//itemLongClick.onItemLongClick(cardList.get(i).Text);
				periodDeleteVM.SetSelected (t, true);
				contactViewHolder.fab.Visibility = ViewStates.Visible;
				InputMethodManager imm = (InputMethodManager)context.GetSystemService (Context.InputMethodService);
				imm.HideSoftInputFromWindow ((sender as View).WindowToken, 0);
			};

			#if DEBUG
			AndroidDebug.SetViewBorders (itemView);
			#endif

			return contactViewHolder;
		}

		public class ContactViewHolder : RecyclerView.ViewHolder
		{

			public TextView title;
			public FloatingActionButton fab;
			public LinearLayout linearLayout;
			public TextView calories;
			public TextView size;
			public Entry entry;

			public ContactViewHolder (View v) : base (v)
			{
				title = v.FindViewById<TextView> (Resource.Id.text_title);
				fab = v.FindViewById<FloatingActionButton> (Resource.Id.fab_cartItem);
				linearLayout = v.FindViewById<LinearLayout> (Resource.Id.card_layout);
				calories = v.FindViewById<TextView> (Resource.Id.text_calories);
				size = v.FindViewById<TextView> (Resource.Id.text_size);
			}
		}
	}

}

