using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace SGTools.Dialogs
{
    // The box that asks for the new number and title.
    // numbersInUse is the sheet's other numbered views: number -> shown title.
    // When Apply is clicked, NewNumber and NewTitle hold the answers.
    public partial class ViewTitleWindow : Window
    {
        public string NewNumber { get; private set; }
        public string NewTitle { get; private set; }

        private string oldNumber;
        private Dictionary<string, string> numbersInUse;
        // A view with no number (a legend) can only have its title changed.
        private bool hasNumber;

        public ViewTitleWindow(string viewName, string oldNumber, string oldTitle,
                               Dictionary<string, string> numbersInUse)
        {
            // Stored first: filling the number field below already runs
            // NumberChanged, which uses all three.
            this.oldNumber = oldNumber;
            this.numbersInUse = numbersInUse;
            hasNumber = oldNumber != "";

            InitializeComponent();

            // Fill the box with what the clicked view has now.
            ViewNameText.Text = viewName;
            NumberBox.Text = oldNumber;
            TitleBox.Text = oldTitle;
            if (!hasNumber)
            {
                NumberBox.IsEnabled = false;
                ShowNotice("This view has no detail number. You can still change its title.");
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Start with the cursor in the number field and the number
            // selected, so typing replaces it. A legend starts in the title.
            TextBox field = hasNumber ? NumberBox : TitleBox;
            field.Focus();
            field.SelectAll();
        }

        // Runs on every change to the text in the number field.
        private void NumberChanged(object sender, TextChangedEventArgs e)
        {
            if (!hasNumber)
            {
                return;
            }
            string number = NumberBox.Text.Trim();
            if (number == "")
            {
                ShowNotice("Type a number. A view can't be left without one.", problem: true);
            }
            else if (number != oldNumber && numbersInUse.ContainsKey(number))
            {
                string otherTitle = numbersInUse[number];
                ShowNotice($"{number} is used by {otherTitle}. " +
                           $"The two views will swap, so {otherTitle} becomes {oldNumber}.");
            }
            else
            {
                Notice.Visibility = Visibility.Collapsed;
            }
        }

        // Show text under the number: teal for information, red for a problem.
        private void ShowNotice(string text, bool problem = false)
        {
            string back = problem ? "SG.ProblemBack" : "SG.NoticeBack";
            string ink = problem ? "SG.ProblemInk" : "SG.NoticeInk";
            Notice.Background = (Brush)FindResource(back);
            NoticeText.Foreground = (Brush)FindResource(ink);
            NoticeText.Text = text;
            Notice.Visibility = Visibility.Visible;
        }

        private void ApplyClicked(object sender, RoutedEventArgs e)
        {
            string number = NumberBox.Text.Trim();
            // An empty number: the red notice already says why, so stay open.
            if (hasNumber && number == "")
            {
                return;
            }
            NewNumber = number;
            NewTitle = TitleBox.Text.Trim();
            DialogResult = true;
        }
        // The × in the title bar: the same as Cancel.
        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        // Lets you move the box by dragging its title bar.
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}