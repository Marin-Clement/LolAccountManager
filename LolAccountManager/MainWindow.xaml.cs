using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Navigation;
using Newtonsoft.Json;
using WindowsInput;
using WindowsInput.Native;

namespace LolAccountManager
{
    public partial class MainWindow
    {
        private const string JsonFileName = "accounts.json";
        private readonly string _jsonFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LolAccountManager", JsonFileName);

        private ObservableCollection<Account> _accounts;

        public MainWindow()
        {
            InitializeComponent();

            // Create the directory if it doesn't exist
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LolAccountManager")))
            {
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LolAccountManager"));
            }

            LoadAccounts();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            AccountListView.ItemsSource = _accounts;
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        private void LoadAccounts()
        {
            if (File.Exists(_jsonFilePath))
            {
                var json = File.ReadAllText(_jsonFilePath);
                _accounts = JsonConvert.DeserializeObject<ObservableCollection<Account>>(json);
            }
            else
            {
                _accounts = new ObservableCollection<Account>();
            }
        }

        private void SaveAccounts()
        {
            var json = JsonConvert.SerializeObject(_accounts);
            File.WriteAllText(_jsonFilePath, json);
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            var addAccountWindow = new AddModifyAccountWindow();
            if (addAccountWindow.ShowDialog() != true) return;
            _accounts.Add(addAccountWindow.Account);
            SaveAccounts();
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListView.SelectedItem != null)
            {
                _accounts.Remove(AccountListView.SelectedItem as Account);
                SaveAccounts();
            }
            else
            {
                MessageBox.Show("Please select an account to delete.", "Information", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void ModifyAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListView.SelectedItem != null)
            {
                var modifyAccountWindow = new AddModifyAccountWindow(AccountListView.SelectedItem as Account);
                if (modifyAccountWindow.ShowDialog() != true) return;
                var index = _accounts.IndexOf(AccountListView.SelectedItem as Account);
                _accounts[index] = modifyAccountWindow.Account;
                SaveAccounts();
            }
            else
            {
                MessageBox.Show("Please select an account to modify.", "Information", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void FocusRiotClient_Click(object sender, RoutedEventArgs e)
        {
            var processes = Process.GetProcessesByName("RiotClientUx");
            if (processes.Length > 0 && AccountListView.SelectedItem != null)
            {
                var hWnd = processes[0].MainWindowHandle;

                ShowWindow(hWnd, 1);
                SetForegroundWindow(hWnd);

                // Set the cursor to the center of the Riot Client window
                GetClientRect(hWnd, out var clientRect);
                var usernameTextBoxPoint = new Point { X = clientRect.Left + 100, Y = clientRect.Top + 250 };
                ClientToScreen(hWnd, ref usernameTextBoxPoint);
                SetCursorPos(usernameTextBoxPoint.X, usernameTextBoxPoint.Y);
                var inputSimulator = new InputSimulator();
                inputSimulator.Mouse.LeftButtonClick();
                inputSimulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL,
                    VirtualKeyCode.VK_A);
                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.BACK);
                inputSimulator.Keyboard.TextEntry((AccountListView.SelectedItem as Account)?.Username);
                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
                inputSimulator.Keyboard.TextEntry((AccountListView.SelectedItem as Account)?.Password);
                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);

                // Reset the cursor to the center of the screen
                SetCursorPos((int)SystemParameters.PrimaryScreenWidth / 2,
                    (int)SystemParameters.PrimaryScreenHeight / 2);
            }
            else
            {
                MessageBox.Show("Riot Client is not running or you haven't selected an account.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }
    }

    public class Account
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public override string ToString()
        {
            return $"{Username}";
        }
    }
}