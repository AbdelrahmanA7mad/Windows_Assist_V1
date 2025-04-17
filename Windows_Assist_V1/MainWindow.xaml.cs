using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
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
                        StatusText.Text = "Executing command...";
                        ExecutePowerShell(command);

                        // Show success animation
                        ProcessingSpinner.Visibility = Visibility.Collapsed;
                        ShowStatusNotification("Command executed successfully", Colors.MediumSeaGreen);
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
            new {
                parts = new[] {
                    new {
                        text = $@"You are a PowerShell expert focused on generating precise Windows automation commands. Generate ONLY executable code without explanations.

USER REQUEST: ""{prompt}""

ESSENTIAL GUIDELINES:
1. Return ONLY executable PowerShell code - no comments, quotes, explanations, or markdown
2. For file paths, ALWAYS use environment variables or absolute paths (e.g., [Environment]::GetFolderPath('Desktop'), $env:USERPROFILE)
3. Handle file existence checks with Test-Path before operations
4. For wallpaper changes: Use the full sequence (validate file, set registry keys, refresh settings)
5. For system settings: Include ALL required steps in the proper sequence
6. Use try/catch blocks for potential errors
7. For desktop files: Always use proper path resolution, not relative paths

WALLPAPER CHANGE PATTERN:
- Verify the image exists
- Use absolute path with environment variables
- Set proper registry keys in 'HKCU:\Control Panel\Desktop'
- Update 'WallpaperStyle' and 'TileWallpaper' keys
- Force a refresh using the user32.dll SendMessageTimeout function
- Handle potential errors

CHECK YOUR OUTPUT: Ensure your command is complete, properly formatted, and executable directly in PowerShell without any modifications."
                    }
                }
            }
        }
            };

            // Rest of the implementation remains the same
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
            MessageBox.Show(command);
            Process.Start(new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-Command \"{command.Replace("\"", "\\\"")}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
    }
}