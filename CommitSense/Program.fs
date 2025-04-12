namespace CommitSense

open System
open Spectre.Console
open Argu

module Program =
    type CommitError =
        | ApiKeyMissing
        | NoStagedChanges
        | GitDiffFailed of string
        | MessageGenerationFailed of string
        | CommitFailed of string

    type CommitContext =
        { ApiKey: string
          Diff: string
          Message: string }

    let private emptyContext = { ApiKey = ""; Diff = ""; Message = "" }

    let private formatError =
        function
        | ApiKeyMissing -> "No API key found. Please provide your OpenAI API key."
        | NoStagedChanges -> "No staged changes found. Please stage your changes first."
        | GitDiffFailed msg -> $"Failed to get git diff: {msg}"
        | MessageGenerationFailed msg -> $"Failed to generate commit message: {msg}"
        | CommitFailed msg -> $"Failed to commit changes: {msg}"

    let private showError error =
        AnsiConsole.MarkupLine $"[bold red]Error:[/] {formatError error}"

        match error with
        | ApiKeyMissing ->
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
        | _ -> ()

    let private confirmCommit (message: string) =
        let panel: Panel = Panel message
        panel.Header <- PanelHeader "Generated Commit Message"
        panel.Border <- BoxBorder.Rounded
        AnsiConsole.Write(panel)
        AnsiConsole.Confirm "Do you want to commit with this message?"

    let private showVersion () =
        AnsiConsole.MarkupLine "CommitSense version is [green]1.0.0[/]."

    let private handleUsageRequest (parser: ArgumentParser<CliArgument>) =
        parser.PrintUsage() |> printfn "%s"
        Ok()

    let private handleVersionRequest () =
        showVersion ()
        Ok()

    let private getApiKey (results: ParseResults<CliArgument>) context =
        match Arguments.getApiKey results with
        | Some key -> Ok { context with ApiKey = key }
        | None -> Error ApiKeyMissing

    let private getDiff context =
        try
            let diff = Git.getDiff ()

            if String.IsNullOrWhiteSpace diff then
                Error NoStagedChanges
            else
                Ok { context with Diff = diff }
        with ex ->
            Error(GitDiffFailed ex.Message)

    let private generateCommitMessage context =
        try
            let message =
                AnsiConsole
                    .Status()
                    .Start(
                        "Generating commit message...",
                        fun ctx ->
                            ctx.Spinner <- Spinner.Known.Dots
                            ctx.SpinnerStyle <- Style.Parse "green"
                            AIService.generateCommitMessage context.ApiKey context.Diff
                    )

            Ok { context with Message = message }
        with ex ->
            Error(MessageGenerationFailed ex.Message)

    let private commitChanges context =
        try
            if confirmCommit context.Message then
                Git.commit context.Message
                AnsiConsole.MarkupLine "[green]Changes committed successfully![/]"

            Ok()
        with ex ->
            Error(CommitFailed ex.Message)

    let private processCommitFlow (parseResults: ParseResults<CliArgument>) =
        emptyContext
        |> getApiKey parseResults
        |> Result.bind getDiff
        |> Result.bind generateCommitMessage
        |> Result.bind commitChanges

    [<EntryPoint>]
    let main argv =
        try
            let parser = Arguments.createParser ()
            let parseResults = parser.ParseCommandLine(inputs = argv, raiseOnUsage = false)

            if parseResults.IsUsageRequested then
                handleUsageRequest parser |> ignore
                0
            elif parseResults.Contains CliArgument.Version then
                handleVersionRequest () |> ignore
                0
            else
                match processCommitFlow parseResults with
                | Ok _ -> 0
                | Error error ->
                    showError error
                    1
        with ex ->
            AnsiConsole.MarkupLine $"[red]ERROR: {ex.Message}[/]"
            1
