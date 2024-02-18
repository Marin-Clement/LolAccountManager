using System;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Win32;

namespace LolAccountManager.View
{
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            var appConfig = new AppConfig
            {
                Version = GetVersion(),
                StartWithWindows = StartWithWindowsCheckBox.IsChecked == true,
                MinimizeToTray = MinimizeToTrayCheckBox.IsChecked == true,
                FirstRun = false,
                RiotGamesPrivateSettingsFile = "RiotGamesPrivateSettings.yaml",
                LolAccountManagerFolder = "LolAccountManager",
                RiotClientProcessName = "RiotClientServices",
                LeagueOfLegendsProcessName = "LeagueClient",
                LeagueOfLegendsPath = LeagueOfLegendsPathTextBox.Text
            };

            if (!System.IO.Directory.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), appConfig.LolAccountManagerFolder)))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), appConfig.LolAccountManagerFolder));
            }

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(appConfig, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), appConfig.LolAccountManagerFolder, "app-config.json"), json);

            if (appConfig.StartWithWindows)
            {
                StartWithWindows_Checked();
            }
            else
            {
                StartWithWindows_Unchecked();
            }
            ExitSettings_Click(null, null);
        }

        private string GetVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        private void LoadSettings()
        {
            var json = System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LolAccountManager", "app-config.json"));
            var appConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<AppConfig>(json);
            if (appConfig == null)
            {
                throw new Exception("Failed to deserialize app-config.json");
            }
            LeagueOfLegendsPathTextBox.Text = appConfig.LeagueOfLegendsPath;
            StartWithWindowsCheckBox.IsChecked = appConfig.StartWithWindows;
            MinimizeToTrayCheckBox.IsChecked = appConfig.MinimizeToTray;
        }

        private void BrowseLeagueOfLegendsPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select League of Legends Executable",
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LeagueOfLegendsPathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void ExitSettings_Click(object sender, RoutedEventArgs e)
        {
            BeginAnimation(OpacityProperty, null);
            var fadeOutAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.2)));
            fadeOutAnimation.Completed += (s, _) => Close();
            BeginAnimation(OpacityProperty, fadeOutAnimation);
        }

        private void StartWithWindows_Checked()
        {
            var registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            // Include an argument when adding to startup
            string startupCommand = $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\" /startHidden";

            registryKey?.SetValue("LolAccountManager", startupCommand);
        }


        private void StartWithWindows_Unchecked()
        {
            var registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            registryKey?.DeleteValue("LolAccountManager", false);
        }
    }
}