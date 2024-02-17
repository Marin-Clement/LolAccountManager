using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LolAccountManager.View
{
    public partial class MainWindow
    {
        private const string LolAccountManagerFolder = "LolAccountManager";
        private const string RiotGamesPrivateSettingsFile = "RiotGamesPrivateSettings.yaml";

        private const string RiotClientProcessName = "RiotClientServices";
        private const string LeagueOfLegendsProcessName = "LeagueClient";
        private ObservableCollection<Account> _accounts;

        private DateTime _lastClickTime = DateTime.MinValue;

        public MainWindow()
        {
            InitializeComponent();

            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    LolAccountManagerFolder)))
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    LolAccountManagerFolder));

            LoadAccounts();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            AccountListView.ItemsSource = _accounts;
        }

        private void OnDraggableTabPanelMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void LoadAccounts()
        {
            _accounts = new ObservableCollection<Account>();

            // load from MyDocuments/LolAccountManager
            var accountFolders = Directory.GetDirectories(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    LolAccountManagerFolder));
            foreach (var accountFolder in accountFolders)
            {
                var files = Directory.GetFiles(accountFolder);
                foreach (var file in files)
                    if (file.EndsWith(RiotGamesPrivateSettingsFile))
                        _accounts.Add(new Account { Username = Path.GetFileName(accountFolder) });
            }
        }

        private void SaveAccount(Account account)
        {
            var accountFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                LolAccountManagerFolder, account.Username);
            if (!Directory.Exists(accountFolderPath)) Directory.CreateDirectory(accountFolderPath);
            // copy the RiotGamesPrivateSettings.yaml file to the account folder
            var sourceFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Riot Games", "Riot Client", "Data", RiotGamesPrivateSettingsFile);
            var destinationFilePath = Path.Combine(accountFolderPath, RiotGamesPrivateSettingsFile);
            File.Copy(sourceFilePath, destinationFilePath, true);
        }


        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            var addAccountWindow = new AddModifyAccountWindow
            {
                Owner = this
            };

            if (addAccountWindow.ShowDialog() != true) return;
            _accounts.Add(addAccountWindow.Account);
            Console.WriteLine(addAccountWindow.Account.Username);
            SaveAccount(addAccountWindow.Account);
            KillRiotRelatedProcesses();
            Disconnect();
            StartLeagueOfLegends();
        }


        private void RefreshAccount_Click(object sender, RoutedEventArgs e)
        {
            SaveAccount(AccountListView.SelectedItem as Account);
            KillRiotRelatedProcesses();
            Disconnect();
            StartLeagueOfLegends();
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListView.SelectedItem != null)
            {
                var accountFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    LolAccountManagerFolder, ((Account)AccountListView.SelectedItem).Username);
                Directory.Delete(accountFolderPath, true);
                _accounts.Remove(AccountListView.SelectedItem as Account);
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            KillRiotRelatedProcesses();
            Disconnect();
            StartLeagueOfLegends();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListView.SelectedItem == null) return;
            KillRiotRelatedProcesses();

            var accountFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                LolAccountManagerFolder, (AccountListView.SelectedItem as Account)?.Username ?? string.Empty);
            var sourceFilePath = Path.Combine(accountFolderPath, RiotGamesPrivateSettingsFile);
            var destinationFilePath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Riot Games", "Riot Client", "Data", RiotGamesPrivateSettingsFile);
            File.Copy(sourceFilePath, destinationFilePath, true);

            // start the client C:\Riot Games\League of Legends
            StartLeagueOfLegends();
        }

        private static void Disconnect()
        {
            // delete the RiotGamesPrivateSettings.yaml file
            var destinationFilePath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Riot Games",
                    "Riot Client", "Data", RiotGamesPrivateSettingsFile);
            File.Delete(destinationFilePath);
        }

        private static void KillRiotRelatedProcesses()
        {
            foreach (var process in Process.GetProcessesByName(LeagueOfLegendsProcessName)) process.Kill();

            foreach (var process in Process.GetProcessesByName(RiotClientProcessName)) process.Kill();
        }

        private static void StartLeagueOfLegends()
        {
            Process.Start("C:\\Riot Games\\League of Legends\\LeagueClient.exe");
        }

        private void OpenAccountFolder_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListView.SelectedItem == null) return;
            var accountFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                LolAccountManagerFolder, ((Account)AccountListView.SelectedItem).Username);
            Process.Start(accountFolderPath);
        }

        private void AccountListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var now = DateTime.Now;
            var timeSinceLastClick = now - _lastClickTime;

            if (timeSinceLastClick.TotalMilliseconds <= 300)
            {
                if (sender is ListViewItem listViewItem)
                    if (listViewItem.DataContext is Account)
                        Connect_Click(sender, e);

                _lastClickTime = DateTime.MinValue;
            }
            else
            {
                _lastClickTime = now;
            }
        }

        private void SettingsTab_Click(object sender, MouseButtonEventArgs e)
        {
            var settingsWindow = new SettingsWindow
            {
                Owner = this
            };
            settingsWindow.ShowDialog();
        }

        private void HelpTab_Click(object sender, MouseButtonEventArgs e)
        {
            var helpWindow = new HelpWindow
            {
                Owner = this
            };
            helpWindow.ShowDialog();
        }

        private void AboutTab_Click(object sender, MouseButtonEventArgs e)
        {
            var aboutWindow = new AboutWindow
            {
                Owner = this
            };
            aboutWindow.ShowDialog();
        }
    }

    public class Account
    {
        public string Username { get; set; }

        public override string ToString()
        {
            return $"{Username}";
        }
    }
}