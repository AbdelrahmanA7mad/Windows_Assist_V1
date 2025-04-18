using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Windows_Assist_V1
{
    public class GeminiService
    {
        private static readonly HttpClient _client = new HttpClient();
        private readonly string _apiKey;

        public GeminiService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<string> GetPowerShellCommandAsync(string prompt)
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
                    }
                }
            };

            var requestJson = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=" + _apiKey),
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            var response = await _client.SendAsync(request);
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
    }
}