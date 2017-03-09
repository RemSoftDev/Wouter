using FoodJournal.AppModel;
using FoodJournal.Logging;
using FoodJournal.Model.Data;
using FoodJournal.Values;
using FoodJournal.ViewModels.Fragments;
using FoodJournal.WinPhone.Common.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodJournal.Resources;

namespace FoodJournal.Model
{

#if Legacy
    public class Recipe : FoodItem, IAcceptsSelectedAmount
    {

        private RecipeDO Data { get { return (data as RecipeDO); } }
        private SortedDictionary<Single, Ingredient> ingredients;

        public Recipe(RecipeDO data) : base(data) { }
#if DEBUG
        public Recipe(string Text, string Description) : base(new RecipeDO() { TextDB = Text, DescriptionDB = Description, CookingTime = "15 min", AmountSelectedDB = "1 serving" }) { }
#endif

        //public string CookTime { get { return Data.CookingTime.ToString(); } set { TimeSpan ts = TimeSpan.MaxValue; TimeSpan.TryParse(value, out ts); Data.CookingTime = ts; } }
        public string CookTime { get { return Data.CookingTime; } set { Data.CookingTime = value; } }

        public int GetFoodItemId() { return data.Id; }

        public override void Save()
        {
            base.Save();
            foreach (var ingredient in Ingredients.Values)
                ingredient.Save();
        }

        private SortedDictionary<Single, Ingredient> Ingredients
        {
            get
            {
                if (ingredients == null)
                {
                    ingredients = new SortedDictionary<float, Ingredient>();
                    if (data.Id != 0)
                    {
                        foreach (Ingredient row in FoodJournalDB.SelectIngredients(this))
                            ingredients.Add(row.OrderId, row);
                    }
                }
                return ingredients;
            }
        }

        public IEnumerable<Ingredient> GetIngredients()
        {
            return Ingredients.Values;
        }

        public void AddIngredient(FoodItem item)
        {
            var newitem = new Ingredient(this, item);
            Ingredients.Add(newitem.OrderId, newitem);
            if (!this.IsNewItem)
                newitem.Save();
        }

        public void DeleteIngredient(Ingredient ingredient)
        {
            Ingredients.Remove(ingredient.OrderId);
            if (!ingredient.IsNewItem)
                ingredient.DeleteFromDB();
        }

        public void MoveIngredient(Ingredient ingredient, bool up)
        {
            var c = Ingredients;
            int index = c.Where(i => i.Key < ingredient.OrderId).Count();
            Single neworder = 0;
            if (up)
            {
                if (index < 2)
                    neworder = c.ElementAt(0).Key - 1;
                else
                    neworder = (c.ElementAt(index - 2).Key + c.ElementAt(index - 1).Key) / 2;
            }
            else
            {
                if (index > c.Count() - 3)
                    neworder = c.ElementAt(c.Count()).Key + 1;
                else
                    neworder = (c.ElementAt(index + 2).Key + c.ElementAt(index + 1).Key) / 2;
            }
            c.Remove(ingredient.OrderId);
            ingredient.OrderId = neworder;
            c.Add(neworder, ingredient);
        }

        public Single MaxOrderId { get { return Ingredients.Count > 0 ? Ingredients.Last().Value.OrderId : 0; } }


        public string EntryText { get { if (TotalAmount.IsZero) return Text; return TotalAmount.AppendItemText(Text.ToLower()); } }
        public Amount TotalAmount { get { return AmountSelected * AmountScale; } }
        // calculates nutrition value given total amount
        public Amount GetPropertyValue(Property property) { return Amount.FromProperty(Values[property] * PivotScale, property); }
        public string Summary { get { return IngredientSummary; } }//TotalAmount.ToString(true) + ": " + IngredientSummary; } }
        public Amount AmountSelected { get { return Data.AmountSelectedDB; } set { SetSelectedAmountAndScale(value, 1); } }
        public Single AmountScale { get { return Data.AmountScaleDB; } set { SetSelectedAmountAndScale(AmountSelected, value); } }


        // private:
        private Single pivotScale = -1;
        public Single PivotScale { get { if (pivotScale == -1) pivotScale = ServingSizes.CalculateScale(TotalAmount, FoodItem.NutritionScale); return pivotScale; } }

        public void OnAmountConversionChanged() { pivotScale = -1; }

        public void SetSelectedAmountAndScale(Values.Amount SelectedAmount, float Scale)
        {
            Data.AmountSelectedDB = SelectedAmount.ToStorageString();
            Data.AmountScaleDB = Scale;

            OnAmountConversionChanged();

            SaveIfNotNew();

            OnPropertyChanged("AmountSelected");
            OnPropertyChanged("AmountScale");
            OnPropertyChanged("TotalAmount");
            //OnPropertyChanged("EntryText");
            //OnPropertyChanged("Summary");
        }

        public Values.ServingSizeCollection GetServingSizeCollection() { return this.ServingSizes; }
        public Values.Amount GetNewDefaultAmount()
        {
            return AppResources.OneServing;
        }

        public Values.Amount GetAmountSelected() { return Data.AmountSelectedDB; }
        public Values.Amount GetTotalAmount() { return TotalAmount; }



