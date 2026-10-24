using System.Windows;

namespace SGTools.Dialogs
{
    // The box that asks for the new number and title.
    // When Apply is clicked, NewNumber and NewTitle hold the answers.
    public partial class ViewTitleWindow : Window
    {
        public string NewNumber { get; private set; }
        public string NewTitle { get; private set; }

        // A view with no number (a legend) can only have its title changed.
        private bool hasNumber;

        public ViewTitleWindow(string viewName, string oldNumber, string oldTitle)
        {
            InitializeComponent();
            hasNumber = oldNumber != "";

            // Fill the box with what the clicked view has now.
            ViewNameText.Text = viewName;
            NumberBox.Text = oldNumber;
            TitleBox.Text = oldTitle;
            if (!hasNumber)
            {
                NumberBox.IsEnabled = false;
            }
        }

        private void ApplyClicked(object sender, RoutedEventArgs e)
        {
            string number = NumberBox.Text.Trim();
            if (hasNumber && number == "")
            {
                MessageBox.Show(this, "A view needs a number.", "View Number + Title");
                return;
            }
            NewNumber = number;
            NewTitle = TitleBox.Text.Trim();
            DialogResult = true;
        }
    }
}