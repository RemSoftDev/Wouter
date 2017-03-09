using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FoodJournal.Model;
using System.Collections.ObjectModel;
using FoodJournal.ViewModels;
using Android.Support.V7.Widget;
using FoodJournal.Values;

namespace FoodJournal.Android15.Adapters
{

    public class ViewHolder : RecyclerView.ViewHolder
    {
        public SearchResultVM svm;
        public int SVMPosition;
        public ViewHolder(View view)
            : base(view)
        { view.Tag = this; }
    }

    public class SearchResultRecyclerAdapter : RecyclerView.Adapter
    {

        public enum SRCType
        {
            Search,
            Properties,
            ServingSizes
        }

        protected Context _context = null;
        private SearchVM _container;
        private PeriodVM _emptyVM;
        private ObservableCollection<SearchResultVM> collection;
        private SRCType _type;
        public OnEvent Click;
        public OnEvent LongClick;

        public delegate void OnEvent(SearchResultVM vm);

        public SearchResultRecyclerAdapter(Context context, SearchVM container, SRCType type)
            : base()
        {
            _context = context;
            _container = container;
            _container.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Results")
                    if (String.IsNullOrEmpty(_container.Query))
                        Collection = _emptyVM.ItemList;
                    else
                        Collection = _container.Results;
            };
#if !DEBUG1

#endif
            _emptyVM = new PeriodVM(DateTime.Now.Date, container.Period);
            _emptyVM.PropertyChanged += (sender, e) => { if (e.PropertyName == "ItemList") Collection = _emptyVM.ItemList; };
            _emptyVM.StartRequery();
            _type = type;
            Collection = _emptyVM.ItemList;
        }

        private ObservableCollection<SearchResultVM> Collection
        {
            set
            {
                if (collection == value)
                    return;
                if (collection != null)
                    collection.CollectionChanged -= HandleCollectionChanged;
                collection = value;
                if (collection != null)
                    collection.CollectionChanged += HandleCollectionChanged;
                this.NotifyDataSetChanged();
            }
        }

        void HandleCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.NotifyDataSetChanged();
        }

        public SearchResultVM this[int position] { get { return collection == null ? null : collection[position]; } }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int ItemCount { get { return collection == null ? 0 : collection.Count; } }

        public override int GetItemViewType(int position)
        {
            return this[position] is SearchResultHeaderVM ? 1 : 2;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            if (viewType == 1)
                return new ViewHolder(View.Inflate(_context, Resource.Layout.item_dialog_section, null));

            switch (_type)
            {
                case SRCType.Search:
                    var view = View.Inflate(_context, Resource.Layout.item_dialog, null);
                    if (Click != null) view.Click += (a, b) =>
                    {
                        this.NotifyItemRemoved((int)((ViewHolder)view.Tag).SVMPosition);
                        Click(((ViewHolder)view.Tag).svm);
                    };
                    if (LongClick != null) view.LongClick += (a, b) => { LongClick(((ViewHolder)view.Tag).svm); };
                    return new ViewHolder(view);
                case SRCType.Properties:
                case SRCType.ServingSizes:
                    return new ViewHolder(View.Inflate(_context, Resource.Layout.item_dialog_copy, null));
            }
            return null;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            SearchResultVM vm = this[position];
            View view = holder.ItemView;
            ((ViewHolder)holder).svm = vm;
            ((ViewHolder)holder).SVMPosition = position;

            if (holder.ItemViewType == 1)
            {
                view.FindViewById<TextView>(Resource.Id.captionAccent).SetText(vm.CaptionAccent, TextView.BufferType.Normal);
                return;
            }

            switch (_type)
            {
                case SRCType.Search:
                    if (FoodJournal.AppModel.AppStats.Current.PremiumItemsLocked)
                    {
                        if (vm.IsLocked)
                        {
                            view.FindViewById<ImageView>(Resource.Id.lockicon).Visibility = ViewStates.Visible;
                            view.FindViewById<TextView>(Resource.Id.text).SetTextColor(view.Context.Resources.GetColor(Resource.Color.SubtleTextColor));
                        }
                        else
                        {
                            view.FindViewById<ImageView>(Resource.Id.lockicon).Visibility = ViewStates.Gone;
                            view.FindViewById<TextView>(Resource.Id.text).SetTextColor(view.Context.Resources.GetColor(Resource.Color.NormalTextColor));
                        }
                        var listener = vm.Listener as SearchVM;

                        if(listener!=null && string.IsNullOrEmpty(listener.Query)==false)
                        {
                            RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
                            lp.SetMargins(25, 0, 0, 0);
                            view.FindViewById<TextView>(Resource.Id.text).LayoutParameters = lp;
                        }
                        else
                        {
                            RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
                            lp.SetMargins(0, 0, 0, 0);
                            view.FindViewById<TextView>(Resource.Id.text).LayoutParameters = lp;
                        }                            
                    }
                    else
                    {
                        view.FindViewById<ImageView>(Resource.Id.lockicon).Visibility = ViewStates.Gone;
                    }
                    view.FindViewById<TextView>(Resource.Id.text).SetText(vm.Text, TextView.BufferType.Normal);

                    break;
                case SRCType.Properties:
                case SRCType.ServingSizes:

                    view.FindViewById<TextView>(Resource.Id.text_title_dialog_copy).SetText(vm.Text, TextView.BufferType.Normal);
                    var item = vm.MakeItem();
                    var line = view.FindViewById<LinearLayout>(Resource.Id.layout_size_dialog_copy);
                    line.RemoveAllViews();
                    int i = 0;
                    foreach (var SS in item.ServingSizes.Amounts)
                        if (SS.amount2.IsValid)
                        {
                            var newview = View.Inflate(_context, Resource.Layout.item_dialog_copy_line, null);
                            newview.FindViewById<TextView>(Resource.Id.text_size).SetText(SS.amount1.ToString(true), TextView.BufferType.Normal);
                            newview.FindViewById<TextView>(Resource.Id.text_size2).SetText(SS.amount2.ValueString(), TextView.BufferType.Normal);
                            line.AddView(newview);
                            if (i++ > 3)
                                break;
                        }
                    break;
            }
        }
    }
}