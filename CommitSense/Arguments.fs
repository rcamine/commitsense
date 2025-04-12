namespace CommitSense

open Argu
open Spectre.Console

type CliArgument =
    | [<AltCommandLine("-k")>] Api_Key of string
    | [<AltCommandLine("-v")>] Version

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Api_Key _ -> "OpenAI API key"
            | Version -> "Display version information"

module Arguments =
    let createParser () = ArgumentParser.Create<CliArgument>()

    let getApiKey (results: ParseResults<CliArgument>) =
        match System.Environment.GetEnvironmentVariable "OPENAI_API_KEY" with
        | key when not (System.String.IsNullOrWhiteSpace key) -> Some key
        | _ ->
            match results.TryGetResult Api_Key with
            | Some key when not (System.String.IsNullOrWhiteSpace key) -> Some key
            | _ -> None
