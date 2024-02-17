using System.Windows;
using System.Windows.Input;

namespace LolAccountManager.View
{
    public partial class AddModifyAccountWindow
    {
        public AddModifyAccountWindow(Account account = null)
        {
            InitializeComponent();

            if (account == null) return;

            UsernameTextBox.Text = account.Username;
        }

        private void OnDraggableTabPanelMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        public Account Account { get; private set; }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Please enter both username", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                Account = new Account { Username = UsernameTextBox.Text };
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Save_Click(sender, e);
                    break;
                case Key.Escape:
                    Cancel_Click(sender, e);
                    break;
            }
        }
    }
}