using System;
using FoodJournal.Extensions;
using Android.Support.V4.App;
using Android.Support.V7.App;
using FoodJournal.Runtime;
using System.Threading.Tasks;

namespace FoodJournal.Android15.Fragments.AppFragments
{
   public class BaseViewFragment : Fragment
    {
        public string Title { get; set; }

       
        public override void OnResume()
        {
            base.OnResume();
            ((AppCompatActivity)Activity).SupportActionBar.Title = Title;
        }
    }
}