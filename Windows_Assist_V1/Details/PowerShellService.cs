using System;
using System.Diagnostics;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;

namespace Windows_Assist_V1
{
    public class PowerShellService
    {
        public delegate void StatusNotificationHandler(string message, MediaColor color);

        public void ExecuteCommand(string command, StatusNotificationHandler statusCallback)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{command}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        statusCallback?.Invoke("Command executed successfully", MediaColor.FromRgb(0, 128, 0)); // Green
                    }
                    else
                    {
                        statusCallback?.Invoke($"Error: {error}", MediaColor.FromRgb(255, 0, 0)); // Red
                    }
                }
            }
            catch (Exception ex)
            {
                statusCallback?.Invoke($"Error executing command: {ex.Message}", MediaColor.FromRgb(255, 0, 0)); // Red
            }
        }
    }
}