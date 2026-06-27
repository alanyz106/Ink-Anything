using System.Windows;

namespace Ink_Anything
{
    public partial class CloseDialog : Window
    {
        public bool ShouldMinimize { get; private set; }
        public bool RememberChoice => CheckBoxRemember.IsChecked == true;

        public CloseDialog()
        {
            InitializeComponent();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            ShouldMinimize = false;
            DialogResult = true;
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            ShouldMinimize = true;
            DialogResult = true;
        }
    }
}
