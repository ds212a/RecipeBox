using Windows.UI.Xaml.Controls;

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
