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
    public sealed partial class EditInstructionDialogBox : ContentDialog
    {
        #region Constructors
        public EditInstructionDialogBox()
        {
            InitializeComponent();
        }

        public EditInstructionDialogBox(RecipeInstruction instruction)
        {
            InitializeComponent();

            Instruction = new RecipeInstruction()
            {
                Id = instruction.Id,
                Index = instruction.Index,
                Instruction = instruction.Instruction
            };

            Opened += EditInstructionDialogBox_Opened;
        }
        #endregion

        #region Properties
        public RecipeInstruction Instruction
        { get; private set; }
        #endregion

        #region Event Handlers
        private void EditInstructionDialogBox_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            EditRecipeInstructionTextBox.Text = Instruction.Instruction;
        }

        private void EditInstructionDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Instruction.Instruction = EditRecipeInstructionTextBox.Text;
        }
        #endregion
    }
}
