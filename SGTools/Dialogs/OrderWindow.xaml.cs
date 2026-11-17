using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SGTools.Dialogs
{
    // Asks for the reading order, with last time's choice highlighted.
    // When a choice is clicked, Choice holds "Columns first" or "Rows first".
    public partial class OrderWindow : Window
    {
        public string Choice { get; private set; }

        public OrderWindow(string lastChoice)
        {
            InitializeComponent();

            // Make last time's choice the teal button that Enter presses.
            Button remembered = lastChoice == "Rows first" ? RowsButton : ColumnsButton;
            remembered.Style = (Style)FindResource("SG.PrimaryButton");
            remembered.IsDefault = true;
        }

        private void ColumnsClicked(object sender, RoutedEventArgs e)
        {
            Choice = "Columns first";
            DialogResult = true;
        }

        private void RowsClicked(object sender, RoutedEventArgs e)
        {
            Choice = "Rows first";
            DialogResult = true;
        }

        // Lets you move the window by dragging its title bar.
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}