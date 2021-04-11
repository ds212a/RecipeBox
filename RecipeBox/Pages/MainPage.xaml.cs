using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RecipeBox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Fields
        public static MainPage Current = null;
        private Recipe persistedItem = null;
        #endregion

        #region Properties
        public ObservableCollection<Recipe> Recipes { get; } = new ObservableCollection<Recipe>();
        public ObservableCollection<Recipe> RecipesFiltered { get; private set; } = new ObservableCollection<Recipe>();
        public static ObservableCollection<string> SuggestedCategories { get; } = new ObservableCollection<string>();
        public static ObservableCollection<string> SuggestedCuisines { get; } = new ObservableCollection<string>();
        #endregion

        #region Constructors
        public MainPage()
        {
            InitializeComponent();
            Current = this;
        }
        #endregion

        // If the recipe is edited and saved in the details page, this method gets called
        // so that the back navigation connected animation uses the correct recipe.
        public void UpdatePersistedItem(Recipe item)
        {
            persistedItem = item;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("RecipeSaveLocation"))
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(localSettings.Values["RecipeSaveLocation"].ToString());
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("RecipeSaveLocationToken", folder);
            }
            else
            {
                ContentDialog saveDialog = new ContentDialog()
                {
                    Title = "Set Recipe Save Location",
                    Content = "Where would you like to save your recipes?",
                    PrimaryButtonText = "Browse"
                };

                ContentDialogResult result = await saveDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    var folderPicker = new Windows.Storage.Pickers.FolderPicker();
                    folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                    folderPicker.FileTypeFilter.Add("*");

                    StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                    if (folder != null)
                    {
                        StorageFolder recipesFolder = await folder.CreateFolderAsync("Recipes", CreationCollisionOption.OpenIfExists);
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("RecipeSaveLocationToken", recipesFolder);
                        localSettings.Values["RecipeSaveLocation"] = recipesFolder.Path;
                    }
                }
            }

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            if (Recipes.Count == 0)
            {
                await GetItemsAsync();
            }

            RecipesFiltered = new ObservableCollection<Recipe>(Recipes);
            RecipeListView.ItemsSource = RecipesFiltered;

            base.OnNavigatedTo(e);
        }

        private async Task GetItemsAsync()
        {
            QueryOptions options = new QueryOptions();
            options.FolderDepth = FolderDepth.Deep;
            options.FileTypeFilter.Add(".xml");

            try
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                StorageFolder recipeFolder = await StorageFolder.GetFolderFromPathAsync(localSettings.Values["RecipeSaveLocation"].ToString());
                var result = recipeFolder.CreateFileQueryWithOptions(options);
                IReadOnlyList<StorageFile> recipeFiles = await result.GetFilesAsync();
                bool unsupportedFilesFound = false;

                foreach (StorageFile file in recipeFiles)
                {
                    // Only files on the local computer are supported. 
                    // Files on OneDrive or a network location are excluded.
                    if (file.Provider.Id == "computer" || file.Provider.Id == "OneDrive")
                    {
                        Recipes.Add(await LoadRecipe(file));

                        foreach (Recipe recipe in Recipes)
                        {
                            LoadSuggestedCategories(recipe);
                            LoadSuggestedCuisines(recipe);
                            recipe.NeedsSaved = false;
                        }
                    }
                    else
                    {
                        unsupportedFilesFound = true;
                    }
                }

                if (unsupportedFilesFound == true)
                {
                    ContentDialog unsupportedFilesDialog = new ContentDialog
                    {
                        Title = "Unsupported recipes found",
                        Content = "This sample app only supports recipes stored locally on the computer. We found files in your library that are stored in OneDrive or another network location. We didn't load those recipes.",
                        CloseButtonText = "Ok"
                    };

                    ContentDialogResult resultNotUsed = await unsupportedFilesDialog.ShowAsync();
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(string.Format("{0}: {1}", ex.ToString(), ex.Message));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async static Task<Recipe> LoadRecipe(StorageFile file)
        {
            object recipeObject = null;
            var serializer = new XmlSerializer(typeof(Recipe));
            Stream stream = await file.OpenStreamForReadAsync();

            using (stream)
            {
                recipeObject = serializer.Deserialize(stream);
            }

            return recipeObject as Recipe;
        }

        public void LoadSuggestedCategories(Recipe recipe)
        {
            foreach (string category in recipe.Categories)
            {
                SuggestedCategories.Add(category);
            }
        }

        public void LoadSuggestedCuisines(Recipe recipe)
        {
            foreach (string cuisine in recipe.Cuisines)
            {
                SuggestedCuisines.Add(cuisine);
            }
        }

        // The following functions are called inside OnFilterChanged:

        /* When the text in any filter is changed, perform a check on each item in the original
        contact list to see if the item should be displayed, taking into account all three of the
        filters currently applied. If the item passes all three checks for all three filters,
        the function returns true and the item is added to the filtered list above. */
        private bool Filter(Recipe recipe)
        {
            return recipe.Name.Contains(FilterByName.Text, StringComparison.InvariantCultureIgnoreCase) &&
                    recipe.Categories.Any(category => category.Contains(FilterByCategory.Text, StringComparison.InvariantCultureIgnoreCase)) &&
                    recipe.Cuisines.Any(cuisine => cuisine.Contains(FilterByCuisine.Text, StringComparison.InvariantCultureIgnoreCase));
        }

        /* These functions go through the current list being displayed (contactsFiltered), and remove
        any items not in the filtered collection (any items that don't belong), or add back any items
        from the original allContacts list that are now supposed to be displayed (i.e. when backspace is hit). */
        private void Remove_NonMatching(IEnumerable<Recipe> filteredData)
        {
            for (int i = RecipesFiltered.Count - 1; i >= 0; i--)
            {
                var item = RecipesFiltered[i];
                // If contact is not in the filtered argument list, remove it from the ListView's source.
                if (!filteredData.Contains(item))
                {
                    RecipesFiltered.Remove(item);
                }
            }
        }

        private void AddBack_Recipe(IEnumerable<Recipe> filteredData)
        {
            foreach (var item in filteredData)
            {
                // If item in filtered list is not currently in ListView's source collection, add it back in
                if (!RecipesFiltered.Contains(item))
                {
                    RecipesFiltered.Add(item);
                }
            }
        }

        #region Event Handlers
        private async void debugNewRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            string guid = Guid.NewGuid().ToString();
            Recipe newRecipe = new Recipe();
            newRecipe.Id = guid;
            newRecipe.Name = "Sambal Rebus - Boiled Chili Sauce";
            newRecipe.Description = "Whenever you make Indonesian soto (soups) at home, you must make sambal rebus to enjoy with your soto. Soto doesn't feel complete without this sambal.";
            newRecipe.PrepTime = new TimeSpan(0, 10, 0);
            newRecipe.CookTime = new TimeSpan(0, 10, 0);
            newRecipe.TotalTime = newRecipe.PrepTime + newRecipe.CookTime;
            newRecipe.Servings = 6;
            newRecipe.Rating = 0;
            newRecipe.Url = "https://dailycookingquest.com/cards/sate-babi-indonesian-pork-satay.html";
            newRecipe.Categories.Add("Sauce");
            newRecipe.Categories.Add("Vegetarian");
            newRecipe.Cuisines.Add("Indonesian");

            newRecipe.Ingredients.Add(new Ingredient(Guid.NewGuid().ToString(), 1, "red Thai chilies (Indonesian: cabe rawit merah), remove the seeds (*)", 20, Ingredient.UnitOfMeasurements.Item));
            newRecipe.Ingredients.Add(new Ingredient(Guid.NewGuid().ToString(), 2, "shallots (Indonesian: bawang merah), peeled", 2, Ingredient.UnitOfMeasurements.Item));
            newRecipe.Ingredients.Add(new Ingredient(Guid.NewGuid().ToString(), 2, "water", 2, Ingredient.UnitOfMeasurements.Milliliters));
            newRecipe.Ingredients.Add(new Ingredient(Guid.NewGuid().ToString(), 2, "salt", 0.25, Ingredient.UnitOfMeasurements.Teaspoon));
            newRecipe.Ingredients.Add(new Ingredient(Guid.NewGuid().ToString(), 2, "sugar", 0.25, Ingredient.UnitOfMeasurements.Teaspoon));

            newRecipe.Instructions.Add(new RecipeInstruction(1, "In a sauce pan, boil together chilies, shallots, and water until the vegetables are soft and the water is reduced by 50%."));
            newRecipe.Instructions.Add(new RecipeInstruction(2, "Transfer chilies, shallots, and water to a food processor or blender, season with salt and sugar. Process until smooth."));

            newRecipe.Notes.Add(new RecipeNote(1, "(*) Do not remove the seeds for a super fiery chili sauce."));

            newRecipe.Images.Add(new RecipeImage("https://dailycookingquest.com/img/2014/04/sambal_rebus.jpg", "sambal_rebus.jpg"));

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            StorageFolder recipesFolder = await StorageFolder.GetFolderFromPathAsync(localSettings.Values["RecipeSaveLocation"].ToString());
            StorageFolder recipeFolder = await recipesFolder.CreateFolderAsync(newRecipe.Id, CreationCollisionOption.OpenIfExists);
            StorageFile file = await recipeFolder.CreateFileAsync("recipe.xml", CreationCollisionOption.OpenIfExists);

            var serializer = new XmlSerializer(typeof(Recipe));
            Stream stream = await file.OpenStreamForWriteAsync();

            using (stream)
            {
                serializer.Serialize(stream, newRecipe);
            }

            await GetItemsAsync();
        }

        private void newRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(NewRecipePage));
        }

        private void appSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AppSettingsPage));
        }

        private void RecipeListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            persistedItem = e.ClickedItem as Recipe;
            Frame.Navigate(typeof(RecipeDetailPage), e.ClickedItem);
        }

        // Whenever text changes in any of the filtering text boxes, the following function is called:
        private void OnFilterChanged(object sender, TextChangedEventArgs args)
        {
            // This is a Linq query that selects only items that return True after being passed through
            // the Filter function, and adds all of those selected items to filtered.
            var filtered = Recipes.Where(recipe => Filter(recipe));
            Remove_NonMatching(filtered);
            AddBack_Recipe(filtered);
        }

        private void EditRecipeMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuFlyoutItem = sender as MenuFlyoutItem;
            Recipe recipe = menuFlyoutItem.DataContext as Recipe;
            Frame.Navigate(typeof(NewRecipePage), recipe);
        }

        private async void DeleteRecipeMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuFlyoutItem = sender as MenuFlyoutItem;
            Recipe recipe = menuFlyoutItem.DataContext as Recipe;

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            StorageFolder recipesFolder = await StorageFolder.GetFolderFromPathAsync(localSettings.Values["RecipeSaveLocation"].ToString());
            StorageFolder recipeFolder = await recipesFolder.CreateFolderAsync(recipe.Id, CreationCollisionOption.OpenIfExists);

            ContentDialog saveDialog = new ContentDialog()
            {
                Title = "Delete Recipe",
                Content = "Are you sure you want to delete this recipe?",
                PrimaryButtonText = "Ok",
                SecondaryButtonText = "Cancel"
            };

            ContentDialogResult result = await saveDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await recipeFolder.DeleteAsync();
                await GetItemsAsync();
            }
            
        }
        #endregion
    }
}
