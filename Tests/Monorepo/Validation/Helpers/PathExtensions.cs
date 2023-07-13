using System.IO;
using System.Linq;

namespace TechTonic.Monorepo.Validation.UnitTests.Helpers;

public static class PathExtensions
{
    public static bool DirectoryContainsFileType(this string directory, string extension) =>
        Directory.GetFiles(directory, $"*.{extension}", SearchOption.TopDirectoryOnly).Any();

    public static string CleanDirectorySeparators(this string path) => path
        .Replace('/', Path.DirectorySeparatorChar)
        .Replace('\\', Path.DirectorySeparatorChar);
}
