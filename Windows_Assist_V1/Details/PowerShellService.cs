using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Media;

namespace Windows_Assist_V1
{
    public class PowerShellService
    {
        public delegate void StatusNotificationHandler(string message, Color color);

        public void ExecuteCommand(string command, StatusNotificationHandler notificationCallback = null)
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

                    notificationCallback?.Invoke($"Error: {errorMessage}", Colors.Crimson);
                }
            }
            catch (Exception ex)
            {
                notificationCallback?.Invoke($"Execution error: {ex.Message}", Colors.Crimson);
            }
        }
    }
}