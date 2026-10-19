namespace DiscussionBoard.Tests.Infrastructure;

public sealed class ProjectFixture
{
    private const string CourseManifest = ".course/course-updates.json";

    public string RepositoryRoot { get; }
    public string ApplicationRoot { get; }
    public string TestRoot { get; }
    public string ApplicationProjectFile => Path.Combine(ApplicationRoot, "DiscussionBoard.csproj");

    public ProjectFixture()
    {
        RepositoryRoot = FindRepositoryRoot();
        ApplicationRoot = Path.Combine(RepositoryRoot, "src", "DiscussionBoard");
        TestRoot = Path.Combine(RepositoryRoot, "tests", "DiscussionBoard.Tests");
        ValidateRepository();
    }

    public string GetRepositoryPath(string relativePath)
    {
        return Path.Combine(RepositoryRoot, Normalize(relativePath));
    }

    public string GetApplicationPath(string relativePath)
    {
        return Path.Combine(ApplicationRoot, Normalize(relativePath));
    }

    public bool RepositoryFileExists(string relativePath)
    {
        return File.Exists(GetRepositoryPath(relativePath));
    }

    public bool ApplicationFileExists(string relativePath)
    {
        return File.Exists(GetApplicationPath(relativePath));
    }

    public string ReadRepositoryFile(string relativePath)
    {
        return File.ReadAllText(GetRepositoryPath(relativePath));
    }

    public string ReadApplicationFile(string relativePath)
    {
        return File.ReadAllText(GetApplicationPath(relativePath));
    }

    public IEnumerable<string> FindApplicationFiles(string searchPattern)
    {
        return Directory.EnumerateFiles(ApplicationRoot, searchPattern, SearchOption.AllDirectories);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string marker = Path.Combine(directory.FullName, Normalize(CourseManifest));

            if (File.Exists(marker))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Unable to locate the DiscussionBoard repository root. " +
            $"Expected to find '{CourseManifest}' in this directory or a parent directory.");
    }

    private void ValidateRepository()
    {
        string[] requiredPaths =
        [
            ".course/course-updates.json",
            "Directory.Build.props",
            ".editorconfig",
            ".noai",
            "src/DiscussionBoard/DiscussionBoard.csproj"
        ];

        string[] missing = requiredPaths
            .Where(path => !RepositoryFileExists(path))
            .ToArray();

        if (missing.Length == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "The DiscussionBoard repository is missing required starter infrastructure:" +
            Environment.NewLine +
            string.Join(Environment.NewLine, missing.Select(path => $"  - {path}")));
    }

    private static string Normalize(string path)
    {
        return path.Replace('/', Path.DirectorySeparatorChar);
    }
}
