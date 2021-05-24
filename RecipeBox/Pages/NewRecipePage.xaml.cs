using HtmlAgilityPack;
using Microsoft.Toolkit.Uwp;
using RecipeBox.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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
                TitleTextBlock.Text = "New Recipe";
            }
            else
            {
                editingRecipe = true;
                recipe.NeedsSaved = false;
                TitleTextBlock.Text = recipe.Name;
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

        private double FractionToDouble(string fraction)
        {
            double result = 0.0;

            if (double.TryParse(fraction, out result))
            {
                return result;
            }

            string[] split = fraction.Split(new char[] { ' ', '/' });

            if (split.Length == 2 || split.Length == 3)
            {
                int a, b;

                if (int.TryParse(split[0], out a) && int.TryParse(split[1], out b))
                {
                    if (split.Length == 2)
                    {
                        return (double)a / b;
                    }

                    int c;

                    if (int.TryParse(split[2], out c))
                    {
                        return a + (double)b / c;
                    }
                }
            }

            throw new FormatException("Not a valid fraction.");
        }

        private void RecalculateIndexes(string listToRecalculate)
        {
            if(listToRecalculate.Equals("ingredients"))
            {
                foreach(Ingredient ingredient in recipe.Ingredients)
                {
                    ingredient.Index = Convert.ToUInt32(recipe.Ingredients.IndexOf(ingredient));
                }
            }
            else if (listToRecalculate.Equals("instructions"))
            {
                foreach (RecipeInstruction instruction in recipe.Instructions)
                {
                    instruction.Index = Convert.ToUInt32(recipe.Instructions.IndexOf(instruction));
                }
            }
            else if (listToRecalculate.Equals("notes"))
            {
                foreach (RecipeNote note in recipe.Notes)
                {
                    note.Index = Convert.ToUInt32(recipe.Notes.IndexOf(note));
                }
            }
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
                recipe.Cuisines = new ObservableCollection<string>(cuisinesTokenText.Split(',').Where(s => string.IsNullOrEmpty(s) == false));
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

            double quantity = 0;
            if (double.IsNaN(AddRecipeIngredientQuantityNumberBox.Value))
            {
                FlyoutBase.ShowAttachedFlyout(AddRecipeIngredientQuantityNumberBox);
                return;
            }
            else
            {
                quantity = AddRecipeIngredientQuantityNumberBox.Value;
            }

            if (unitOfMeasurement == Ingredient.UnitOfMeasurements.Header)
                quantity = 0;

            recipe.Ingredients.Add(new Ingredient(Guid.NewGuid().ToString(), index, AddRecipeIngredientNameTextBox.Text, quantity, unitOfMeasurement));
        }

        private async void IngredientsContextFlyoutEditItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuFlyout = sender as MenuFlyoutItem;
            Ingredient originalIngredient = menuFlyout.DataContext as Ingredient;
            EditIngredientDialogBox editIngredientDialog = new EditIngredientDialogBox(originalIngredient);
            var result = await editIngredientDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                int index = recipe.Ingredients.IndexOf(originalIngredient);
                recipe.Ingredients.RemoveAt(index);
                Ingredient updatedIngredient = new Ingredient()
                {
                    Id = editIngredientDialog.Ingredient.Id,
                    Index = editIngredientDialog.Ingredient.Index,
                    Name = editIngredientDialog.Ingredient.Name,
                    Quantity = editIngredientDialog.Ingredient.Quantity,
                    UnitOfMeasurement = editIngredientDialog.Ingredient.UnitOfMeasurement
                };

                recipe.Ingredients.Insert(index, updatedIngredient);
            }
        }

        private void RecipeIngredientsDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = 0;
            MenuFlyoutItem menuFlyout = sender as MenuFlyoutItem;
            if(menuFlyout != null)
            {
                Ingredient ingredientToRemove = menuFlyout.DataContext as Ingredient;
                index = recipe.Ingredients.IndexOf(ingredientToRemove);
            }
            else
            {
                index = RecipeIngredientsListView.Items.IndexOf(RecipeIngredientsListView.SelectedItem);
            }
            
            recipe.Ingredients.RemoveAt(index);
            RecalculateIndexes("ingredients");
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

        private async void InstructionsContextFlyoutEditItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuFlyout = sender as MenuFlyoutItem;
            RecipeInstruction originalInstruction = menuFlyout.DataContext as RecipeInstruction;
            EditInstructionDialogBox editInstructionDialog = new EditInstructionDialogBox(originalInstruction);
            var result = await editInstructionDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                int index = recipe.Instructions.IndexOf(originalInstruction);
                recipe.Instructions.RemoveAt(index);
                RecipeInstruction updatedInstruction = new RecipeInstruction()
                {
                    Id = editInstructionDialog.Instruction.Id,
                    Index = editInstructionDialog.Instruction.Index,
                    Instruction = editInstructionDialog.Instruction.Instruction
                };

                recipe.Instructions.Insert(index, updatedInstruction);
            }
        }

        private void RecipeInstructionsDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = 0;
            MenuFlyoutItem menuFlyout = sender as MenuFlyoutItem;
            if (menuFlyout != null)
            {
                RecipeInstruction instructionToRemove = menuFlyout.DataContext as RecipeInstruction;
                index = recipe.Instructions.IndexOf(instructionToRemove);
            }
            else
            {
                index = RecipeInstructionsListView.Items.IndexOf(RecipeInstructionsListView.SelectedItem);
            }

            recipe.Instructions.RemoveAt(index);
            RecalculateIndexes("instructions");
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

        private async void NotesContextFlyoutEditItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuFlyout = sender as MenuFlyoutItem;
            RecipeNote originalNote = menuFlyout.DataContext as RecipeNote;
            EditNoteDialogBox editNoteDialog = new EditNoteDialogBox(originalNote);
            var result = await editNoteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                int index = recipe.Notes.IndexOf(originalNote);
                recipe.Notes.RemoveAt(index);
                RecipeNote updatedNote = new RecipeNote()
                {
                    Id = editNoteDialog.Note.Id,
                    Index = editNoteDialog.Note.Index,
                    Note = editNoteDialog.Note.Note
                };

                recipe.Notes.Insert(index, updatedNote);
            }
        }

        private void RecipeNotesDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = 0;
            MenuFlyoutItem menuFlyout = sender as MenuFlyoutItem;
            if (menuFlyout != null)
            {
                RecipeNote noteToRemove = menuFlyout.DataContext as RecipeNote;
                index = recipe.Notes.IndexOf(noteToRemove);
            }
            else
            {
                index = RecipeNotesListView.Items.IndexOf(RecipeNotesListView.SelectedItem);
            }

            recipe.Notes.RemoveAt(index);
            RecalculateIndexes("notes");
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

        private async void getRecipeFromUrlButton_Click(object sender, RoutedEventArgs e)
        {
            Regex timeRegEx = new Regex(@"(?<prefix>(Prep|Cook)\sTime:)\s+(?<hours>\d+\s(hours|hour))?(\s+)?(?<minutes>\d+\s(mins|min))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            string unitsOfMeaturementsRegExString = string.Join("|", Enum.GetNames(typeof(Ingredient.UnitOfMeasurements)));
            Regex ingredientRegEx = new Regex($@"(?<prefix>(\D+\s\D+\s|\D+))?(?<quantity>(\d+|\d+\/\d+))?(?<unitOfMeasurement>\D+({unitsOfMeaturementsRegExString}))?\s(?<name>.+)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (string.IsNullOrEmpty(recipe.Id))
            {
                string guid = Guid.NewGuid().ToString();
                recipe.Id = guid;
            }

            //var url = "https://dailycookingquest.com/cards/sate-babi-indonesian-pork-satay.html";
            var url = RecipeUrl.Text;
            if (string.IsNullOrEmpty(url))
            {
                ContentDialog unsupportedUrlDialog = new ContentDialog()
                {
                    Title = "No website entered",
                    Content = "You haven't entered a website, please enter a website.  Currently only dailycookingquest.com is supported.",
                    PrimaryButtonText = "Ok"
                };

                ContentDialogResult result = await unsupportedUrlDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    return;
                }
            }
            else if (!url.Contains("dailycookingquest"))
            {
                ContentDialog unsupportedUrlDialog = new ContentDialog()
                {
                    Title = "Unsupported Website",
                    Content = "You have entered an unsupported website, currently only dailycookingquest.com is supported.",
                    PrimaryButtonText = "Ok"
                };

                ContentDialogResult result = await unsupportedUrlDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    return;
                }
            }

            recipe.Url = url;

            // https://html-agility-pack.net/documentation
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
                            recipe.Description = att.Value;
                        }    
                    }
                }
            }

            var bodyNodes = doc.DocumentNode.SelectNodes("//body/div/div/div/article/section");
            foreach(var childNode in bodyNodes[0].ChildNodes)
            {
                if (childNode.Name == "h2")
                    recipe.Name = childNode.InnerText;
                
                if(childNode.Name == "p")
                {
                    if (childNode.InnerText.StartsWith("Categories:"))
                    {
                        string categories = childNode.InnerText.Replace("Categories: ", "");

                        if(categories.Contains("Main Dish"))
                        {
                            recipe.Categories.Add("Main Dish");
                            categories = categories.Replace("Main Dish", "");
                        }

                        foreach (string category in categories.Split())
                        {
                            if(!string.IsNullOrEmpty(category))
                                recipe.Categories.Add(category);
                        }
                    }
                    else if (childNode.InnerText.StartsWith("Cuisine:"))
                    {
                        foreach(string cuisine in childNode.InnerText.Split())
                        {
                            if (cuisine.StartsWith("Cuisine:"))
                                continue;

                            recipe.Cuisines.Add(cuisine);
                        }
                    }
                    else if (childNode.InnerText.StartsWith("Prep Time:"))
                    {
                        int hours = 0;
                        int minutes = 0;
                        int seconds = 0;
                        
                        MatchCollection matches = timeRegEx.Matches(childNode.InnerText);
                        if(matches.Count > 0)
                        {
                            Group group = matches[0].Groups["hours"];
                            if (group.Success)
                                hours = Convert.ToInt32(matches[0].Groups["hours"].Value.Split()[0]);

                            group = matches[0].Groups["minutes"];
                            if (group.Success)
                                minutes = Convert.ToInt32(matches[0].Groups["minutes"].Value.Split()[0]);
                        }
                        
                        recipe.PrepTime = new TimeSpan(hours, minutes, seconds);
                    }
                    else if (childNode.InnerText.StartsWith("Cook Time:"))
                    {
                        int hours = 0;
                        int minutes = 0;
                        int seconds = 0;
                        
                        MatchCollection matches = timeRegEx.Matches(childNode.InnerText);
                        if (matches.Count > 0)
                        {
                            Group group = matches[0].Groups["hours"];
                            if (group.Success)
                                hours = Convert.ToInt32(matches[0].Groups["hours"].Value.Split()[0]);

                            group = matches[0].Groups["minutes"];
                            if (group.Success)
                                minutes = Convert.ToInt32(matches[0].Groups["minutes"].Value.Split()[0]);
                        }

                        recipe.CookTime = new TimeSpan(hours, minutes, seconds);
                    }
                    else if (childNode.InnerText.StartsWith("Serves:"))
                    {
                        recipe.Servings = Convert.ToUInt32(childNode.InnerText.Split()[1]);
                    }
                }

                if(childNode.Name == "h3")
                {
                    if (childNode.InnerText == "Ingredients")
                    {
                        if (childNode.NextSibling.Name == "ul")
                        {
                            foreach (var listNode in childNode.NextSibling.ChildNodes)
                            {
                                foreach (var attribute in listNode.Attributes)
                                {
                                    if (attribute.Name.ToLower() == "class" && attribute.Value.ToLower() == "ingredient")
                                    {
                                        string prefix = string.Empty;
                                        double quantity = 0;
                                        Ingredient.UnitOfMeasurements unitOfMeasurement = Ingredient.UnitOfMeasurements.Item;
                                        string name = string.Empty;

                                        MatchCollection matches = ingredientRegEx.Matches(listNode.InnerText);
                                        if (matches.Count > 0)
                                        {
                                            Group group = matches[0].Groups["prefix"];
                                            if (group.Success)
                                                prefix = matches[0].Groups["prefix"].Value.ToString();

                                            group = matches[0].Groups["quantity"];
                                            if (group.Success)
                                            {
                                                if (matches[0].Groups["quantity"].Value.ToString().Contains("/"))
                                                {
                                                    quantity = FractionToDouble(matches[0].Groups["quantity"].Value.ToString());
                                                }
                                                else
                                                {
                                                    quantity = Convert.ToDouble(matches[0].Groups["quantity"].Value.ToString());
                                                }
                                            }

                                            group = matches[0].Groups["unitOfMeasurement"];
                                            if (group.Success)
                                                unitOfMeasurement = (Ingredient.UnitOfMeasurements)Enum.Parse(typeof(Ingredient.UnitOfMeasurements), matches[0].Groups["unitOfMeasurement"].Value.ToString(), true);

                                            group = matches[0].Groups["name"];
                                            if (group.Success)
                                            {
                                                if (string.IsNullOrEmpty(prefix))
                                                {
                                                    name = matches[0].Groups["name"].Value.ToString();
                                                }
                                                else
                                                {
                                                    name = $"{prefix} {matches[0].Groups["name"].Value}";
                                                }
                                            }
                                        }

                                        Ingredient newIngredient = new Ingredient()
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            Index = Convert.ToUInt32(childNode.NextSibling.ChildNodes.IndexOf(listNode)),
                                            Name = name,
                                            UnitOfMeasurement = unitOfMeasurement,
                                            Quantity = quantity
                                        };

                                        recipe.Ingredients.Add(newIngredient);
                                    }
                                    else if (attribute.Name.ToLower() == "class" && attribute.Value.ToLower() == "header")
                                    {
                                        Ingredient newIngredient = new Ingredient()
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            Index = Convert.ToUInt32(childNode.NextSibling.ChildNodes.IndexOf(listNode)),
                                            Name = listNode.InnerText,
                                            UnitOfMeasurement = Ingredient.UnitOfMeasurements.Header,
                                            Quantity = 0
                                        };

                                        recipe.Ingredients.Add(newIngredient);
                                    }
                                }
                            }
                        }
                    }
                    else if (childNode.InnerText == "Instructions")
                    {
                        if (childNode.NextSibling.Name == "ol")
                        {
                            foreach (var listNode in childNode.NextSibling.ChildNodes)
                            {
                                RecipeInstruction newInstruction = new RecipeInstruction()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Index = Convert.ToUInt32(childNode.NextSibling.ChildNodes.IndexOf(listNode)) + 1,
                                    Instruction = listNode.InnerText
                                };

                                recipe.Instructions.Add(newInstruction);
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
