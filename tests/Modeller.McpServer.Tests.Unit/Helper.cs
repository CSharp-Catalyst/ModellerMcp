namespace Modeller.McpServer.Tests.Unit;

internal static class Helper
{
    internal static string? GetSolutionFolder()
    {
        // Get the base directory of the application
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Search for the solution file (.sln) in parent directories
        var solutionFile = FindSolutionFile(baseDirectory);

        if (solutionFile != null)
            // If solution file is found, return its directory
            return Path.GetDirectoryName(solutionFile);

        // If no solution file is found, return the base directory
        return baseDirectory;
    }

    internal static string? FindSolutionFile(string? directory)
    {
        // Search for the solution file in the current directory and parent directories
        while (directory != null)
        {
            var solutionFile = Directory.GetFiles(directory, "*.sln").FirstOrDefault();
            if (solutionFile != null)
                return solutionFile;

            // Move to the parent directory
            directory = Path.GetDirectoryName(directory);
        }

        return null; // Solution file not found
    }
}
