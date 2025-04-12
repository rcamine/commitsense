# CommitSense

> Generate meaningful commit messages using OpenAI

CommitSense is a command-line tool that analyzes your staged git changes and generates clear, conventional commit messages using OpenAI.

## Quick Start

1. Install:
   ```bash
   dotnet tool install --global commitsense
   ```

2. Set your OpenAI API key:
   ```bash
   # Option 1: Command line
   commitsense --api-key your_api_key_here

   # Option 2: Environment variable
   export OPENAI_API_KEY=your_api_key_here
   commitsense
   ```

3. Use in your git repository:
   ```bash
   git add .
   commitsense
   ```

## Configuration

### Unix-like systems (macOS/Linux)
Add to `~/.zshrc` or `~/.bashrc`:
```bash
export OPENAI_API_KEY="your_api_key_here"
```

### Windows (PowerShell)
Add to your PowerShell profile:
```powershell
$env:OPENAI_API_KEY = "your_api_key_here"
```

To find your PowerShell profile location:
```powershell
$PROFILE
```

## Requirements

- Git
- .NET 8.0 Runtime
- OpenAI API key

## Development

```bash
git clone https://github.com/rcamine/commitsense.git
cd commitsense
dotnet build
dotnet run -- --help
```

## License

MIT 