using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RecipeBox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewRecipePage : Page, INotifyPropertyChanged
    {
        #region Fields
        private bool canNavigateWithUnsavedChanges = false;
        private ObservableCollection<string> newRecipeCategories = new ObservableCollection<string>();
        private ObservableCollection<string> newRecipeCuisines = new ObservableCollection<string>();
        private ObservableCollection<Ingredient> newRecipeIngredients = new ObservableCollection<Ingredient>();
        private ObservableCollection<RecipeInstruction> newRecipeInstructions = new ObservableCollection<RecipeInstruction>();
        private ObservableCollection<RecipeNote> newRecipeNotes = new ObservableCollection<RecipeNote>();
        private ObservableCollection<RecipeImage> newRecipeImages = new ObservableCollection<RecipeImage>();
        #endregion

        #region Properties
        public ObservableCollection<string> NewRecipeCategories { get => newRecipeCategories; }

        public ObservableCollection<string> NewRecipeCuisines { get => newRecipeCuisines; }

        public ObservableCollection<Ingredient> NewRecipeIngredients { get => newRecipeIngredients; }

        public ObservableCollection<RecipeInstruction> NewRecipeInstructions { get => newRecipeInstructions; }

        public ObservableCollection<RecipeNote> NewRecipeNotes { get => newRecipeNotes; }

        public ObservableCollection<RecipeImage> NewRecipeImages { get => newRecipeImages; }
        #endregion

        #region Constructors
        public NewRecipePage()
        {
            InitializeComponent();

            var _enumval = Enum.GetValues(typeof(Ingredient.UnitOfMeasurements)).Cast<Ingredient.UnitOfMeasurements>();
            AddRecipeIngredientUnitOfMeasurementComboBox.ItemsSource = _enumval.ToList();
        }
        #endregion

        #region Event Handlers
        private async void saveRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            string guid = Guid.NewGuid().ToString();
            Recipe newRecipe = new Recipe();
            newRecipe.Id = guid;
            newRecipe.Name = RecipeName.Text;
            newRecipe.Description = RecipeDescription.Text;
            newRecipe.PrepTime = RecipePrepTime.Text;
            newRecipe.CookTime = RecipeCookTime.Text;
            newRecipe.TotalTime = "50 minutes";
            newRecipe.Servings = (uint)(string.IsNullOrEmpty(RecipeServings.Text) ? 0 : Convert.ToUInt16(RecipeServings.Text));
            newRecipe.Rating = RecipeRating.Value;
            newRecipe.Url = RecipeUrl.Text;

            foreach (string category in RecipeCategories.Text.Split(','))
            {
                newRecipe.Categories.Add(category);
            }

            foreach (string cuisine in RecipeCuisines.Text.Split(','))
            {
                newRecipe.Cuisines.Add(cuisine);
            }


            newRecipe.Ingredients = newRecipeIngredients;
            newRecipe.Instructions = newRecipeInstructions;
            newRecipe.Notes = newRecipeNotes;
            newRecipe.Images = newRecipeImages;

            StorageFolder recipesFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Recipes", CreationCollisionOption.OpenIfExists);
            StorageFolder recipeFolder = await recipesFolder.CreateFolderAsync(newRecipe.Id, CreationCollisionOption.OpenIfExists);
            StorageFile file = await recipeFolder.CreateFileAsync("recipe.xml", CreationCollisionOption.OpenIfExists);

            var serializer = new XmlSerializer(typeof(Recipe));
            Stream stream = await file.OpenStreamForWriteAsync();

            using (stream)
            {
                serializer.Serialize(stream, newRecipe);
            }

            Frame.GoBack();
        }

        private void RecipeIngredientsAddButton_Click(object sender, RoutedEventArgs e)
        {
            uint index = Convert.ToUInt16(RecipeIngredientsListView.Items.Count + 1);
            Ingredient.UnitOfMeasurements unitOfMeasurement = (Ingredient.UnitOfMeasurements)Enum.Parse(typeof(Ingredient.UnitOfMeasurements), AddRecipeIngredientUnitOfMeasurementComboBox.SelectedItem.ToString());
            newRecipeIngredients.Add(new Ingredient(Guid.NewGuid().ToString(), index, AddRecipeIngredientQuantityTextBox.Text, Convert.ToUInt16(AddRecipeIngredientQuantityTextBox.Text), unitOfMeasurement));
        }

        private void RecipeIngredientsDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            newRecipeIngredients.RemoveAt(RecipeIngredientsListView.Items.IndexOf(RecipeIngredientsListView.SelectedItem));
        }

        private void RecipeIngredientsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView)
            {
                if (listView.SelectedItem != null)
                    RecipeIngredientDeleteButton.IsEnabled = true;
                else
                    RecipeIngredientDeleteButton.IsEnabled = false;
            }
        }

        private void RecipeInstructionsAddButton_Click(object sender, RoutedEventArgs e)
        {
            uint index = Convert.ToUInt16(RecipeInstructionsListView.Items.Count + 1);
            newRecipeInstructions.Add(new RecipeInstruction(index, AddRecipeInstructionTextBox.Text));
        }

        private void RecipeInstructionsDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            newRecipeInstructions.RemoveAt(RecipeInstructionsListView.Items.IndexOf(RecipeInstructionsListView.SelectedItem));
        }

        private void RecipeInstructionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView)
            {
                if (listView.SelectedItem != null)
                    RecipeInstructionDeleteButton.IsEnabled = true;
                else
                    RecipeInstructionDeleteButton.IsEnabled = false;
            }
        }

        private void RecipeNotesAddButton_Click(object sender, RoutedEventArgs e)
        {
            uint index = Convert.ToUInt16(RecipeNotesListView.Items.Count + 1);
            newRecipeNotes.Add(new RecipeNote(index, AddRecipeNoteTextBox.Text));
        }

        private void RecipeNotesDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            newRecipeNotes.RemoveAt(RecipeNotesListView.Items.IndexOf(RecipeNotesListView.SelectedItem));
        }

        private void RecipeNotesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView)
            {
                if (listView.SelectedItem != null)
                    RecipeNoteDeleteButton.IsEnabled = true;
                else
                    RecipeNoteDeleteButton.IsEnabled = false;
            }
        }
        #endregion

        #region Property Changed methods
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value))
            {
                return false;
            }
            else
            {
                storage = value;
                OnPropertyChanged(propertyName);
                return true;
            }
        }
        #endregion
    }
}
