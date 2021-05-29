using System;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RecipeBox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppSettingsPage : Page
    {
        #region Fields
        #endregion

        #region Constructors
        public AppSettingsPage()
        {
            InitializeComponent();
        }
        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            else
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("RecipeSaveLocation"))
                RecipeSaveLocation.Text = localSettings.Values["RecipeSaveLocation"] as string;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        #region Event Handlers
        private async void RecipeSaveLocationBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
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

            if (localSettings.Values.ContainsKey("RecipeSaveLocation"))
                RecipeSaveLocation.Text = localSettings.Values["RecipeSaveLocation"] as string;
        }
        #endregion
    }
}
