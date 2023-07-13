using System.Collections.Generic;
using System.IO;

namespace TechTonic.Monorepo.Validation.UnitTests.Helpers;

public static class MonoRepoDirectoryStructure
{
    public static readonly string ArchitectureDesignRecordDirectoryPathSuffix = $"Docs{Path.DirectorySeparatorChar}Architecture{Path.DirectorySeparatorChar}Decisions";

    // AUTHOR AND PR REVIEWER: THIS SHOULD VERY RARELY CHANGE - PLEASE GET A LEAD TO REVIEW THIS CHANGE BEFORE APPROVING
    public static IReadOnlyCollection<string> TopLevelDirectoryNames => new[]
    {
        SpecialDirectories.Git,
        SpecialDirectories.DotNetToolsConfig,
        SpecialDirectories.AzureDevopsConfig,
        "Build",
        "Docs",
        "Domains",
        "Components",
        "Monorepo",
        "Tests",
        "Pipelines"
    };
}
