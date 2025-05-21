# Windows Assistant

A modern, AI-powered Windows utility that helps you execute PowerShell commands through natural language input. Built with WPF and .NET 8.0.

## Features

- 🤖 AI-powered command generation using Google's Gemini API
- ⌨️ Global hotkey (Alt + Space) for quick access
- 🔒 Security checks for potentially dangerous operations
- 📋 Command history management
- 🎯 System tray integration
- 🎨 Modern, clean UI with animations
- 📝 Paste functionality for quick input
- ⚡ Real-time command execution feedback

## Screenshots

*[Add screenshots of your application here]*

## Requirements

- Windows 10 or later
- .NET 8.0 Runtime
- Google Gemini API key

## Installation

1. Download the latest release from the [Releases](https://github.com/yourusername/Windows_Assist_V1/releases) page
2. Extract the ZIP file to your desired location
3. Run `Windows_Assist_V1.exe`

## Configuration

1. Get your Google Gemini API key from [Google AI Studio](https://makersuite.google.com/app/apikey)
2. The application will prompt you to enter your API key on first run

## Usage

1. Press `Alt + Space` to open the application
2. Type your request in natural language
3. The AI will generate the appropriate PowerShell command
4. Review and execute the command
5. For dangerous operations, a security confirmation dialog will appear

## Development

### Prerequisites

- Visual Studio 2022 or later
- .NET 8.0 SDK
- Windows SDK

### Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/Windows_Assist_V1.git
   ```

2. Open the solution in Visual Studio

3. Restore NuGet packages

4. Build the solution

### Project Structure

- `MainWindow.xaml` - Main UI layout
- `MainWindow.xaml.cs` - Main window logic
- `Details/`
  - `GeminiService.cs` - AI command generation service
  - `PowerShellService.cs` - PowerShell command execution
  - `SecurityService.cs` - Security checks and alerts

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**Abdelrhaman Ahmed**

## Acknowledgments

- Google Gemini API for AI capabilities
- WPF for the modern UI framework
- .NET 8.0 for the runtime environment

## Support

If you encounter any issues or have questions, please:
1. Check the [Issues](https://github.com/yourusername/Windows_Assist_V1/issues) page
2. Create a new issue if your problem isn't already listed

## Roadmap

- [ ] Add command templates
- [ ] Implement command favorites
- [ ] Add custom hotkey configuration
- [ ] Support for multiple AI providers
- [ ] Command execution history export
- [ ] Dark/Light theme support

---

Made with ❤️ by Abdelrhaman Ahmed 