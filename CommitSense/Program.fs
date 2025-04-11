open System
open System.Diagnostics
open OpenAI_API
open OpenAI_API.Chat
open Spectre.Console

type Options =
    { ApiKey: string option
      ShowHelp: bool
      ShowVersion: bool }

let parseArgs (argv: string[]) =
    let rec parse args options =
        match args with
        | "--api-key" :: key :: rest -> parse rest { options with ApiKey = Some key }
        | "--help" :: rest -> parse rest { options with ShowHelp = true }
        | "-h" :: rest -> parse rest { options with ShowHelp = true }
        | "--version" :: rest -> parse rest { options with ShowVersion = true }
        | "-v" :: rest -> parse rest { options with ShowVersion = true }
        | _ :: rest -> parse rest options
        | [] -> options

    parse
        (List.ofArray argv)
        { ApiKey = None
          ShowHelp = false
          ShowVersion = false }

let showHelp () =
    printfn
        """
CommitSense - Generate meaningful commit messages using OpenAI

Usage:
  commitsense [options]

Options:
  --api-key <key>    OpenAI API key
  --help, -h         Show this help message
  --version, -v      Show version information

Environment Variables:
  OPENAI_API_KEY     OpenAI API key (alternative to --api-key)

Examples:
  commitsense --api-key sk-...
  commitsense (with OPENAI_API_KEY set)
"""

    0

let showVersion () =
    AnsiConsole.MarkupLine "[bold]CommitSense[/] version 1.0.0"
    0

let getApiKey options =
    match options.ApiKey with
    | Some key -> key
    | None ->
        match Environment.GetEnvironmentVariable "OPENAI_API_KEY" with
        | null ->
            failwith
                "API key not found. Please provide it using --api-key <key> or set OPENAI_API_KEY environment variable"
        | key when String.IsNullOrWhiteSpace key ->
            failwith
                "API key is empty. Please provide it using --api-key <key> or set OPENAI_API_KEY environment variable"
        | key -> key

let getGitDiff () =
    let startInfo =
        ProcessStartInfo(
            FileName = "git",
            Arguments = "diff --staged",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        )

    use proc = new Process(StartInfo = startInfo)
    proc.Start() |> ignore

    let output = proc.StandardOutput.ReadToEnd()
    let error = proc.StandardError.ReadToEnd()

    if not (proc.WaitForExit 5000) then
        proc.Kill()
        failwith "Git diff operation timed out after 5 seconds"

    if proc.ExitCode <> 0 then
        failwith $"Git diff failed with exit code {proc.ExitCode}. Error: {error}"

    output

let generateCommitMessage diff apiKey =
    let openAi = new OpenAIAPI(APIAuthentication(apiKey))

    let messages =
        [| ChatMessage(
               ChatMessageRole.System,
               "You are a helpful assistant that generates meaningful commit messages based on git diff content. Keep the messages concise and follow conventional commit format."
           )
           ChatMessage(ChatMessageRole.User, $"Please generate a commit message for the following changes:\n{diff}") |]

    let response =
        AnsiConsole
            .Status()
            .Start(
                "Generating commit message...",
                fun ctx ->
                    ctx.Spinner <- Spinner.Known.Dots
                    ctx.SpinnerStyle <- Style.Parse "green"
                    openAi.Chat.CreateChatCompletionAsync(messages).Result
            )

    response.Choices.[0].Message.Content

[<EntryPoint>]
let main argv =
    try
        let options = parseArgs argv

        match options with
        | { ShowHelp = true } -> showHelp ()
        | { ShowVersion = true } -> showVersion ()
        | _ ->
            let apiKey = getApiKey options
            let diff = getGitDiff ()

            match String.IsNullOrEmpty diff with
            | true ->
                AnsiConsole.MarkupLine "[red]No staged changes found. Please stage your changes first.[/]"
                1
            | false ->
                let commitMessage = generateCommitMessage diff apiKey
                AnsiConsole.MarkupLine "[green]Suggested commit message:[/]"
                AnsiConsole.MarkupLine $"[yellow]{commitMessage}[/]"

                match AnsiConsole.Confirm "Do you want to commit with this message?" with
                | false -> 0
                | true ->
                    let startInfo =
                        ProcessStartInfo(
                            FileName = "git",
                            Arguments = $"commit -m \"{commitMessage}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        )

                    use proc = new Process(StartInfo = startInfo)
                    proc.Start() |> ignore
                    proc.WaitForExit()

                    AnsiConsole.MarkupLine "[green]Changes committed successfully![/]"
                    0
    with ex ->
        AnsiConsole.MarkupLine $"[red]Error: {ex.Message}[/]"
        1
