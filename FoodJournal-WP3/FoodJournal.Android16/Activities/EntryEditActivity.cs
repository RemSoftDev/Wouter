using System;
using FoodJournal.Android15.Adapters;
using FoodJournal.ViewModels.Fragments;
using FoodJournal.Runtime;
using FoodJournal.ViewModels;
using FoodJournal.Resources;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Gms.Ads;
using FoodJournal.AppModel;
using System.Threading.Tasks;
using FoodJournal.Logging;

namespace FoodJournal.Android15.Activities
{
	[Android.App.Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class EntryEditActivity : AppCompatActivity
    {
        private EntryDetailVM vm;
        private const int MAXSERVINGSFIRST = 3;
        AdView _bannerad;

        private LinearLayout advertisements;
        private Android.Widget.LinearLayout listEntryEdit;
        private EditText addInputDialog;
        IMenu Menu { get; set; }
        bool EditMode = false;
        string OriginalText = string.Empty;

        protected override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_entrydetail);
            OverridePendingTransition(Resource.Animation.zoom_enter, Resource.Animation.zoom_exit);
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar_addItem);
            SetSupportActionBar(toolbar);
			this.SupportActionBar.Title = AppResources.PageTitleItemDetail;
			this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            listEntryEdit = FindViewById<Android.Widget.LinearLayout>(Resource.Id.contentView);
            var e = Navigate.selectedEntry;
            vm = new EntryDetailVM(e);
            var view = listEntryEdit;

#if SCREENSHOT
            Navigate.screenshotScreen = this;
#endif

            var binding = DataContext<EntryDetailVM>.FromView(view);
            binding.VM = vm;
            listEntryEdit.AddView(View.Inflate(this, Resource.Layout.item_edit_entry_top, null));
            AmountCollectionVM avm = vm.AmountCollectionVM;
            ServingSizeAdapter a = new ServingSizeAdapter(this, avm);

            // serving sizes
            listEntryEdit.AddView(View.Inflate(this, Resource.Layout.item_serving, null));
            var ServingExtraMenu = FindViewById(Resource.Id.addItemDialog);
            ServingExtraMenu.Click += (object sender, EventArgs e2) =>
            {
                ShowServingsExtraMenu();
            };

            var servingsizes = FindViewById<Android.Widget.LinearLayout>(Resource.Id.servingSizes);
            int cnt = Math.Min(a.Count, MAXSERVINGSFIRST);
            for (int i = 0; i < cnt; i++)
                servingsizes.AddView(a.GetView2(i, null, servingsizes));

            View showmore = null;
            if (a.Count > MAXSERVINGSFIRST)
            {
                showmore = View.Inflate(this, Resource.Layout.item_servings_show_more, null);
                listEntryEdit.AddView(showmore);
                var button = FindViewById<TextView>(Resource.Id.button_showmoreoptions);
                button.Click += (sender, e2) =>
                {
                    servingsizes.RemoveAllViews();
                    for (int i = 0; i < a.Count; i++)
                        servingsizes.AddView(a.GetView2(i, null, servingsizes));
                    showmore.Visibility = ViewStates.Invisible;
                };
            }
            
            // add serving szie
            avm.Amounts.CollectionChanged += (sender, e3) =>
            {
                if (showmore != null) showmore.Visibility = ViewStates.Invisible;
                servingsizes.RemoveAllViews();
                for (int i = 0; i < a.Count; i++)
                    servingsizes.AddView(a.GetView2(i, null, servingsizes));
            };

            PropertyAdapter p = new PropertyAdapter(this, vm);
            
            // properties
            listEntryEdit.AddView(View.Inflate(this, Resource.Layout.item_nutrition, null));
            var properties = FindViewById<Android.Widget.LinearLayout>(Resource.Id.properties);
            for (int i = 0; i < p.Count; i++)
                properties.AddView(p.GetView2(i, null, properties));

            Action action = () =>
            {
                servingsizes.RemoveAllViewsInLayout();
                for (int i = 0; i < a.Count; i++)
                    servingsizes.AddView(a.GetView(i, null, servingsizes));
            };
            
            a.RegisterDataSetObserver(new DSO(action));

            if (binding.Bindings.Count == 0)
            {
                binding.Add(Resource.Id.addInputDialog, x => x.Text);
                binding.Add(Resource.Id.opt_pergrams, x => x.Is100Gram);
                binding.Add(Resource.Id.opt_percup, x => x.IsNot100Gram);
                binding.Add(Resource.Id.opt_percup, x => x.SelectedServingText);

            }
            addInputDialog = listEntryEdit.FindViewById<EditText>(Resource.Id.addInputDialog);
            if (Navigate.selectedEntry.Item.IsNewItem)
            {
                addInputDialog.TextChanged += AddInputDialogTextChanged;
            }
            else
            {
                EditMode = true;
            }
            OriginalText = Navigate.selectedItem.Text;
#if DEBUG
            AndroidDebug.SetViewBorders(listEntryEdit);
#endif 
        }

        private void AddInputDialogTextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (Menu != null)
            {
                var text = e.Text.ToString();
                var item = Menu.FindItem(Resource.Id.action_done);
                if (!EditMode)
                {
                    if (!string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text))
                    {
                        item.SetVisible(true);
                    }
                    else
                    {
                        item.SetVisible(false);
                    }
                }
            }
        }

		public override bool OnCreateOptionsMenu(IMenu menu)
		{

			try{
				// Inflate the menu; this adds items to the action bar if it is present.
				if (!EditMode)
					MenuInflater.Inflate(Resource.Menu.menu_add_item, menu);
				Menu = menu;
				if (addInputDialog.Text.Length == 0)
					menu.FindItem(Resource.Id.action_done).SetVisible(false);
			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}
			return true;
		}

        public void ShowServingsExtraMenu()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(SupportActionBar.ThemedContext);
            builder.Create();
