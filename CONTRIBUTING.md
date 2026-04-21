# Contributing to GameModeAPI

Thank you for your interest in contributing to GameModeAPI!

## How to Contribute

1. **Open an Issue** — Before making changes, please open an issue to discuss what you'd like to do.
2. **Fork & Branch** — Fork the repository and create a feature branch from `main`.
3. **Code Style** — Follow the existing code style. Use the `.editorconfig` settings.
4. **Test** — Make sure all tests pass with `dotnet test`.
5. **Pull Request** — Submit a PR with a clear description of what you changed and why.

## Development Setup

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- An MQTT broker (e.g., [Mosquitto](https://mosquitto.org/)) for integration testing
- A game launcher (Steam, etc.) for manual testing

### Building
```bash
dotnet build
```

### Running Tests
```bash
dotnet test
```

### Running Locally
```bash
dotnet run --project src/GameModeAPI
```

## Reporting Bugs

Please open a GitHub issue with:
- Your Windows version
- .NET runtime version
- Steps to reproduce
- Expected vs actual behavior
- Relevant log output

## License

By contributing, you agree that your contributions will be licensed under the GPL-3.0 License.
