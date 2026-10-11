using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SGTools.Dialogs
{
    // The logic behind the box that asks for the new number and title.
    // Its layout is in ViewTitleWindow.xaml; the two files together make
    // one class, which is what "partial" means.
    public partial class ViewTitleWindow : Window
    {
        private readonly string oldNumber;
        private readonly Dictionary<string, string> numbersInUse;
        // A view with no number (a legend) can only have its title changed.
        private readonly bool hasNumber;

        // The answers. Applied stays false if the box was cancelled.
        public bool Applied { get; private set; }
        public string NewNumber { get; private set; }
        public string NewTitle { get; private set; }

        public ViewTitleWindow(string viewName, string viewType, string oldNumber,
                               string oldTitle, Dictionary<string, string> numbersInUse)
        {
            this.oldNumber = oldNumber;
            this.numbersInUse = numbersInUse;
            hasNumber = oldNumber != "";

            // Builds everything described in the XAML file.
            InitializeComponent();

            // Fill the box with what the picked view has now.
            ViewNameRun.Text = viewName;
            ViewTypeRun.Text = viewType;
            TitleHint.Text = viewName;
            TitleBox.Text = oldTitle;
            NumberBox.Text = oldNumber;

            if (!hasNumber)
            {
                NumberBox.IsEnabled = false;
                ShowNotice("This view has no detail number. You can still change its title.", false);
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Put the cursor in the number field with the number selected,
            // so typing replaces it. Without a number, start in the title.
            TextBox field = hasNumber ? NumberBox : TitleBox;
            field.Focus();
            field.SelectAll();
        }

        // Runs on every keystroke in the number field.
        private void NumberChanged(object sender, TextChangedEventArgs e)
        {
            if (!hasNumber)
            {
                return;
            }
            string number = NumberBox.Text.Trim();
            if (number == "")
            {
                ShowNotice("Type a number. A view can't be left without one.", true);
            }
            else if (number != oldNumber && numbersInUse.ContainsKey(number))
            {
                string otherTitle = numbersInUse[number];
                ShowNotice(number + " is used by " + otherTitle + ". The two views will swap, so " +
                           otherTitle + " becomes " + oldNumber + ".", false);
            }
            else
            {
                Notice.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowNotice(string text, bool problem)
        {
            string kind = problem ? "Problem" : "Notice";
            Notice.Background = (Brush)FindResource("SG." + kind + "Back");
            NoticeText.Foreground = (Brush)FindResource("SG." + kind + "Ink");
            NoticeText.Text = text;
            Notice.Visibility = Visibility.Visible;
        }

        private void Apply(object sender, RoutedEventArgs e)
        {
            string number = NumberBox.Text.Trim();
            if (hasNumber && number == "")
            {
                return;
            }
            NewNumber = number;
            NewTitle = TitleBox.Text.Trim();
            Applied = true;
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
