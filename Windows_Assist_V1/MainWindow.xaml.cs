using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Windows_Assist_V1
{
    public partial class MainWindow : Window
    {
        private readonly GeminiService _geminiService;
        private readonly PowerShellService _powerShellService;
        private readonly SecurityService _securityService;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize services
            _geminiService = new GeminiService("AIzaSyDagl4Y255fV_j2ikxzpGKbbrvFKcH6HAA");
            _powerShellService = new PowerShellService();
            _securityService = new SecurityService();

            // Apply drop shadow effect
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
                    string command = await _geminiService.GetPowerShellCommandAsync(userText);
                    if (!string.IsNullOrEmpty(command))
                    {
                        // Check if command contains potentially dangerous operations
                        if (_securityService.IsPotentiallyDangerousOperation(command))
                        {
                            ProcessingSpinner.Visibility = Visibility.Collapsed;
                            // Show confirmation dialog
                            bool confirmed = await _securityService.ShowDangerousOperationAlert(command);
                            if (confirmed)
                            {
                                ProcessingSpinner.Visibility = Visibility.Visible;
                                StatusText.Text = "Executing command...";
                                _powerShellService.ExecuteCommand(command, ShowStatusNotification);
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
                            _powerShellService.ExecuteCommand(command, ShowStatusNotification);
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

        public void ShowStatusNotification(string message, Color color)
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
    }
}