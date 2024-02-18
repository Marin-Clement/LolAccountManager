using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using static System.Windows.Media.ColorConverter;

namespace LolAccountManager.View
{
    public partial class MainWindow
    {
        private TaskbarIcon _taskbarIcon;

        public bool IsFirstRun { get; set; }
        private static string _lolAccountManagerFolder;
        private static string _riotGamesPrivateSettingsFile;
        private static string _riotClientProcessName;
        private static string _leagueOfLegendsProcessName;
        private static string _leagueOfLegendsPath;
        private ObservableCollection<Account> _accounts;

        private DateTime _lastClickTime = DateTime.MinValue;

        public MainWindow()
        {
            LoadAppConfig();
            InitializeComponent();
            InitializeTaskbarIcon();
            if (IsFirstRun) PlayIntroAnimation(HelpTab);
            DataContext = this;

            LoadAccounts();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            AccountListView.ItemsSource = _accounts;
        }

        private void InitializeTaskbarIcon()
        {
            _taskbarIcon = new TaskbarIcon
            {
                IconSource = new BitmapImage(new Uri("pack://application:,,,/LolAccountManager;component/Resources/IconTaskbar.ico")),
                ToolTipText = "Lol Account Manager",
                Visibility = Visibility.Visible
            };

            _taskbarIcon.TrayMouseDoubleClick += (sender, args) => ShowMainWindow();

            // Adding a right-click context menu with a custom style
            _taskbarIcon.ContextMenu = new ContextMenu { Style = (Style)FindResource("CustomContextMenuStyle") };

            // Add a "Show" menu item
            var showMenuItem = new MenuItem { Header = "Show", Style = (Style)FindResource("CustomMenuItemStyle"), Width = 100, FontSize = 14 };
            showMenuItem.Click += (o, eventArgs) => ShowMainWindow();
            _taskbarIcon.ContextMenu.Items.Add(showMenuItem);


            // Add a "Close" menu item with the custom style
            var closeMenuItem = new MenuItem { Header = "Close", Style = (Style)FindResource("CustomMenuItemStyle"), Width = 100, FontSize = 14 };
            closeMenuItem.Click += (o, eventArgs) => CloseApplication_Click();
            _taskbarIcon.ContextMenu.Items.Add(closeMenuItem);
        }

        private void CloseApplication_Click()
        {
            _taskbarIcon.Dispose();
            Application.Current.Shutdown();
        }


        private void ShowMainWindow()
        {
            Show();
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            Activate();
        }

        private void LoadAppConfig()
        {
            var json = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "LolAccountManager", "appconfig.json"));
            var appConfig = JsonConvert.DeserializeObject<AppConfig>(json);
            if (appConfig == null)
            {
                throw new Exception("Failed to deserialize appconfig.json");
            }
            IsFirstRun = appConfig.FirstRun;
            _lolAccountManagerFolder = appConfig.LolAccountManagerFolder;
            _riotGamesPrivateSettingsFile = appConfig.RiotGamesPrivateSettingsFile;
            _riotClientProcessName = appConfig.RiotClientProcessName;
            _leagueOfLegendsProcessName = appConfig.LeagueOfLegendsProcessName;
            _leagueOfLegendsPath = appConfig.LeagueOfLegendsPath;
        }

        private void PlayIntroAnimation(TabItem tab)
        {
            ColorAnimation colorAnimation = new ColorAnimation
            {
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true,
                From = (Color)ConvertFromString("#FFA500"),
                To = (Color)ConvertFromString("#110638"),
                Duration = TimeSpan.FromSeconds(1)
            };

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(colorAnimation);

            // Set the target property for the color animation
            Storyboard.SetTarget(colorAnimation, tab);
            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("(Border.Background).(SolidColorBrush.Color)"));

            // Begin the animation
            storyboard.Begin();
        }


        private void StopWelcomeAnimation(TabItem tab)
        {
            tab.Background = new SolidColorBrush((Color)ConvertFromString("#110638"));
        }


        private void OnDraggableTabPanelMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            ShowInTaskbar = true;
        }


        private void LoadAccounts()
        {
            _accounts = new ObservableCollection<Account>();

            // load from MyDocuments/LolAccountManager
            var accountFolders = Directory.GetDirectories(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    _lolAccountManagerFolder));
            foreach (var accountFolder in accountFolders)
            {
                var files = Directory.GetFiles(accountFolder);
                foreach (var file in files)
                    if (file.EndsWith(_riotGamesPrivateSettingsFile))
                        _accounts.Add(new Account { Username = Path.GetFileName(accountFolder) });
            }
        }

        private void SaveAccount(Account account)
        {
            var accountFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                _lolAccountManagerFolder, account.Username);
            if (!Directory.Exists(accountFolderPath)) Directory.CreateDirectory(accountFolderPath);
            // copy the RiotGamesPrivateSettings.yaml file to the account folder
            var sourceFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Riot Games", "Riot Client", "Data", _riotGamesPrivateSettingsFile);
            var destinationFilePath = Path.Combine(accountFolderPath, _riotGamesPrivateSettingsFile);
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
                    _lolAccountManagerFolder, ((Account)AccountListView.SelectedItem).Username);
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
                _lolAccountManagerFolder, (AccountListView.SelectedItem as Account)?.Username ?? string.Empty);
            var sourceFilePath = Path.Combine(accountFolderPath, _riotGamesPrivateSettingsFile);
            var destinationFilePath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Riot Games", "Riot Client", "Data", _riotGamesPrivateSettingsFile);
            File.Copy(sourceFilePath, destinationFilePath, true);

            // start the client C:\Riot Games\League of Legends
            StartLeagueOfLegends();
        }

        private static void Disconnect()
        {
            // delete the RiotGamesPrivateSettings.yaml file
            var destinationFilePath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Riot Games",
                    "Riot Client", "Data", _riotGamesPrivateSettingsFile);
            File.Delete(destinationFilePath);
        }

        private static void KillRiotRelatedProcesses()
        {
            foreach (var process in Process.GetProcessesByName(_leagueOfLegendsProcessName)) process.Kill();

            foreach (var process in Process.GetProcessesByName(_riotClientProcessName)) process.Kill();
        }

        private static void StartLeagueOfLegends()
        {
            Process.Start(_leagueOfLegendsPath);
        }

        private void OpenAccountFolder_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListView.SelectedItem == null) return;
            var accountFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                _lolAccountManagerFolder, ((Account)AccountListView.SelectedItem).Username);
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
            if (IsFirstRun)
            {
                StopWelcomeAnimation(SettingsTab);

                var json = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "LolAccountManager", "appconfig.json"));
                var appConfig = JsonConvert.DeserializeObject<AppConfig>(json);
                appConfig.FirstRun = false;
                IsFirstRun = appConfig.FirstRun;

                var json2 = JsonConvert.SerializeObject(appConfig, Formatting.Indented);
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "LolAccountManager", "appconfig.json"), json2);
            }
            settingsWindow.ShowDialog();
        }

        private void HelpTab_Click(object sender, MouseButtonEventArgs e)
        {
            if (IsFirstRun)
            {
                StopWelcomeAnimation(HelpTab);
                PlayIntroAnimation(SettingsTab);
            }

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