using System.Diagnostics;

namespace DiscussionBoard.Tests.Infrastructure;

public sealed class GitRepositoryInspector
{
    private readonly string _repositoryRoot;

    public GitRepositoryInspector(string repositoryRoot)
    {
        _repositoryRoot = repositoryRoot;
    }

    public bool IsIgnored(string relativePath)
    {
        ProcessResult result = Run("check-ignore", "--quiet", "--", relativePath);
        return result.ExitCode == 0;
    }

    public bool IsTracked(string relativePath)
    {
        ProcessResult result = Run("ls-files", "--error-unmatch", "--", relativePath);
        return result.ExitCode == 0;
    }

    private ProcessResult Run(params string[] arguments)
    {
        ProcessStartInfo startInfo = new("git")
        {
            WorkingDirectory = _repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Unable to start Git. Ensure Git is installed and available on PATH.");

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new ProcessResult(process.ExitCode, output, error);
    }

    private sealed record ProcessResult(int ExitCode, string Output, string Error);
}
