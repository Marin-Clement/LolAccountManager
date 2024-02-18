using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace LolAccountManager
{
    public partial class App : Application
    {
        private App()
        {
            Application_Startup();
        }

        private void Application_Startup()
        {
            try
            {
                InitAppConfig();
            }
            catch (Exception ex)
            {
                // Log the exception or display an error message to the user.
                MessageBox.Show($"An error occurred during application startup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // You might want to close the application or take appropriate action.
                Shutdown();
            }
        }

        private void InitAppConfig()
        {
            var configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "LolAccountManager", "appconfig.json");

            if (File.Exists(configFilePath))
                return;

            var appConfig = new AppConfig
            {
                Version = "2.0.0",
                StartWithWindows = false,
                FirstRun = true,
                RiotGamesPrivateSettingsFile = "RiotGamesPrivateSettings.yaml",
                LolAccountManagerFolder = "LolAccountManager",
                RiotClientProcessName = "RiotClientServices",
                LeagueOfLegendsProcessName = "LeagueClient",
                LeagueOfLegendsPath = "C:\\Riot Games\\League of Legends\\LeagueClient.exe"
            };

            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                appConfig.LolAccountManagerFolder)))
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    appConfig.LolAccountManagerFolder));

            var json = JsonConvert.SerializeObject(appConfig, Formatting.Indented);
            File.WriteAllText(configFilePath, json);

            // Log success or any additional information.
            Console.WriteLine("Application configuration initialized successfully.");
        }
    }

    public class AppConfig
    {
        public string Version { get; set; }
        public bool StartWithWindows { get; set; }
        public bool FirstRun { get; set; }
        public string RiotGamesPrivateSettingsFile { get; set; }
        public string LolAccountManagerFolder { get; set; }
        public string RiotClientProcessName { get; set; }
        public string LeagueOfLegendsProcessName { get; set; }
        public string LeagueOfLegendsPath { get; set; }
    }
}
