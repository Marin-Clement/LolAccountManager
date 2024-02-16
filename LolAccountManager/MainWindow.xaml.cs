﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace LolAccountManager
{
    public partial class MainWindow
    {
        private const string LolAccountManagerFolder = "LolAccountManager";
        private const string RiotGamesPrivateSettingsFile = "RiotGamesPrivateSettings.yaml";

        private const string RiotClientProcessName = "RiotClientServices";
        private const string LeagueOfLegendsProcessName = "LeagueClient";
        private ObservableCollection<Account> _accounts;

        public MainWindow()
        {
            InitializeComponent();

            // create in MyDocuments/LolAccountManager
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    LolAccountManagerFolder)))
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    LolAccountManagerFolder));

            LoadAccounts();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            AccountListView.ItemsSource = _accounts;
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
            var addAccountWindow = new AddModifyAccountWindow();
            if (addAccountWindow.ShowDialog() != true) return;
            _accounts.Add(addAccountWindow.Account);
            Console.WriteLine(addAccountWindow.Account.Username);
            SaveAccount(addAccountWindow.Account);
            KillRiotRelatedProcesses();
            Disconnect();
            StartLeagueOfLegends();
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListView.SelectedItem != null)
            {
                var accountFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    LolAccountManagerFolder, (AccountListView.SelectedItem as Account).Username);
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
            if (AccountListView.SelectedItem != null)
            {
                KillRiotRelatedProcesses();

                // replace the RiotGamesPrivateSettings.yaml file whis the selected account
                var accountFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    LolAccountManagerFolder, (AccountListView.SelectedItem as Account).Username);
                var sourceFilePath = Path.Combine(accountFolderPath, RiotGamesPrivateSettingsFile);
                var destinationFilePath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Riot Games", "Riot Client", "Data", RiotGamesPrivateSettingsFile);
                File.Copy(sourceFilePath, destinationFilePath, true);

                // start the client C:\Riot Games\League of Legends
                StartLeagueOfLegends();
            }
        }

        private void Disconnect()
        {
            // delete the RiotGamesPrivateSettings.yaml file
            var destinationFilePath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Riot Games",
                    "Riot Client", "Data", RiotGamesPrivateSettingsFile);
            File.Delete(destinationFilePath);
        }

        private void KillRiotRelatedProcesses()
        {
            foreach (var process in Process.GetProcessesByName(LeagueOfLegendsProcessName)) process.Kill();

            foreach (var process in Process.GetProcessesByName(RiotClientProcessName)) process.Kill();
        }

        private void StartLeagueOfLegends()
        {
            Process.Start("C:\\Riot Games\\League of Legends\\LeagueClient.exe");
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void CopyUsername_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListView.SelectedItem != null)
                Clipboard.SetText((AccountListView.SelectedItem as Account).Username);
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