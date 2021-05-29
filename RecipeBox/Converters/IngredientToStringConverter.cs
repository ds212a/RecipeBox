using System;
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
            else if (ingredient.UnitOfMeasurement == Ingredient.UnitOfMeasurements.Item)
            {
                formatted = $"\u2022 {ingredient.Quantity} {ingredient.Name}";
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
