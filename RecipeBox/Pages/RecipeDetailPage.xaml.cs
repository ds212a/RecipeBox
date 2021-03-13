using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class RecipeDetailPage : Page
    {
        #region Fields
        Recipe item;
        CultureInfo culture = CultureInfo.CurrentCulture;
        bool canNavigateWithUnsavedChanges = false;
        #endregion

        #region Constructors
        public RecipeDetailPage()
        {
            InitializeComponent();
        }
        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            item = e.Parameter as Recipe;
            canNavigateWithUnsavedChanges = false;

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
            if (item.NeedsSaved && !canNavigateWithUnsavedChanges)
            {
                // The item has unsaved changes and we haven't shown the
                // dialog yet. Cancel navigation and show the dialog.
                e.Cancel = true;
                ShowSaveDialog(e);
            }
            else
            {
                canNavigateWithUnsavedChanges = false;
                base.OnNavigatingFrom(e);
            }
        }

        /// <summary>
        /// Gives users a chance to save the image before navigating
        /// to a different page.
        /// </summary>
        private async void ShowSaveDialog(NavigatingCancelEventArgs e)
        {
            ContentDialog saveDialog = new ContentDialog()
            {
                Title = "Unsaved changes",
                Content = "You have unsaved changes that will be lost if you leave this page.",
                PrimaryButtonText = "Leave this page",
                SecondaryButtonText = "Stay"
            };
            ContentDialogResult result = await saveDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // The user decided to leave the page. Restart
                // the navigation attempt. 
                canNavigateWithUnsavedChanges = true;
                Frame.Navigate(e.SourcePageType, e.Parameter);
            }
        }

        #region Event Handlers
        #endregion

        private void editRecipeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void printRecipeButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
