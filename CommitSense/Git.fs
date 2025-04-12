namespace CommitSense

open System.Diagnostics

module Git =
    let private timeoutMs = 10000

    let getDiff () =
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

        if not (proc.WaitForExit timeoutMs) then
            proc.Kill()
            failwith "Git diff command timed out"

        if proc.ExitCode <> 0 then
            failwith $"Git diff failed with exit code {proc.ExitCode}. Error: {error}"

        output

    let commit (message: string) =
        let startInfo =
            ProcessStartInfo(
                FileName = "git",
                Arguments = $"commit -m \"{message}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            )

        use proc = new Process(StartInfo = startInfo)
        proc.Start() |> ignore

        proc.WaitForExit()

        if proc.ExitCode <> 0 then
            failwith $"Git commit failed with exit code {proc.ExitCode}."
