namespace CommitSense

open System
open Spectre.Console

module Program =
    let private confirmCommit (message: string) =
        let panel = Panel message
        panel.Header <- PanelHeader "Generated Commit Message"
        panel.Border <- BoxBorder.Rounded
        AnsiConsole.Write panel
        AnsiConsole.Confirm "Do you want to commit with this message?"

    let private showApiKeyError () =
        AnsiConsole.MarkupLine "[bold red]Error:[/] No OpenAI API key found."
        AnsiConsole.MarkupLine ""
        AnsiConsole.MarkupLine "[bold]How to provide your API key:[/]"
        AnsiConsole.MarkupLine "1. Use command line argument:"
        AnsiConsole.MarkupLine "   [blue]commitsense --api-key your_api_key_here[/]"
        AnsiConsole.MarkupLine ""
        AnsiConsole.MarkupLine "2. Set environment variable:"
        AnsiConsole.MarkupLine "   [blue]export OPENAI_API_KEY=your_api_key_here[/]"
        AnsiConsole.MarkupLine "   [blue]commitsense[/]"
        AnsiConsole.MarkupLine ""
        AnsiConsole.MarkupLine "[bold]You can get your API key from:[/]"
        AnsiConsole.MarkupLine "   [blue]https://platform.openai.com/api-keys[/]"

    let private showVersion () =
        AnsiConsole.MarkupLine "CommitSense version is [green]1.0.0[/]."

    [<EntryPoint>]
    let main argv =
        try
            let parser = Arguments.createParser ()
            let results = parser.ParseCommandLine(inputs = argv, raiseOnUsage = false)

            if results.IsUsageRequested then
                parser.PrintUsage() |> printfn "%s"
                exit 0

            if results.Contains CliArgument.Version then
                showVersion ()
                exit 0

            match Arguments.getApiKey results with
            | None ->
                showApiKeyError ()
                exit 1
            | Some apiKey ->
                let diff = Git.getDiff ()

                if String.IsNullOrWhiteSpace diff then
                    AnsiConsole.MarkupLine "[red]No staged changes found. Please stage your changes first.[/]"
                    exit 1

                let message =
                    AnsiConsole
                        .Status()
                        .Start(
                            "Generating commit message...",
                            fun ctx ->
                                ctx.Spinner <- Spinner.Known.Dots
                                ctx.SpinnerStyle <- Style.Parse "green"
                                AIService.generateCommitMessage apiKey diff
                        )

                if confirmCommit message then
                    Git.commit message
                    AnsiConsole.MarkupLine "[green]Changes committed successfully![/]"

                0
        with ex ->
            AnsiConsole.MarkupLine $"[red]ERROR: {ex.Message}[/]"
            1
