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
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RecipeBox.Services.Printing
{
    public partial class PrintPage
    {
        #region Fields
        #endregion

        #region Properties
        internal Grid PrintableArea => printableArea;

        internal RichTextBlock TextContent => textContent;

        internal RichTextBlockOverflow TextOverflow => textOverflow;
        #endregion

        #region Constructors
        public PrintPage()
        {
            InitializeComponent();
        }

        public PrintPage(RichTextBlockOverflow textLinkContainer)
            : this()
        {
            if (textLinkContainer == null) throw new ArgumentNullException(nameof(textLinkContainer));
            textLinkContainer.OverflowContentTarget = textOverflow;
        }
        #endregion

        internal void AddContent(Paragraph block)
        {
            textContent.Blocks.Add(block);
        }
    }
}
