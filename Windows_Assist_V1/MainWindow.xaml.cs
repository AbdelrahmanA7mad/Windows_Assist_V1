using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Windows_Assist_V1
{
    public partial class MainWindow : Window
    {
     
        private static readonly HttpClient client = new HttpClient();
        private readonly string apiKey = "AIzaSyDagl4Y255fV_j2ikxzpGKbbrvFKcH6HAA";

        public MainWindow()
        {
            InitializeComponent();

            var dropShadowEffect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 315,
                ShadowDepth = 5,
                Opacity = 0.6,
                BlurRadius = 15
            };

            MainBorder.Effect = dropShadowEffect;

            // Set window to be draggable
            this.MouseLeftButtonDown += (s, e) =>
            {
                this.DragMove();
            };

        }

        private async void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(UserInput.Text))
            {
                // Show processing animation
                ProcessingSpinner.Visibility = Visibility.Visible;
                StatusText.Text = "Processing...";

                string userText = UserInput.Text;

                try
                {
                    string command = await GetPowerShellCommandFromGemini(userText);
                    if (!string.IsNullOrEmpty(command))
                    {
                        // Check if command contains potentially dangerous operations
                        if (IsPotentiallyDangerousOperation(command))
                        {
                            ProcessingSpinner.Visibility = Visibility.Collapsed;
                            // Show confirmation dialog
                            bool confirmed = await ShowDangerousOperationAlert(command);
                            if (confirmed)
                            {
                                ProcessingSpinner.Visibility = Visibility.Visible;
                                StatusText.Text = "Executing command...";
                                ExecutePowerShell(command);
                                ProcessingSpinner.Visibility = Visibility.Collapsed;
                                ShowStatusNotification("Command executed successfully", Colors.MediumSeaGreen);
                            }
                            else
                            {
                                ShowStatusNotification("Operation cancelled by user", Colors.Orange);
                            }
                        }
                        else
                        {
                            // Safe command, execute directly
                            StatusText.Text = "Executing command...";
                            ExecutePowerShell(command);
                            ProcessingSpinner.Visibility = Visibility.Collapsed;
                            ShowStatusNotification("Command executed successfully", Colors.MediumSeaGreen);
                        }
                    }
                    else
                    {
                        ProcessingSpinner.Visibility = Visibility.Collapsed;
                        ShowStatusNotification("Failed to generate command", Colors.Crimson);
                    }
                }
                catch (Exception ex)
                {
                    ProcessingSpinner.Visibility = Visibility.Collapsed;
                    ShowStatusNotification("Error: " + ex.Message, Colors.Crimson);
                }

                UserInput.Clear();
            }
        }
        private bool IsPotentiallyDangerousOperation(string command)
        {
            // List of potentially dangerous PowerShell commands and operations
            string[] dangerousPatterns = new string[]
            {
                @"\bRemove-Item\b", @"\bRm\b", @"\bDel\b", @"\bDelete\b",    // File/directory deletion
                @"\bFormat-Volume\b", @"\bFormat\b",                         // Disk formatting
                @"\bRemove-Service\b",                                      // Removing services
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
        private Task<bool> ShowDangerousOperationAlert(string command)
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
        private void ShowStatusNotification(string message, Color color)
        {
            StatusText.Text = message;
            StatusText.Foreground = new SolidColorBrush(color);

            // Create fade animation
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
            StatusText.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            // Reset status after delay
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

            timer.Tick += (s, e) =>
            {
                DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
                fadeOut.Completed += (sender, args) =>
                {
                    StatusText.Text = "Ready";
                    StatusText.Foreground = new SolidColorBrush(Colors.Gray);
                    StatusText.Opacity = 1;
                };

                StatusText.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                timer.Stop();
            };

            timer.Start();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
private async Task<string> GetPowerShellCommandFromGemini(string prompt)
{
    var requestBody = new
    {
        contents = new[]
        {
            new
            {
                parts = new[]
                {
                    new
                    {
                        text = $@"
You are an elite AI specializing in generating **perfect and highly optimized PowerShell automation scripts** for Windows environments. Your sole mission is to generate **unblemished, executable PowerShell code**—no commentary, explanations, or markdown. Just the script itself, designed for flawless execution.

USER TASK: ""{prompt}""

STRICT SCRIPTING GUIDELINES:
1. The output MUST ONLY include valid, error-free PowerShell code — NO markdown, comments, or explanations.
2. Resolve all file paths using environment variables (`$env:USERPROFILE`, `[Environment]::GetFolderPath('Desktop')`, etc.)—AVOID relative paths at all costs.
3. Always verify file existence with `Test-Path` before performing any operations that involve file or directory access.
4. For operations such as altering system settings, changing wallpapers, or modifying configurations, provide the FULL, explicit sequence—no shortcuts, no omissions.
5. Use comprehensive `try-catch` blocks around potentially risky or destructive operations to ensure proper error handling and script resilience.
6. Ensure the script is **complete, executable**, and ready to run directly in PowerShell without requiring any additional steps or modifications.
7. Handle edge cases and exceptions proactively, ensuring the script will function under all normal and edge-case conditions.

STRICTLY FOLLOW THESE RULES AND RETURN **ONLY** THE FINAL, EXECUTABLE PowerShell SCRIPT. DO NOT OMIT ANY DETAILS."
                    }
                }
            }}

        };

    var requestJson = JsonSerializer.Serialize(requestBody);
    var request = new HttpRequestMessage
    {
        Method = HttpMethod.Post,
        RequestUri = new Uri("https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=" + apiKey),
        Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
    };

    var response = await client.SendAsync(request);
    var responseJson = await response.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(responseJson);
    var command = doc.RootElement
        .GetProperty("candidates")[0]
        .GetProperty("content")
        .GetProperty("parts")[0]
        .GetProperty("text")
        .GetString();

    return command?.Trim();
}


        private void ExecutePowerShell(string command)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell",
                        Arguments = $"-Command \"{command.Replace("\"", "\\\"")}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                // Capture output and error
                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                        output.AppendLine(args.Data);
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                        error.AppendLine(args.Data);
                };

                // Start process and begin reading output
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                // Check for errors
                if (process.ExitCode != 0 || error.Length > 0)
                {
                    string errorMessage = error.Length > 0
                        ? error.ToString()
                        : $"Command failed with exit code: {process.ExitCode}";

                    ShowStatusNotification($"Error: {errorMessage}", Colors.Crimson);
                }
                else
                {
                    ShowStatusNotification("Command executed successfully", Colors.MediumSeaGreen);
                }
            }
            catch (Exception ex)
            {
                ShowStatusNotification($"Execution error: {ex.Message}", Colors.Crimson);
            }
        }
    
}
}