# Contributing to OGT WatchTower

First off, thank you for considering contributing to OGT WatchTower! It's people like you that make this tool better for everyone.

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues to avoid duplicates. When you create a bug report, include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Provide specific examples** (code snippets, screenshots, etc.)
- **Describe the behavior you observed** and what you expected
- **Include your environment details** (Windows version, .NET version, etc.)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide a detailed description** of the suggested enhancement
- **Explain why this enhancement would be useful**
- **List any alternative solutions** you've considered

### Pull Requests

1. **Fork the repository** and create your branch from `main`
2. **Follow the coding standards** (see below)
3. **Test your changes** thoroughly
4. **Update documentation** if needed
5. **Write clear commit messages**
6. **Submit a pull request**

## Development Setup

### Prerequisites

- Windows 10/11 (64-bit)
- .NET 10.0 SDK
- Visual Studio 2022 or Visual Studio Code
- Git

### Setting Up Your Development Environment

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/Living-off-the-Land-Protection-Platform.git
cd Living-off-the-Land-Protection-Platform

# Add upstream remote
git remote add upstream https://github.com/ogtamimi/Living-off-the-Land-Protection-Platform.git

# Create a branch for your feature
git checkout -b feature/your-feature-name
```

### Building the Project

```bash
cd src
dotnet restore
dotnet build --configuration Debug
```

### Running Tests

```bash
# Run unit tests (when available)
dotnet test

# Test with attack simulations
cd ../simulate_attacks
./simulate_attack.bat
```

## Coding Standards

### C# Code Style

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use **4 spaces** for indentation (no tabs)
- Use **PascalCase** for class names and public members
- Use **camelCase** for private fields and local variables
- Prefix private fields with underscore: `_fieldName`
- Use **meaningful variable names**
- Add **XML documentation** for public APIs

Example:
```csharp
namespace OGT.WatchTower.Core
{
    /// <summary>
    /// Detects Living-off-the-Land Binary (LOLBin) attacks
    /// </summary>
    public class LolbinDetector
    {
        private readonly ILogger _logger;
        
        /// <summary>
        /// Analyzes a process for LOLBin behavior
        /// </summary>
        /// <param name="processInfo">Process information to analyze</param>
        /// <returns>Detection result with threat level</returns>
        public DetectionResult Analyze(ProcessInfo processInfo)
        {
            // Implementation
        }
    }
}
```

### XAML Style

- Use **4 spaces** for indentation
- Group related properties together
- Use **meaningful names** for x:Name attributes
- Follow WPF best practices

### Commit Messages

Follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

Examples:
```
feat(detection): Add support for custom Sigma rules

Implemented a new rule loader that supports user-defined Sigma rules
in the config/rules directory. Rules are validated on load and
automatically reloaded when files change.

Closes #123
```

```
fix(ui): Resolve memory leak in event monitoring panel

The DataGrid was not properly disposing of event subscriptions,
causing memory to accumulate over time. Added proper cleanup in
the Dispose method.

Fixes #456
```

## Project Structure

```
/
├── src/
│   ├── OGT.WatchTower.App/      # WPF Application
│   │   ├── Views/               # XAML views
│   │   ├── ViewModels/          # View models (MVVM)
│   │   └── Assets/              # Images, icons
│   ├── OGT.WatchTower.Core/     # Core detection engine
│   │   ├── Detection/           # Detection logic
│   │   ├── Monitor/             # Process monitoring
│   │   ├── Rules/               # Rule engines
│   │   └── Intelligence/        # Threat intelligence
│   └── config/                  # Configuration files
├── simulate_attacks/            # Attack simulation scripts
├── assets/                      # Documentation assets
└── docs/                        # Additional documentation
```

## Adding New Features

### Adding a New Detection Rule

1. Create a Sigma rule in `src/config/rules/`
2. Test with attack simulation
3. Update documentation
4. Submit PR

### Adding a New LOLBin Definition

1. Edit `src/config/lolbins.json`
2. Follow the existing format:
```json
{
  "name": "certutil.exe",
  "description": "Certificate utility that can download files",
  "techniques": ["T1105"],
  "suspicious_args": ["-urlcache", "-split", "-f"],
  "severity": "high"
}
```
3. Create a test case in `simulate_attacks/`
4. Submit PR

### Adding UI Features

1. Follow MVVM pattern
2. Use the existing design system
3. Ensure dark mode compatibility
4. Test on different screen resolutions
5. Update screenshots if needed

## Testing

### Manual Testing Checklist

- [ ] Application starts without errors
- [ ] Process monitoring works correctly
- [ ] Detections trigger as expected
- [ ] UI is responsive and displays correctly
- [ ] Dark mode works properly
- [ ] Settings persist correctly
- [ ] Attack simulations are detected

### Performance Testing

- Monitor memory usage during extended operation
- Ensure CPU usage remains reasonable
- Test with high process creation rates

## Documentation

When adding new features:

1. Update the README.md if needed
2. Add inline code comments for complex logic
3. Update XML documentation for public APIs
4. Add examples to the wiki (if applicable)

## Release Process

Maintainers will handle releases, but contributors should:

1. Ensure version numbers are updated (if applicable)
2. Update CHANGELOG.md with changes
3. Verify all tests pass
4. Ensure documentation is current

## Questions?

Feel free to open an issue with the `question` label if you need help or clarification.

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to OGT WatchTower! 🛡️
