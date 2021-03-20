using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing;
using Windows.UI.Core;
using Windows.UI.Popups;
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
        Recipe recipe;
        CultureInfo culture = CultureInfo.CurrentCulture;
        private PrintHelper printHelper;
        #endregion

        #region Constructors
        public RecipeDetailPage()
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
        }
        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            recipe = e.Parameter as Recipe;

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
            base.OnNavigatingFrom(e);
        }

        private void ReleasePrintHelper()
        {
            printHelper.Dispose();
        }

        #region Event Handlers
        private void editRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(NewRecipePage), recipe);
        }

        private async void printRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            // Provide an invisible container
            printHelper = new PrintHelper(PrintContainer);

            var cont = new ContentControl();
            cont.ContentTemplate = Resources["RecipePrintTemplate"] as DataTemplate;
            cont.DataContext = recipe;

            printHelper.AddFrameworkElementToPrint(cont);

            printHelper.OnPrintCanceled += PrintHelper_OnPrintCanceled;
            printHelper.OnPrintFailed += PrintHelper_OnPrintFailed;
            printHelper.OnPrintSucceeded += PrintHelper_OnPrintSucceeded;

            // Create a new PrintHelperOptions instance
            var printHelperOptions = new PrintHelperOptions();

            // Add options that you want to be displayed on the print dialog
            printHelperOptions.AddDisplayOption(StandardPrintTaskOptions.Orientation);

            // Set preselected settings
            printHelperOptions.Orientation = PrintOrientation.Portrait;

            await printHelper.ShowPrintUIAsync("Recipe Box", printHelperOptions);
        }

        private async void PrintHelper_OnPrintSucceeded()
        {
            ReleasePrintHelper();
            var dialog = new MessageDialog("Printing done.");
            await dialog.ShowAsync();
        }

        private async void PrintHelper_OnPrintFailed()
        {
            ReleasePrintHelper();
            var dialog = new MessageDialog("Printing failed.");
            await dialog.ShowAsync();
        }

        private void PrintHelper_OnPrintCanceled()
        {
            ReleasePrintHelper();
        }
        #endregion
    }
}
