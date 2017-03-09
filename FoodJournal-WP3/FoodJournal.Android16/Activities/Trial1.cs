using Android.App;
using Android.Graphics;
using Android.Text;
using Android.Widget;
using FoodJournal.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoodJournal.Android15
{
    [Activity(Label = "Trial1")]
    class Trial1 : Activity
    {
        protected override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.trial1);
            String a = Intent.GetStringExtra("TrialData1") ?? "Data not available";
            String b = Intent.GetStringExtra("TrialData2") ?? "Data not available";
            String c = Intent.GetStringExtra("TrialData3") ?? "Data not available";
            TextView t1 = FindViewById<Android.Widget.TextView>(Resource.Id.text1);
            var t2 = FindViewById<Android.Widget.TextView>(Resource.Id.text2);
            var i1 = FindViewById<Android.Widget.ImageView>(Resource.Id.cancel);

            i1.Click += delegate
            {
                Finish();
            };

            t2.Click += delegate
            {
                Finish();
                Navigate.ToBuyNowPage();
            };

            SpannableStringBuilder builder = new SpannableStringBuilder();
            SpannableString black = new SpannableString(a);
            black.SetSpan(new Android.Text.Style.ForegroundColorSpan(Color.Black), 0, black.Length(), 0);
            builder.Append(black);

            SpannableString blue = new SpannableString(b);
            blue.SetSpan(new Android.Text.Style.ForegroundColorSpan(Application.Context.Resources.GetColor(Resource.Color.trialcolor)), 0, blue.Length(), 0);
            builder.Append(blue);

            SpannableString black1 = new SpannableString(c);
            black1.SetSpan(new Android.Text.Style.ForegroundColorSpan(Color.Black), 0, black1.Length(), 0);
            builder.Append(black1);
            t1.SetText(builder, Android.Widget.TextView.BufferType.Spannable);            
        }
    }
}