#if DEBUG1
            //  string[] items = new String[] { AppResources.Add, "Copy from similar" };
            string[] items = new String[] { AppResources.Add };
#endif

            EventHandler<Android.Content.DialogClickEventArgs> eh;
            eh = (o, ex) =>
            {
                switch (ex.Which)
                {
                    case 0:
                        vm.AddServingSize();
                        break;
                    case 1:
                        ShowAddDialog();
                        break;
                    default:
                        break;
                }
            };
            builder.SetItems(items, eh);
            builder.Show();
        }

        public void ShowAddDialog()
        {
            View dialogContent = View.Inflate(this, Resource.Layout.dialog_copy_from_similar, null);
            AlertDialog.Builder builder = new AlertDialog.Builder(SupportActionBar.ThemedContext, Resource.Style.Dialog);
            builder.SetView(dialogContent);
            var dialogList = dialogContent.FindViewById<Android.Support.V7.Widget.RecyclerView>(Resource.Id.recycler_dialog_copy_from_similar);
            dialogList.HasFixedSize = false;
            LinearLayoutManager mLayoutManager = new LinearLayoutManager(this);
            dialogList.SetLayoutManager(mLayoutManager);

#if DEBUG1
            SearchVM vm = new SearchVM(Values.Period.Dinner);
            //vm.Query="Banana";
#endif

            var addInputDialog = dialogContent.FindViewById<Android.Widget.EditText>(Resource.Id.addInputDialog);
            addInputDialog.TextChanged += (sender, e) =>
            {
                vm.Query = addInputDialog.Text;
            };

            // create an empty cardsAdapter and add it to the recycler view
            var cardsAdapter = new SearchResultRecyclerAdapter(this, vm, SearchResultRecyclerAdapter.SRCType.ServingSizes);
            dialogList.SetAdapter(cardsAdapter);
            var dialog = builder.Create();
            cardsAdapter.Click = (svm) =>
            {
                dialog.Dismiss();
            };
            dialog.Show();
        }

        public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
        {
            // switch the menu item click and handle accordingly
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                case Resource.Id.action_done:
                    vm.CommitNewItem();
                    Finish();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnBackPressed()
        {
            try {
                if (EditMode)
                {
                    addInputDialog = listEntryEdit.FindViewById<EditText>(Resource.Id.addInputDialog);
                    if (addInputDialog.Text.Length == 0)
                    {
                        confirmDelete();
                        //GrabAdIntertitials();
                    }
                    else
                    {
                        Android.Views.InputMethods.InputMethodManager inputMethodManager = (Android.Views.InputMethods.InputMethodManager)this.GetSystemService(MainActivity.InputMethodService);
                        inputMethodManager.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, 0);
                        Finish();
                        //GrabAdIntertitials();
                    }
                }
                else
                {
                    base.OnBackPressed();
                }
            } catch (Exception ex) { LittleWatson.ReportException(ex); }
        }
        
        void confirmDelete()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(SupportActionBar.ThemedContext);
            builder.Create();
            builder.SetTitle(Resource.String.DeleteEntry);
            builder.SetMessage(Resource.String.DeleteEntryMessage);
            builder.SetPositiveButton(Resource.String.Ok, (sender,e) => {
                Navigate.selectedEntry.Delete();
                Finish();
            });
            builder.SetNegativeButton(Resource.String.Cancel, (sender,e) => {
                addInputDialog.Text =OriginalText;
            });
            builder.Show();
        }
       
        public class DSO : Android.Database.DataSetObserver
        {
            private Action action;

            public DSO(Action action)
            {
                this.action = action;
            }

            public override void OnChanged()
            {
                base.OnChanged();
                action.Invoke();
            }
        }

        #if INTERSTITIAL
        public async void GrabAdIntertitials()
        {
            bannerView();
        }
        public async Task bannerView()
        {
            //----------------------------------------------banner add stuff
			if (!AppStats.Current.ShouldShowInterstitials) return;

            advertisements = FindViewById<LinearLayout>(Resource.Id.advertisement);
            Context con = advertisements.Context;
            {
                _bannerad = AdWrapper.ConstructStandardBanner(con, AdSize.SmartBanner, "ca-app-pub-3167302081266616/1445459882");
                var listener = new adlistener();
                listener.AdLoaded += () => { };
                _bannerad.AdListener = listener;
                _bannerad.CustomBuild();

                var layout = FindViewById<LinearLayout>(Resource.Id.adbox);
                layout.AddView(_bannerad);
                var FinalAd = AdWrapper.ConstructFullPageAdd(con, "ca-app-pub-3167302081266616/1445459882");
                var intlistener = new adlistener();
                intlistener.AdLoaded += () => { if (FinalAd.IsLoaded)FinalAd.Show(); };
                FinalAd.AdListener = intlistener;
                FinalAd.CustomBuild();
            }
        }
#endif
    }
}

