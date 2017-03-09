//using FoodJournal.AppModel.SQLite;
using FoodJournal.Values;
using System;
using System.Collections.Generic;
#if WINDOWS_PHONE
using System.Data.Linq.Mapping;
#else
using FoodJournal.AppModel.SQLite;
#endif
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournal.Model.Data
{

    public enum FoodItemType
    {
        Food,
        Recipe
    }

    public interface IFoodItemDO
    {

        int Id { get; set; }
        FoodItemType Type { get; }

        [Column(Name="CommonMeal")]
        string CommonMeal { get; set; }
        [Column(Name="TextDB")]
        string TextDB { get; set; }
        [Column(Name="DescriptionDB")]
        string DescriptionDB { get; set; }
        [Column(Name="Culture")]
        string Culture { get; set; }
        [Column(Name="SourceID")]
        string SourceID { get; set; }
        [Column(Name="NutritionDB")]
        string NutritionDB { get; set; }
        [Column(Name="ServingSizesDB")]
        string ServingSizesDB { get; set; }
        [Column(Name="LastAmountDB")]
        string LastAmountDB { get; set; }
    }

    [Table(Name = "FoodItem")]
    public partial class FoodItemDO : IFoodItemDO
    {

		#if WINDOWS_PHONE
		[Column(IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
		#else
		[PrimaryKey, AutoIncrement]
		#endif
        public int Id { get; set; }

        public FoodItemType Type { get { return FoodItemType.Food; } }

        [Column(Name="CommonMeal")]
        public string CommonMeal { get; set; }
        [Column(Name="TextDB")]
        public string TextDB { get; set; }
        [Column(Name="DescriptionDB")]
        public string DescriptionDB { get; set; }
        [Column(Name="Culture")]
        public string Culture { get; set; }
        [Column(Name="SourceID")]
        public string SourceID { get; set; }
        [Column(Name="NutritionDB")]
        public string NutritionDB { get; set; }
        [Column(Name="ServingSizesDB")]
        public string ServingSizesDB { get; set; }
        [Column(Name="LastAmountDB")]
        public string LastAmountDB { get; set; }

        [Column(Name="OldDBID")]
        public int OldDBID { get; set; }

    }

    [Table(Name = "Entry")]
    public partial class EntryDO
    {

        public EntryDO() { }

		#if WINDOWS_PHONE
		[Column(IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
		#else
		[PrimaryKey, AutoIncrement]
		#endif
        public int Id { get; set; }

        [Column(Name="Date")]
		public String Date { get; set; }

        [Column(Name="Period")]
        public Period Period { get; set; }

        [Column(Name="FoodItemType")]
        public FoodItemType FoodItemType { get; set; }
        [Column(Name="FoodItemId")]
        public int FoodItemId { get; set; }

        [Column(Name="AmountSelectedDB")]
        public String AmountSelectedDB { get; set; }
        [Column(Name="AmountScaleDB")]
        public Single AmountScaleDB { get; set; }

    }

    [Table(Name = "Recipe")]
    public partial class RecipeDO : IFoodItemDO
    {

		#if WINDOWS_PHONE
		[Column(IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Name = "IDDD")]
		#else
		[PrimaryKey, AutoIncrement]
		#endif
        public int Id { get; set; }

        public FoodItemType Type { get { return FoodItemType.Recipe; } }

        [Column(Name="CommonMeal")]
        public string CommonMeal { get; set; }
        [Column(Name="TextDB")]
        public string TextDB { get; set; }
        [Column(Name="DescriptionDB")]
        public string DescriptionDB { get; set; }
        [Column(Name="Culture")]
        public string Culture { get; set; }
        [Column(Name="SourceID")]
        public string SourceID { get; set; }
        [Column(Name="NutritionDB")]
        public string NutritionDB { get; set; }
        [Column(Name="ServingSizesDB")]
        public string ServingSizesDB { get; set; }
        [Column(Name="LastAmountDB")]
        public string LastAmountDB { get; set; }

        [Column(Name="Book")]
        public String Book { get; set; }
        [Column(Name="CookingTime")]
        public String CookingTime { get; set; }


        [Column(Name="AmountSelectedDB")]
        public String AmountSelectedDB { get; set; }
        [Column(Name="AmountScaleDB")]
        public Single AmountScaleDB { get; set; }

    }

    [Table(Name = "RecipeIngredient")]
    public partial class IngredientDO
    {

        public IngredientDO() { }

		#if WINDOWS_PHONE
		[Column(IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
		#else
		[PrimaryKey, AutoIncrement]
		#endif
        public int Id { get; set; }

        [Column(Name="RecipeId")]
        public int RecipeId { get; set; }
        [Column(Name="ItemId")]
        public int ItemId { get; set; }

        [Column(Name="AmountSelectedDB")]
        public String AmountSelectedDB { get; set; }
        [Column(Name="AmountScaleDB")]
        public Single AmountScaleDB { get; set; }

        [Column(Name="OrderId")]
        public Single OrderId { get; set; }

    }


}

