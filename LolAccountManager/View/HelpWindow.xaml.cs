using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace LolAccountManager.View
{
    public partial class HelpWindow
    {
        private class ScreenshotInfo
        {
            public string Description { get; set; }
            public string Path { get; set; }
        }

        private readonly List<ScreenshotInfo> _screenshots = new List<ScreenshotInfo>
        {
            new ScreenshotInfo
            {
                Description = "Welcome to the League of Legends Account Manager!",
                Path = "pack://application:,,,/LolAccountManager;component/Resources/HelpWindow/HelpWindowScreenshot1.png"
            },
            new ScreenshotInfo
            {
                Description = "To add an account, connect to the League of Legends client and check the 'Stay signed in' (IF NOT. The manager will not be able to log in.)",
                Path = "pack://application:,,,/LolAccountManager;component/Resources/HelpWindow/HelpWindowScreenshot2.png"
            },
            new ScreenshotInfo
            {
                Description = "Now, Start the League of Legends client and click the 'Save Account' button. (You can match the account with the summoner name.{optional})",
                Path = "pack://application:,,,/LolAccountManager;component/Resources/HelpWindow/HelpWindowScreenshot3.png"
            },
            new ScreenshotInfo
            {
                Description = "You will get automatically logged out of the client. The account is now added to the manager. You can now right-click the account to see the options.",
                Path = "pack://application:,,,/LolAccountManager;component/Resources/HelpWindow/HelpWindowScreenshot4.png"
            },
            new ScreenshotInfo
            {
                Description = "Never Disconnect the account from the client. If you want to disconnect the account use the Disconnect button in the manager or simply exit the client.",
                Path = "pack://application:,,,/LolAccountManager;component/Resources/HelpWindow/HelpWindowScreenshot5.png"
            },
            new ScreenshotInfo
            {
                Description = "Before you close UserGuide, go to the settings and set the path to the League of Legends client. (If you have not done this already)",
                Path = "pack://application:,,,/LolAccountManager;component/Resources/HelpWindow/HelpWindowScreenshot6.png"
            },
        };

        private int _currentScreenshotIndex;

        public HelpWindow()
        {
            InitializeComponent();
            UpdateScreenshot();
        }

        private void PreviousScreenshot_Click(object sender, RoutedEventArgs e)
        {
            if (_currentScreenshotIndex > 0)
            {
                _currentScreenshotIndex--;
                UpdateScreenshot();
            }
        }

        private void NextScreenshot_Click(object sender, RoutedEventArgs e)
        {
            if (_currentScreenshotIndex < _screenshots.Count - 1)
            {
                _currentScreenshotIndex++;
                UpdateScreenshot();
            }
        }

        private void UpdateScreenshot()
        {
            UpdateButtonState();
            // fade in/out
            ScreenshotImage.BeginAnimation(OpacityProperty, null);
            ScreenshotImage.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 0.7, new Duration(TimeSpan.FromSeconds(0.5))));

            ScreenShotDescription.Text = _screenshots[_currentScreenshotIndex].Description;
            ScreenShotDescription.BeginAnimation(OpacityProperty, null);
            ScreenShotDescription.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.5))));

            var imagePath = _screenshots[_currentScreenshotIndex].Path;
            ScreenshotImage.Source = new BitmapImage(new Uri(imagePath));
        }

        private void UpdateButtonState()
        {
            PreviousButton.IsEnabled = _currentScreenshotIndex > 0;
            PreviousButton.Opacity = PreviousButton.IsEnabled ? 1 : 0.5;
            NextButton.IsEnabled = _currentScreenshotIndex < _screenshots.Count - 1;
            NextButton.Opacity = NextButton.IsEnabled ? 1 : 0.5;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // fade out
            BeginAnimation(OpacityProperty, null);
            var fadeOutAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.2)));
            fadeOutAnimation.Completed += (s, _) => Close();
            BeginAnimation(OpacityProperty, fadeOutAnimation);
        }

        private void OnDraggableTabPanelMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}
