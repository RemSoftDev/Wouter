using Android.Content;
using Android.Views;
using Android.Widget;
using FoodJournal.ViewModels.Fragments;
using System.Linq;
namespace FoodJournal.Android15.Adapters
{
    public class ServingSizeAdapter : BaseAdapter<AmountVM>
    {
        protected Context _context = null;
        private AmountCollectionVM _collection;

        public ServingSizeAdapter(Context context, AmountCollectionVM collection)
            : base()
        {
            _context = context;
            var temp = collection.Amounts.OrderByDescending(x => x.IsChecked).ToList();
            collection.Amounts.Clear();
            foreach (var item in temp)
            {
                collection.Amounts.Add(item);
            }
            _collection = collection;
            _collection.PropertyChanged += (sender, e) => { if (e.PropertyName == "Amounts") this.NotifyDataSetChanged(); };
        }

        public override AmountVM this[int position] { get { return _collection.Amounts[position]; } }
        public override long GetItemId(int position) { return position; }
        public override int Count { get { return _collection.Amounts.Count; } }

        public View GetView2(int position, View convertView, ViewGroup parent)
        {
            AmountVM vm = this[position];
            View view = convertView;
            if (view == null)
            {
                view = View.Inflate(_context, Resource.Layout.item_servings_dynamic_item, null);
            }

            var binding = DataContext<AmountVM>.FromView(view);
            binding.VM = vm;

            if (binding.Bindings.Count == 0)
            {
                binding.Add(Resource.Id.opt_serving_selected, x => x.DeleteVisibility);
                binding.Add(Resource.Id.opt_serving_selected, x => x.IsChecked);
                binding.Add(Resource.Id.opt_serving_selected, x => x.NotCheckedText);
                binding.Add(Resource.Id.edit_text_serving_cup, x => x.Text);
                binding.Add(Resource.Id.edit_text_serving_cup, x => x.CheckedVisibility);
                binding.Add(Resource.Id.edit_text_serving_gram, x => x.EquivalentNumber);
                binding.Add(Resource.Id.edit_text_serving_gram, x => x.CheckedVisibility);
                binding.Add(Resource.Id.btn_delete, x => x.DeleteVisibility);
                binding.Add(Resource.Id.btn_delete, (x) => { x.DeleteAmount(); });
                binding.Add(Resource.Id.SliderValue, x => x.CheckedVisibility);
                binding.Add(Resource.Id.SliderValue, x => x.AndroidSliderValue, "Progress");
                binding.Add(Resource.Id.SliderValue, (x) => { x.OnSliderLostMouseCapture(); }, "StopTrackingTouch");

                if (view == null) {
                	view.FindViewById<RadioButton> (Resource.Id.SliderValue);
                    view.FindViewById<SeekBar> (Resource.Id.SliderValue).Progress = 100;
                    var value = view.FindViewById<SeekBar> (Resource.Id.SliderValue).Progress;
                    view.FindViewById<SeekBar> (Resource.Id.SliderValue).ProgressChanged += (sender, e) =>  {view = null;};
                    view.FindViewById<SeekBar> (Resource.Id.SliderValue).StopTrackingTouch += (sender, e) =>  {view = null;};
                } 
                var radio = view.FindViewById<RadioButton>(Resource.Id.opt_serving_selected);

                if (!radio.Checked)
                {
                    LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent);
                    param.Weight = 3f;
                    radio.LayoutParameters = param;
                    view.FindViewById<ImageView>(Resource.Id.btn_delete).Visibility = ViewStates.Gone;
                }

                radio.CheckedChange += (sender, e) =>
                {
                    LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent);
                    if (e.IsChecked)
                    {
                        param.Weight = 0.08f;
                        view.FindViewById<ImageView>(Resource.Id.btn_delete).Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        param.Weight = 3f;
                        view.FindViewById<ImageView>(Resource.Id.btn_delete).Visibility = ViewStates.Gone;
                    }
                    ((RadioButton)sender).LayoutParameters = param;
                };
            }
            return view;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            return GetView2(position, convertView, parent);
        }
    }
}