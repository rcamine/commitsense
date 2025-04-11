# CommitSense

> Generate meaningful commit messages using OpenAI

CommitSense is a command-line tool that analyzes your staged git changes and generates clear, conventional commit messages using OpenAI.

## Prerequisites

- Git
- .NET 8.0 Runtime
- OpenAI API key

## Installation

```bash
dotnet tool install --global commitsense
```

## Usage

```bash
# Using command line argument
commitsense --api-key your_api_key_here

# Using environment variable
export OPENAI_API_KEY=your_api_key_here
commitsense
```

## Options

- `--api-key <key>` - Set OpenAI API key
- `--help, -h` - Show help message
- `--version, -v` - Show version information

## Development

```bash
# Clone the repository
git clone https://github.com/yourusername/commitsense.git
cd commitsense

# Build the project
dotnet build

# Run in development
dotnet run -- --help
```

## License

MIT 