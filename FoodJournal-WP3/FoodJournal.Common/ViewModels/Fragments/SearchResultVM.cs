using FoodJournal.Extensions;
using FoodJournal.Model;
using FoodJournal.Values;
using System.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using FoodJournal.Runtime;

namespace FoodJournal.ViewModels
{

    public interface IAcceptsSearchResult
    {
        void OnItemTap(SearchResultVM result);
    }

    #region class SortedResultsComparer

    /// <summary>
    /// Used for search result accuracy comparing
    /// </summary>
    public class SortedResultsComparer : IComparer<SearchResultVM>
    {
        public int Compare(SearchResultVM x, SearchResultVM y)
        {
            if (x == null) return -1;
            if (y == null) return 1;
            var i = x.AccuracyScore.CompareTo(y.AccuracyScore);
            if (i == 0) return string.Compare(x.Text, y.Text);
            return i;
        }
    }

    #endregion

    #region class SearchResultHeaderVM

    public class SearchResultHeaderVM : SearchResultVM
    {
        public SearchResultHeaderVM(string Header, Single pos) : base(null) { CaptionAccent = Header; AccuracyScore = pos - 100; }
    }

    #endregion
    #region class SearchResultFoodItemVM

    public class SearchResultFoodItemVM : SearchResultVM
    {
        private FoodItem item;
        public SearchResultFoodItemVM(FoodItem item) : base(item.Text) { this.item = item; }
        public override FoodItem MakeItem() { return item; }
    }

    #endregion
	#region class SearchResultRecentVM

	public class SearchResultRecentVM : SearchResultVM
	{
		private RecentItem item;
		public SearchResultRecentVM(RecentItem item) : base(item.Text) { this.item = item; }
		public override FoodItem MakeItem() { return item.FullItem; }
	}

	#endregion
    #region class SearchResultEntryVM

    public class SearchResultEntryVM : SearchResultFoodItemVM
    {
        public SearchResultEntryVM(Entry selection) : base(selection.Item) { }
    }

    #endregion

    //#region class SearchResultRecipeVM

    //public class SearchResultRecipeVM : SearchResultVM
    //{
    //    private Recipe recipe;

    //    public string CookingTime { get { return recipe.CookTime; } }
    //    public string Summary { get { return recipe.Summary; } }

    //    public SearchResultRecipeVM(Recipe recipe) : base(recipe.Text) { this.recipe = recipe; }
    //    public override FoodItem MakeItem() { return recipe; }
    //    public void NavigateToRecipeDetails() { Navigate.ToRecipeDetail(recipe); }
    //}

    //#endregion

    // Here's the case for INotifyPropertyChanged
    // Item is in search results
    // Entry changes Item Text in different Period
    // ... -> refresh Text...
    // proposal is to handle this in Requery instead of implementing INotifyPropertyChanged for Search Results

    public class SearchResultVM
    {

        // text, accuracyscore

        public IAcceptsSearchResult Listener;
        public String Text { get; set; }
        public String CaptionAccent { get; set; }
        public Single AccuracyScore;
        public bool IsAndHit;

        public SearchResultVM(String Text) { this.Text = Text; }

        public void SetAccuracy(Single HitScore, int MaxHitCount)
        {
            if (Text != null)
                AccuracyScore = (100 - HitScore) * 100 + Text.Length;
            IsAndHit = (HitScore > MaxHitCount - 1);
        }

		public virtual bool IsLocked{get{return false;}}

        public virtual FoodItem MakeItem() { return null; }

		#if WINDOWS_PHONE
        public Visibility LockVisibility { get { return IsLocked ? Visibility.Visible : Visibility.Collapsed; } }
        public System.Windows.Media.Brush TextBrush { get { return (System.Windows.Media.Brush)Application.Current.Resources[IsLocked ? "PhoneSubtleBrush" : "PhoneForegroundBrush"]; } }
        public Thickness TextMargin { get { return FoodJournal.AppModel.AppStats.Current.PremiumItemsLocked ? new Thickness(48, 12, 12, 12) : new Thickness(12, 12, 12, 12); } }
		#endif

    }
}