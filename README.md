# CommitSense

A CLI tool that generates meaningful commit messages using OpenAI.

## Installation

### As a .NET CLI Tool

```bash
dotnet tool install --global commitsense
```

### As a Self-Contained Executable

Download the appropriate package for your platform from the [releases](https://github.com/yourusername/commitsense/releases) page:

- Windows: `commitsense-windows-x64.zip`
- Linux: `commitsense-linux-x64.tar.gz`
- macOS: `commitsense-macos-x64.tar.gz`

### Development

To run the tool during development:

```bash
# Show help
dotnet run -- --help

# Show version
dotnet run -- --version

# Use with API key
dotnet run -- --api-key sk-...
```

## Usage

### Command Line Options

```bash
# Show help
commitsense --help
commitsense -h

# Show version
commitsense --version
commitsense -v

# Use with API key
commitsense --api-key sk-...

# Use with environment variable
export OPENAI_API_KEY=sk-...
commitsense
```

### Environment Variables

- `OPENAI_API_KEY`: Your OpenAI API key (alternative to --api-key)

## Features

- Generates meaningful commit messages based on staged changes
- Supports conventional commit format
- Interactive confirmation before committing
- Cross-platform support (Windows, Linux, macOS)

## Requirements

- Git
- OpenAI API key
- .NET 8.0 Runtime (if using self-contained executable)

## License

MIT 