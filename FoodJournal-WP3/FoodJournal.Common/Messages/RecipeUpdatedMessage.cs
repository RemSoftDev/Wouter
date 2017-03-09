using FoodJournal.Logging;
using FoodJournal.Model;
using FoodJournal.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournal.Messages
{
#if false

    [DataContract]
    public class RecipeUpdatedMessage
    {

        public RecipeUpdatedMessage() { }

        public RecipeUpdatedMessage(Recipe recipe)
        {
            try
            {
                Text = recipe.Text;
                Description = recipe.Description;
                TotalAmount = recipe.TotalAmount.ToStorageString();
                
                //Nutrition = recipe.NutritionSummary;

                Ingredients = recipe.GetIngredients().Select(i => new Part(i)).ToList();

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        [DataMember]
        public string Text;

        [DataMember]
        public string Description;

        [DataMember]
        public string TotalAmount;

        [DataMember]
        public string CookTime;

        [DataMember]
        public List<Part> Ingredients;

        [DataContract]
        public class Part
        {
            public Part(Ingredient ingredient)
            {
                Item = ingredient.Item.Text;
                Amount = ingredient.TotalAmount.ToStorageString();
            }

            [DataMember]
            public string Item;

            [DataMember]
            public string Amount;

        }

    }
#endif

}
