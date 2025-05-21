using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;
using WPF = System.Windows;
using Controls = System.Windows.Controls;
using Media = System.Windows.Media;

namespace Windows_Assist_V1
{
    public class SecurityService
    {
        private readonly List<string> _dangerousOperations = new()
        {
            @"rm\s+-r",
            @"rmdir\s+/s",
            @"del\s+/s",
            @"format",
            @"shutdown",
            @"restart-computer",
            @"stop-computer",
            @"remove-item\s+-recurse",
            @"remove-item\s+-force",
            @"remove-item\s+-path",
            @"remove-item\s+-literalpath"
        };

        public bool IsPotentiallyDangerousOperation(string command)
        {
            foreach (var operation in _dangerousOperations)
            {
                if (Regex.IsMatch(command, operation, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> ShowDangerousOperationAlert(string command)
        {
            var result = await WPF.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new Window
                {
                    Title = "Security Warning",
                    Width = 500,
                    Height = 300,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    Background = new SolidColorBrush(MediaColor.FromRgb(45, 45, 45)),
                    WindowStyle = WindowStyle.ToolWindow
                };

                var mainGrid = new Grid
                {
                    Margin = new Thickness(20)
                };

                var warningIcon = new TextBlock
                {
                    Text = "⚠️",
                    FontSize = 48,
                    HorizontalAlignment = WPF.HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 20)
                };

                var warningText = new TextBlock
                {
                    Text = "This command may be potentially dangerous:",
                    Foreground = new SolidColorBrush(MediaColor.FromRgb(255, 255, 255)),
                    FontSize = 16,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = WPF.HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var commandText = new TextBlock
                {
                    Text = command,
                    Foreground = new SolidColorBrush(MediaColor.FromRgb(255, 100, 100)),
                    FontFamily = new Media.FontFamily("Consolas"),
                    FontSize = 14,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = WPF.HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 20)
                };

                var buttonPanel = new StackPanel
                {
                    Orientation = Controls.Orientation.Horizontal,
                    HorizontalAlignment = WPF.HorizontalAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                };

                var confirmButton = new Controls.Button
                {
                    Content = "Execute",
                    Width = 100,
                    Height = 30,
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = new SolidColorBrush(MediaColor.FromRgb(200, 50, 50)),
                    Foreground = new SolidColorBrush(MediaColor.FromRgb(255, 255, 255)),
                    BorderThickness = new Thickness(0)
                };

                var cancelButton = new Controls.Button
                {
                    Content = "Cancel",
                    Width = 100,
                    Height = 30,
                    Background = new SolidColorBrush(MediaColor.FromRgb(60, 60, 60)),
                    Foreground = new SolidColorBrush(MediaColor.FromRgb(255, 255, 255)),
                    BorderThickness = new Thickness(0)
                };

                bool? dialogResult = null;

                confirmButton.Click += (s, e) =>
                {
                    dialogResult = true;
                    dialog.Close();
                };

                cancelButton.Click += (s, e) =>
                {
                    dialogResult = false;
                    dialog.Close();
                };

                buttonPanel.Children.Add(confirmButton);
                buttonPanel.Children.Add(cancelButton);

                mainGrid.Children.Add(warningIcon);
                mainGrid.Children.Add(warningText);
                mainGrid.Children.Add(commandText);
                mainGrid.Children.Add(buttonPanel);

                dialog.Content = mainGrid;
                dialog.ShowDialog();

                return dialogResult ?? false;
            });

            return result;
        }
    }
}