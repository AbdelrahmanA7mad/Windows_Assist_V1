using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Windows_Assist_V1
{
    public class SecurityService
    {
        public bool IsPotentiallyDangerousOperation(string command)
        {
            // List of potentially dangerous PowerShell commands and operations
            string[] dangerousPatterns = new string[]
            {
                @"\bRemove-Item\b", @"\bRm\b", @"\bDel\b", @"\bDelete\b",    // File/directory deletion
                @"\bFormat-Volume\b", @"\bFormat\b",                         // Disk formatting
                @"\bRestart-Computer\b", @"\bShutdown\b",                   // System restart/shutdown
            };

            // Check if command contains any dangerous patterns
            foreach (string pattern in dangerousPatterns)
            {
                if (Regex.IsMatch(command, pattern, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public Task<bool> ShowDangerousOperationAlert(string command)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            // Create and configure the confirmation dialog
            var confirmationWindow = new Window
            {
                Title = "Security Alert",
                Width = 500,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = new SolidColorBrush(Colors.Transparent)
            };

            // Create main border with shadow
            var mainBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(35, 35, 35)),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(10)
            };

            mainBorder.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 315,
                ShadowDepth = 5,
                Opacity = 0.6,
                BlurRadius = 15
            };

            // Create layout grid
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });

            // Create header
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Background = new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                Height = 50
            };

            var headerText = new TextBlock
            {
                Text = "⚠️ WARNING: Potentially Dangerous Operation",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };

            headerPanel.Children.Add(headerText);
            Grid.SetRow(headerPanel, 0);
            grid.Children.Add(headerPanel);

            // Create content panel
            var contentPanel = new StackPanel
            {
                Margin = new Thickness(20)
            };

            var warningText = new TextBlock
            {
                Text = "The following operation could modify your system in significant ways:",
                Foreground = new SolidColorBrush(Colors.WhiteSmoke),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 15)
            };

            var questionText = new TextBlock
            {
                Text = "Are you sure you want to proceed with this operation?",
                Foreground = new SolidColorBrush(Colors.Yellow),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 15, 0, 0)
            };
            contentPanel.Children.Add(warningText);
            contentPanel.Children.Add(questionText);
            Grid.SetRow(contentPanel, 1);
            grid.Children.Add(contentPanel);

            // Create button panel
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(20, 0, 20, 20)
            };

            var yesButton = new Button
            {
                Content = "Yes, Do It",
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(10, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                Foreground = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0)
            };

            var noButton = new Button
            {
                Content = "No, Cancel ",
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(10, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                Foreground = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0)
            };

            yesButton.Click += (s, e) =>
            {
                confirmationWindow.Close();
                taskCompletionSource.SetResult(true);
            };

            noButton.Click += (s, e) =>
            {
                confirmationWindow.Close();
                taskCompletionSource.SetResult(false);
            };

            buttonPanel.Children.Add(noButton);
            buttonPanel.Children.Add(yesButton);
            Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            // Add grid to border
            mainBorder.Child = grid;

            // Set main content of window
            confirmationWindow.Content = mainBorder;

            // Make window draggable
            headerPanel.MouseLeftButtonDown += (s, e) =>
            {
                confirmationWindow.DragMove();
            };

            // Show dialog
            confirmationWindow.Show();

            return taskCompletionSource.Task;
        }
    }
}