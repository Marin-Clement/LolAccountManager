using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace LolAccountManager.View
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // fade out
            BeginAnimation(OpacityProperty, null);
            var fadeOutAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.2)));
            fadeOutAnimation.Completed += (s, _) => Close();
            BeginAnimation(OpacityProperty, fadeOutAnimation);
        }

        private void LinkTextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/Marin-Clement");
            e.Handled = true;
        }

        private void OnDraggableTabPanelMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}