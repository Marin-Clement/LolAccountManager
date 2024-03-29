﻿using System;
using System.IO;
using System.Windows;
using LolAccountManager.View;
using Newtonsoft.Json;

namespace LolAccountManager
{
    public partial class App
    {
        private App()
        {
            Application_Startup();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // check if this application is the only instance running
            var processModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
            if (processModule != null)
            {
                var processName = Path.GetFileNameWithoutExtension(processModule.FileName);
                var processes = System.Diagnostics.Process.GetProcessesByName(processName);
                if (processes.Length > 1)
                {
                    MessageBox.Show("Another instance of the application is already running.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }
            }


            bool startHidden = e.Args.Length > 0 && e.Args[0] == "/startHidden";

            if (startHidden)
            {
                MainWindow = new MainWindow();
                MainWindow.Hide();
            }
            else
            {
                MainWindow = new MainWindow();
                MainWindow.Show();
            }
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
                "LolAccountManager", "app-config.json");

            AppConfig appConfig;

            string json;
            if (File.Exists(configFilePath))
            {
                // check if version is outdated and update the file
                json = File.ReadAllText(configFilePath);

                if (json.Contains("Version"))
                {
                    appConfig = JsonConvert.DeserializeObject<AppConfig>(json);

                    if (appConfig != null && appConfig.Version != GetAppVersion())
                    {
                        appConfig.Version = GetAppVersion();
                        json = JsonConvert.SerializeObject(appConfig, Formatting.Indented);
                        File.WriteAllText(configFilePath, json);
                    }
                    return;
                }
            }


            appConfig = new AppConfig
            {
                Version = GetAppVersion(),
                StartWithWindows = false,
                MinimizeToTray = true,
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

            json = JsonConvert.SerializeObject(appConfig, Formatting.Indented);
            File.WriteAllText(configFilePath, json);

            // Log success or any additional information.
            Console.WriteLine(@"Application configuration initialized successfully.");
        }

        private string GetAppVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }

    public class AppConfig
    {
        public string Version { get; set; }
        public bool StartWithWindows { get; set; }
        public bool MinimizeToTray { get; set; }
        public bool FirstRun { get; set; }
        public string RiotGamesPrivateSettingsFile { get; set; }
        public string LolAccountManagerFolder { get; set; }
        public string RiotClientProcessName { get; set; }
        public string LeagueOfLegendsProcessName { get; set; }
        public string LeagueOfLegendsPath { get; set; }
    }
}
