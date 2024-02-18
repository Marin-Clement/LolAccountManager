using System;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Win32;

namespace LolAccountManager.View
{
    public partial class SettingsWindow : Window
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
                Version = "2.0.0",
                StartWithWindows = StartWithWindowsCheckBox.IsChecked == true,
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
            System.IO.File.WriteAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), appConfig.LolAccountManagerFolder, "appconfig.json"), json);

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

        private void LoadSettings()
        {
            var json = System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LolAccountManager", "appconfig.json"));
            var appConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<AppConfig>(json);
            if (appConfig == null)
            {
                throw new Exception("Failed to deserialize appconfig.json");
            }
            LeagueOfLegendsPathTextBox.Text = appConfig.LeagueOfLegendsPath;
            StartWithWindowsCheckBox.IsChecked = appConfig.StartWithWindows;
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
            registryKey?.SetValue("LolAccountManager", System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private void StartWithWindows_Unchecked()
        {
            var registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            registryKey?.DeleteValue("LolAccountManager", false);
        }
    }
}