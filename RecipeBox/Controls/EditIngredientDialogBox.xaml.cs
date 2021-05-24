using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace RecipeBox.Controls
{
    public sealed partial class EditIngredientDialogBox : ContentDialog
    {
        #region Constructors
        public EditIngredientDialogBox()
        {
            InitializeComponent();
        }

        public EditIngredientDialogBox(Ingredient ingredient)
        {
            InitializeComponent();

            Ingredient = new Ingredient()
            {
                Id = ingredient.Id,
                Index = ingredient.Index,
                Name = ingredient.Name,
                Quantity = ingredient.Quantity,
                UnitOfMeasurement = ingredient.UnitOfMeasurement
            };

            Opened += EditIngredientDialogBox_Opened;

            var _enumval = Enum.GetValues(typeof(Ingredient.UnitOfMeasurements)).Cast<Ingredient.UnitOfMeasurements>();
            EditRecipeIngredientUnitOfMeasurementComboBox.ItemsSource = _enumval.ToList();
        }
        #endregion

        #region Properties
        public Ingredient Ingredient
        { get; private set; }
        #endregion

        #region Event Handlers
        private void EditIngredientDialogBox_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            EditRecipeIngredientQuantityNumberBox.Value = Ingredient.Quantity;
            EditRecipeIngredientUnitOfMeasurementComboBox.SelectedItem = Ingredient.UnitOfMeasurement;
            EditRecipeIngredientNameTextBox.Text = Ingredient.Name;
        }

        private void EditIngredientDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Ingredient.UnitOfMeasurements unitOfMeasurement = (Ingredient.UnitOfMeasurements)Enum.Parse(typeof(Ingredient.UnitOfMeasurements), EditRecipeIngredientUnitOfMeasurementComboBox.SelectedItem.ToString());

            double quantity = 0;
            if (double.IsNaN(EditRecipeIngredientQuantityNumberBox.Value))
            {
                FlyoutBase.ShowAttachedFlyout(EditRecipeIngredientQuantityNumberBox);
                return;
            }
            else
            {
                quantity = EditRecipeIngredientQuantityNumberBox.Value;
            }

            if (unitOfMeasurement == Ingredient.UnitOfMeasurements.Header)
                quantity = 0;

            Ingredient.Quantity = quantity;
            Ingredient.UnitOfMeasurement = unitOfMeasurement;
            Ingredient.Name = EditRecipeIngredientNameTextBox.Text;
        }
        #endregion
    }
}
