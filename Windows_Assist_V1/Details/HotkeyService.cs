using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Windows_Assist_V1
{
    public class HotkeyService : IDisposable
    {
        // Windows API constants
        private const int WM_HOTKEY = 0x0312;

        // Hotkey ID for our application
        private const int HOTKEY_ID = 9000;

        // Default key modifiers
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        // Virtual key code
        private const uint VK_F8 = 0x77; // F8 key

        // Window handle for WPF
        private IntPtr _windowHandle;
        private HwndSource _source;

        // Action to execute when hotkey is pressed
        public Action ToggleApplicationAction { get; set; }

        // Custom hotkey settings (can be expanded to be configurable)
        public uint KeyModifiers { get; set; } = MOD_ALT | MOD_CONTROL; // Alt+Ctrl
        public uint VirtualKey { get; set; } = VK_F8; // F8 key

        public void RegisterHotKey(Window window)
        {
            // Get the window handle
            _windowHandle = new WindowInteropHelper(window).Handle;

            // Add hook for the window messages
            _source = HwndSource.FromHwnd(_windowHandle);
            _source?.AddHook(HwndHook);

            // Register the hotkey with Windows
            bool registered = RegisterHotKey(
                _windowHandle,
                HOTKEY_ID,
                KeyModifiers,
                VirtualKey);

            if (!registered)
            {
                int error = Marshal.GetLastWin32Error();
                MessageBox.Show($"Failed to register hotkey (Error: {error}). Another application might be using this key combination.",
                               "Hotkey Registration Failed",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                // Hotkey was pressed, execute the toggle action
                ToggleApplicationAction?.Invoke();
                handled = true;
            }

            return IntPtr.Zero;
        }

        public void UnregisterHotkey()
        {
            // Unregister the hotkey when it's no longer needed
            if (_windowHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
            }

            // Detach the window hook
            _source?.RemoveHook(HwndHook);
            _source = null;
        }

        public void Dispose()
        {
            UnregisterHotkey();
        }

        #region Native Methods

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        #endregion
    }
}