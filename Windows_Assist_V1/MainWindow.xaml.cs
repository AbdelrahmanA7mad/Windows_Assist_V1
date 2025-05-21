using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Interop;
using Forms = System.Windows.Forms;
using WPF = System.Windows;
using System.IO;

namespace Windows_Assist_V1
{
    public partial class MainWindow : Window
    {
        private readonly GeminiService _geminiService;
        private readonly PowerShellService _powerShellService;
        private readonly SecurityService _securityService;
        private Forms.NotifyIcon? _notifyIcon;
        private HotKey? _hotKey;
        private readonly List<string> _commandHistory;
        private const int HOTKEY_ID = 1;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize services
            _geminiService = new GeminiService("AIzaSyDagl4Y255fV_j2ikxzpGKbbrvFKcH6HAA");
            _powerShellService = new PowerShellService();
            _securityService = new SecurityService();
            _commandHistory = new List<string>();

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

            // Initialize system tray icon
            InitializeSystemTray();

            // Register global hotkey (Alt + Space) after window is loaded
            this.Loaded += (s, e) => RegisterHotKey();

            // Handle window closing
            this.Closing += MainWindow_Closing;
        }

        private void InitializeSystemTray()
        {
            try
            {
                _notifyIcon = new Forms.NotifyIcon();
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bot.ico");
                
                if (!File.Exists(iconPath))
                {
                    // Fallback to embedded resource if file not found
                    using var stream = GetType().Assembly.GetManifestResourceStream("Windows_Assist_V1.bot.ico");
                    if (stream != null)
                    {
                        _notifyIcon.Icon = new Icon(stream);
                    }
                    else
                    {
                        // If all else fails, use a default icon
                        _notifyIcon.Icon = SystemIcons.Application;
                    }
                }
                else
                {
                    _notifyIcon.Icon = new Icon(iconPath);
                }

                _notifyIcon.Text = "PowerShell Assistant";
                _notifyIcon.Visible = true;

                var contextMenu = new Forms.ContextMenuStrip();
                contextMenu.Items.Add("Show", null, (s, e) => ShowWindow());
                contextMenu.Items.Add("Settings", null, (s, e) => ShowSettings());
                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Exit", null, (s, e) => Close());

                _notifyIcon.ContextMenuStrip = contextMenu;
                _notifyIcon.DoubleClick += (s, e) => ShowWindow();
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Error initializing system tray: {ex.Message}");
                // Use default icon as fallback
                _notifyIcon = new Forms.NotifyIcon
                {
                    Icon = SystemIcons.Application,
                    Text = "PowerShell Assistant",
                    Visible = true
                };
            }
        }

        private void RegisterHotKey()
        {
            try
            {
                var handle = new WindowInteropHelper(this).Handle;
                if (handle != IntPtr.Zero)
                {
                    var source = HwndSource.FromHwnd(handle);
                    source?.AddHook(HwndHook);

                    _hotKey = new HotKey(ModifierKeys.Alt, Key.Space, this);
                    _hotKey.Register();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering hotkey: {ex.Message}");
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                ShowWindow();
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void ShowWindow()
        {
            if (this.Visibility == Visibility.Hidden)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
            }
            else
            {
                this.Hide();
            }
        }

        private void ShowSettings()
        {
            // TODO: Implement settings window
            //MessageBox.Show("Settings window will be implemented soon!", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _notifyIcon?.Dispose();
            _hotKey?.Unregister();
        }

        private void AddToHistory(string command)
        {
            if (!string.IsNullOrWhiteSpace(command))
            {
                _commandHistory.Insert(0, command);
                if (_commandHistory.Count > 10) // Keep last 10 commands
                {
                    _commandHistory.RemoveAt(_commandHistory.Count - 1);
                }
                UpdateHistoryList();
            }
        }

        private void UpdateHistoryList()
        {
            HistoryListBox.Items.Clear();
            foreach (var command in _commandHistory)
            {
                HistoryListBox.Items.Add(command);
            }
        }

        private void HistoryListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (HistoryListBox.SelectedItem != null)
            {
                UserInput.Text = HistoryListBox.SelectedItem.ToString() ?? string.Empty;
                UserInput.CaretIndex = UserInput.Text.Length;
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            UserInput.Clear();
            UserInput.Focus();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        private async void UserInput_KeyDown(object sender, WPF.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(UserInput.Text))
            {
                string userText = UserInput.Text;
                AddToHistory(userText);

                // Show processing animation
                ProcessingSpinner.Visibility = Visibility.Visible;
                StatusText.Text = "Processing...";

                try
                {
                    string? command = await _geminiService.GetPowerShellCommandAsync(userText);
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
                                _powerShellService.ExecuteCommand(command, (msg, color) => ShowStatusNotification(msg, color));
                                ProcessingSpinner.Visibility = Visibility.Collapsed;
                                ShowStatusNotification("Command executed successfully", System.Windows.Media.Colors.MediumSeaGreen);
                            }
                            else
                            {
                                ShowStatusNotification("Operation cancelled by user", System.Windows.Media.Colors.Orange);
                            }
                        }
                        else
                        {
                            // Safe command, execute directly
                            StatusText.Text = "Executing command...";
                            _powerShellService.ExecuteCommand(command, (msg, color) => ShowStatusNotification(msg, color));
                            ProcessingSpinner.Visibility = Visibility.Collapsed;
                            ShowStatusNotification("Command executed successfully", System.Windows.Media.Colors.MediumSeaGreen);
                        }
                    }
                    else
                    {
                        ProcessingSpinner.Visibility = Visibility.Collapsed;
                        ShowStatusNotification("Failed to generate command", System.Windows.Media.Colors.Crimson);
                    }
                }
                catch (Exception ex)
                {
                    ProcessingSpinner.Visibility = Visibility.Collapsed;
                    ShowStatusNotification("Error: " + ex.Message, System.Windows.Media.Colors.Crimson);
                }

                UserInput.Clear();
            }
        }

        public void ShowStatusNotification(string message, System.Windows.Media.Color color)
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
                    StatusText.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Gray);
                    StatusText.Opacity = 1;
                };

                StatusText.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                timer.Stop();
            };

            timer.Start();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Clipboard.ContainsText())
            {
                UserInput.Text = System.Windows.Clipboard.GetText();
                UserInput.CaretIndex = UserInput.Text.Length;
            }
        }
    }

    public class HotKey
    {
        private const int WM_HOTKEY = 0x0312;
        private readonly int _id;
        private readonly IntPtr _handle;
        private readonly ModifierKeys _modifier;
        private readonly Key _key;

        public HotKey(ModifierKeys modifier, Key key, Window window)
        {
            _id = 1;
            _handle = new WindowInteropHelper(window).Handle;
            _modifier = modifier;
            _key = key;
        }

        public bool Register()
        {
            return RegisterHotKey(_handle, _id, (uint)_modifier, (uint)KeyInterop.VirtualKeyFromKey(_key));
        }

        public bool Unregister()
        {
            return UnregisterHotKey(_handle, _id);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}