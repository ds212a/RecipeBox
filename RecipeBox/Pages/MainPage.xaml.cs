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
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        #region Fields
        public static MainPage Current = null;
        private Recipe persistedItem = null;
        #endregion

        #region Properties
        public ObservableCollection<Recipe> Recipes { get; } = new ObservableCollection<Recipe>();
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
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            if (Recipes.Count == 0)
            {
                await GetItemsAsync();
            }

            base.OnNavigatedTo(e);
        }

        private async Task GetItemsAsync()
        {
            QueryOptions options = new QueryOptions();
            options.FolderDepth = FolderDepth.Deep;
            options.FileTypeFilter.Add(".xml");

            try
            {
                StorageFolder appLocalFolder = ApplicationData.Current.LocalFolder;
                var result = appLocalFolder.CreateFileQueryWithOptions(options);
                IReadOnlyList<StorageFile> recipeFiles = await result.GetFilesAsync();
                bool unsupportedFilesFound = false;

                foreach (StorageFile file in recipeFiles)
                {
                    // Only files on the local computer are supported. 
                    // Files on OneDrive or a network location are excluded.
                    if (file.Provider.Id == "computer")
                    {
                        Recipes.Add(await LoadRecipe(file));
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

        #region Event Handlers
        private async void debugNewRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            string guid = Guid.NewGuid().ToString();
            Recipe newRecipe = new Recipe();
            newRecipe.Id = guid;
            newRecipe.Name = "Test Recipe";
            newRecipe.Description = "Recipe Description";
            newRecipe.PrepTime = "20 minutes";
            newRecipe.CookTime = "30 minutes";
            newRecipe.TotalTime = "50 minutes";
            newRecipe.Servings = 6;
            newRecipe.Rating = 0;
            newRecipe.Url = "myUrl";
            newRecipe.Categories.Add("Main Meal");
            newRecipe.Cuisines.Add("Indonesian");
            newRecipe.Ingredients.Add(new Ingredient(Guid.NewGuid().ToString(), 1, "Ingredient 1", 1, Ingredient.UnitOfMeasurements.Item));
            newRecipe.Ingredients.Add(new Ingredient(Guid.NewGuid().ToString(), 2, "Ingredient 2", 2, Ingredient.UnitOfMeasurements.Item));
            newRecipe.Instructions.Add(new RecipeInstruction(1, "Instruction 1"));
            newRecipe.Instructions.Add(new RecipeInstruction(2, "Instruction 2"));
            newRecipe.Notes.Add(new RecipeNote(1, "Note"));
            newRecipe.Images.Add(new RecipeImage("originalPathToImage.jpg", "localPathToImage.jpg"));

            StorageFolder recipesFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Recipes", CreationCollisionOption.OpenIfExists);
            StorageFolder recipeFolder = await recipesFolder.CreateFolderAsync(newRecipe.Id, CreationCollisionOption.OpenIfExists);
            StorageFile file = await recipeFolder.CreateFileAsync("recipe.xml", CreationCollisionOption.OpenIfExists);

            var serializer = new XmlSerializer(typeof(Recipe));
            Stream stream = await file.OpenStreamForWriteAsync();

            using (stream)
            {
                serializer.Serialize(stream, newRecipe);
            }
        }

        private void newRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(NewRecipePage));
        }

        private void appSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            //Frame.Navigate(typeof(AppSettingsPage));
        }

        private void RecipeListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            persistedItem = e.ClickedItem as Recipe;
            Frame.Navigate(typeof(RecipeDetailPage), e.ClickedItem);
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
