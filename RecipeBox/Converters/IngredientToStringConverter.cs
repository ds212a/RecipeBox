using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace RecipeBox.Converters
{
    public class IngredientToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Ingredient ingredient = (Ingredient)value;
            string formatted = string.Empty;

            if (ingredient.UnitOfMeasurement == Ingredient.UnitOfMeasurements.Header)
            {
                formatted = $"{ingredient.Name}";
            }
            else
            {
                formatted = $"\u2022 {ingredient}";
            }

            return formatted;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
