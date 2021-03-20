using Microsoft.Toolkit.Uwp.Helpers;
using RecipeBox.Services.Printing;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
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
        private readonly PrintServiceProvider _printServiceProvider = new PrintServiceProvider();
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

            _printServiceProvider.StatusChanged += PrintServiceProvider_StatusChanged;
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
            _printServiceProvider.UnregisterForPrinting();
            base.OnNavigatingFrom(e);
        }

        #region Event Handlers
        private void editRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(NewRecipePage), recipe);
        }

        private void printRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            _printServiceProvider.RegisterForPrinting(this, typeof(RecipePrintPage), DataContext);
            _printServiceProvider.Print();
        }

        private void PrintServiceProvider_StatusChanged(object sender, PrintServiceEventArgs e)
        {
            switch (e.Severity)
            {
                case EventLevel.Informational:
                    Console.Write(e.Message);
                    break;
                default:
                    Console.WriteLine(e.Message);
                    break;
            }
        }
        #endregion
    }
}