        public string IngredientSummary
        {
            get
            {
                var sb = new System.Text.StringBuilder();
                string value;
                foreach (var ing in GetIngredients())
                {
                    value = ing.Text;
                    if (sb.Length > 0) sb.Append(AppResources.CommaListSeparater);
                    sb.Append(value);
                }
                if (sb.Length == 0) return AppResources.NoIngredients;
                return sb.ToString();
            }
        }
    }


    public class Ingredient : ModelObject, IAcceptsSelectedAmount
    {

        private Recipe recipe;
        private IngredientDO data;

        public Ingredient(Recipe recipe, IngredientDO data) { this.recipe = recipe; this.data = data; }
        public Ingredient(Recipe recipe, FoodItem fooditem)
        {
            this.recipe = recipe;
            this.data = new IngredientDO();
            data.AmountSelectedDB = fooditem.NewEntryAmount.ToStorageString();
            data.AmountScaleDB = 1;
            data.ItemId = fooditem.GetItemId();
            this.item = fooditem;
            item.NotifyIngredients.Add(this);
            data.OrderId = recipe.MaxOrderId + 1;
            data.RecipeId = recipe.GetFoodItemId();
        }

        public Single OrderId { get { return data.OrderId; } set { data.OrderId = value; } }
        public string Text { get { return Item.Text; } }
        public bool IsNewItem { get { return data.Id == 0; } }

        public Amount TotalAmount { get { return AmountSelected * AmountScale; } }
        public Amount GetPropertyValue(Property property) { return Amount.FromProperty(Item.Values[property] * PivotScale, property); }
        public string Summary { get { return TotalAmount.ToString(true) + ": " + NutritionSummary; } }
        public Amount AmountSelected { get { return data.AmountSelectedDB; } set { SetSelectedAmountAndScale(value, 1); } }
        public Single AmountScale { get { return data.AmountScaleDB; } set { SetSelectedAmountAndScale(AmountSelected, value); } }

        protected override void SaveIfNotNew() { if (!IsNewItem) Save(); }
        public override void Save()
        {
            bool WasNewItem = IsNewItem;
            if (Item.IsNewItem) { Item.Save(); data.ItemId = Item.GetItemId(); }
            FoodJournalDB.SaveIngredientDO(data);
        }

        private FoodItem item;
        public FoodItem Item
        {
            get
            {
                if (item == null) { item = Cache.GetItemById(FoodItemType.Food, data.ItemId); item.NotifyIngredients.Add(this); }
                if (item == null)
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#else
                    SessionLog.RecordTraceValue("ERROR: item not found in DB.", data.ItemId.ToString());
                    item = new FoodItem("ERROR", "");
#endif
                }
                return item;
            }
        }

        public void DeleteFromDB()
        {
            FoodJournalDB.DeleteIngredientDO(data);
            item.NotifyIngredients.Remove(this);
        }

        public void NotifyParentOfPropertyChanged()
        {

            //recipe.OnPropertyChanged("Ingredient");
            foreach (Entry entry in recipe.NotifyEntries)
                Cache.OnEntryChanged(entry);
        }

        public void OnFoodItemChanged() { NotifyParentOfPropertyChanged(); }

        // private:
        private Single pivotScale = -1;
        public Single PivotScale { get { if (pivotScale == -1) pivotScale = Item.ServingSizes.CalculateScale(TotalAmount, FoodItem.NutritionScale); return pivotScale; } }
        public void OnAmountConversionChanged() { pivotScale = -1; NotifyParentOfPropertyChanged(); }

        public void SetSelectedAmountAndScale(Values.Amount SelectedAmount, float Scale)
        {
            data.AmountSelectedDB = SelectedAmount.ToStorageString();
            data.AmountScaleDB = Scale;

            OnAmountConversionChanged();

            SaveIfNotNew();

            OnPropertyChanged("AmountSelected");
            OnPropertyChanged("AmountScale");
            OnPropertyChanged("TotalAmount");
            //OnPropertyChanged("EntryText");
            //OnPropertyChanged("Summary");

            NotifyParentOfPropertyChanged();

        }

        public Values.ServingSizeCollection GetServingSizeCollection() { return this.Item.ServingSizes; }
        public Values.Amount GetNewDefaultAmount() { return Item.NewEntryAmount; }
        public Values.Amount GetAmountSelected() { return data.AmountSelectedDB; }
        public Values.Amount GetTotalAmount() { return TotalAmount; }



        public string NutritionSummary
        {
            get
            {
                if (TotalAmount.IsZero) return AppResources.HowMuch;
                if (PivotScale == 0) return AppResources.CantCalculateNutrition;
                var sb = new System.Text.StringBuilder();
                Amount amt;
                string value;
                foreach (var prop in UserSettings.Current.SelectedProperties)
                {
                    amt = GetPropertyValue(prop);
                    if (!amt.IsAlmostZero)
                    {
                        value = amt.ToString(true);
                        if (sb.Length > 0) sb.Append(AppResources.CommaListSeparater);
                        sb.Append(value);
                    }
                }
                if (sb.Length == 0) return AppResources.NoNutritionInfo;
                return sb.ToString();
            }
        }

    }
#endif

}
