using Windows.UI.Xaml.Controls;

namespace RecipeBox.Controls
{
    public sealed partial class EditNoteDialogBox : ContentDialog
    {
        #region Constructors
        public EditNoteDialogBox()
        {
            InitializeComponent();
        }

        public EditNoteDialogBox(RecipeNote note)
        {
            InitializeComponent();

            Note = new RecipeNote()
            {
                Id = note.Id,
                Index = note.Index,
                Note = note.Note
            };

            Opened += EditNoteDialogBox_Opened;
        }
        #endregion

        #region Properties
        public RecipeNote Note
        { get; private set; }
        #endregion

        #region Event Handlers
        private void EditNoteDialogBox_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            EditRecipeNoteTextBox.Text = Note.Note;
        }

        private void EditNoteDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Note.Note = EditRecipeNoteTextBox.Text;
        }
        #endregion
    }
}
