using HtmlAgilityPack;
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
    public sealed partial class NewRecipePage : Page
    {
        #region Fields
        Recipe recipe = null;
        ContentDialog saveDialog = null;
        private bool canNavigateWithUnsavedChanges = false;
        private bool editingRecipe = false;
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public NewRecipePage()
        {
            InitializeComponent();

            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                if (Frame.CanGoBack)
                {
                    e.Handled = true;
                    Frame.GoBack();
                }
            };

            saveDialog = new ContentDialog()
            {
                Title = "Unsaved changes",
                Content = "You have unsaved changes that will be lost if you leave this page.",
                PrimaryButtonText = "Leave this page",
                SecondaryButtonText = "Stay"
            };

            saveDialog.IsEnabled = false;

            var _enumval = Enum.GetValues(typeof(Ingredient.UnitOfMeasurements)).Cast<Ingredient.UnitOfMeasurements>();
            AddRecipeIngredientUnitOfMeasurementComboBox.ItemsSource = _enumval.ToList();
        }
        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            recipe = e.Parameter as Recipe;
            if (recipe == null)
            {
                recipe = new Recipe();
                recipe.NeedsSaved = true;
            }
            else
            {
                editingRecipe = true;
                recipe.NeedsSaved = false;
            }

            if (Frame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            else
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // If the recipe has unsaved changes, we want to show a dialog
            // that warns the user before the navigation happens
            // To give the user a chance to view the dialog and respond,
            // we go ahead and cancel the navigation.
            // If the user wants to leave the page, we restart the
            // navigation. We use the canNavigateWithUnsavedChanges field to
            // track whether the user has been asked.
            if (recipe != null)
            {
                if (recipe.NeedsSaved && canNavigateWithUnsavedChanges == false)
                {
                    // The item has unsaved changes and we haven't shown the
                    // dialog yet. Cancel navigation and show the dialog.
                    e.Cancel = true;
                    if (saveDialog.IsEnabled == false)
                        ShowSaveDialog(e);
                }
                else
                {
                    canNavigateWithUnsavedChanges = false;
                    base.OnNavigatingFrom(e);
                }
            }
            else
            {
                base.OnNavigatingFrom(e);
            }
        }

        /// <summary>
        /// Gives users a chance to save the recipe before navigating
        /// to a different page.
        /// </summary>
        private async void ShowSaveDialog(NavigatingCancelEventArgs e)
        {
            saveDialog.IsEnabled = true;
            ContentDialogResult result = await saveDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // The user decided to leave the page. Restart
                // the navigation attempt. 
                canNavigateWithUnsavedChanges = true;
                saveDialog.IsEnabled = false;
                Frame.Navigate(e.SourcePageType, e.Parameter);
            }

            saveDialog.IsEnabled = false;
        }

        #region Event Handlers
        private async void saveRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            if (editingRecipe == false)
            {
                string guid = Guid.NewGuid().ToString();
                recipe.Id = guid;
            }
            
            recipe.Name = RecipeName.Text;
            recipe.Description = RecipeDescription.Text;

            string categoriesTokenText = RecipeCategoriesTokenizingTextBox.SelectedTokenText;
            if (string.IsNullOrEmpty(categoriesTokenText))
            {
                recipe.Categories.Clear();
                recipe.Categories.Add(RecipeCategoriesTokenizingTextBox.Text);
            }
            else
            {
                recipe.Categories.Clear();
                recipe.Categories = new ObservableCollection<string>(categoriesTokenText.Split(',').Where(s => string.IsNullOrEmpty(s) == false));
            }

            string cuisinesTokenText = RecipeCuisinesTokenizingTextBox.SelectedTokenText;
            if (string.IsNullOrEmpty(cuisinesTokenText))
            {
                recipe.Cuisines.Clear();
                recipe.Cuisines.Add(RecipeCuisinesTokenizingTextBox.Text);
            }
            else
            {
                recipe.Cuisines.Clear();
                recipe.Cuisines = new ObservableCollection<string>(categoriesTokenText.Split(',').Where(s => string.IsNullOrEmpty(s) == false));
            }

            string[] prepTimeSplit = RecipePrepTime.Text.Split(':');
            recipe.PrepTime = new TimeSpan(Convert.ToInt32(prepTimeSplit[0]), Convert.ToInt32(prepTimeSplit[1]), 0);
            string[] cookTimeSplit = RecipeCookTime.Text.Split(':');
            recipe.CookTime = new TimeSpan(Convert.ToInt32(cookTimeSplit[0]), Convert.ToInt32(cookTimeSplit[1]), 0);
            recipe.TotalTime = recipe.PrepTime + recipe.CookTime;

            recipe.Servings = (uint)(string.IsNullOrEmpty(RecipeServings.Text) ? 0 : Convert.ToUInt16(RecipeServings.Text));
            recipe.Rating = RecipeRating.Value;
            recipe.Url = RecipeUrl.Text;

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            StorageFolder recipesFolder = await StorageFolder.GetFolderFromPathAsync(localSettings.Values["RecipeSaveLocation"].ToString());
            StorageFolder recipeFolder = await recipesFolder.CreateFolderAsync(recipe.Id, CreationCollisionOption.OpenIfExists);
            StorageFile file = await recipeFolder.CreateFileAsync("recipe.xml", CreationCollisionOption.OpenIfExists);

            var serializer = new XmlSerializer(typeof(Recipe));
            Stream stream = await file.OpenStreamForWriteAsync();

            using (stream)
            {
                serializer.Serialize(stream, recipe);
            }

            recipe.NeedsSaved = false;

            Frame.GoBack();
        }

        private void RecipeIngredientsAddButton_Click(object sender, RoutedEventArgs e)
        {
            uint index = Convert.ToUInt16(RecipeIngredientsListView.Items.Count + 1);
            Ingredient.UnitOfMeasurements unitOfMeasurement = (Ingredient.UnitOfMeasurements)Enum.Parse(typeof(Ingredient.UnitOfMeasurements), AddRecipeIngredientUnitOfMeasurementComboBox.SelectedItem.ToString());
            recipe.Ingredients.Add(new Ingredient(Guid.NewGuid().ToString(), index, AddRecipeIngredientNameTextBox.Text, Convert.ToDouble(AddRecipeIngredientQuantityTextBox.Text), unitOfMeasurement));
        }

        private void RecipeIngredientsDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            recipe.Ingredients.RemoveAt(RecipeIngredientsListView.Items.IndexOf(RecipeIngredientsListView.SelectedItem));
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
            recipe.Instructions.Add(new RecipeInstruction(index, AddRecipeInstructionTextBox.Text));
        }

        private void RecipeInstructionsDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            recipe.Instructions.RemoveAt(RecipeInstructionsListView.Items.IndexOf(RecipeInstructionsListView.SelectedItem));
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
            recipe.Notes.Add(new RecipeNote(index, AddRecipeNoteTextBox.Text));
        }

        private void RecipeNotesDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            recipe.Notes.RemoveAt(RecipeNotesListView.Items.IndexOf(RecipeNotesListView.SelectedItem));
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

        private void getRecipeFromUrlButton_Click(object sender, RoutedEventArgs e)
        {
            string recipeName = string.Empty;
            string recipeDescription = string.Empty;

            var url = "https://dailycookingquest.com/cards/sate-babi-indonesian-pork-satay.html";
            var web = new HtmlWeb();
            var doc = web.Load(url);
            var metaNodes = doc.DocumentNode.SelectNodes("//head/meta");

            foreach(var metaNode in metaNodes)
            {
                foreach(var attribute in metaNode.Attributes.AttributesWithName("name"))
                {
                    if (attribute.Value.ToLower().Equals("description"))
                    {
                        foreach(var att in metaNode.Attributes.AttributesWithName("content"))
                        {
                            recipeDescription = att.Value;
                        }    
                    }
                }
            }

            var bodyNodes = doc.DocumentNode.SelectNodes("//body");
        }
        #endregion
    }
}
