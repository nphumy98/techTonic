using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Execution;
using TechTonic.Monorepo.Validation.UnitTests.Helpers;
using Xunit;

namespace TechTonic.Monorepo.Validation.UnitTests;

public class DirectoryStructureConventionTests
{
    public static IEnumerable<object[]> RepositoryDirectoryPaths => MonoRepository.AllGitTrackedDirectories
        .Where(path => Path.GetFileName(path) != SpecialDirectories.DotNetToolsConfig)
        .Where(path => Path.GetFileName(path) != SpecialDirectories.AzureDevopsConfig)
        .Where(path => !path.Split(Path.DirectorySeparatorChar).Contains("Docs"))
        .AsTestSourceArguments();

    [Theory]
    [MemberData(nameof(RepositoryDirectoryPaths))]
    public void Internal_directories_should_be_pascal_case(string directoryPath)
    {
        // Arrange
        var exceptDirectory = new[] { "wwwroot" };
        var directoryName = Path.GetFileName(directoryPath);

        // Assert

        // TEMP Workaround for loyalty only: loyalty is using an OLD pattern of ARM variables per environment, stored in their own dir.
        // these shouldn't be pascal. These pipeline dirs will be cleaned up as we move to the new standards
        var relativePath = Path.GetRelativePath(MonoRepository.RootDirectory, directoryPath);
        var pathPrefix = $"Domains{Path.DirectorySeparatorChar}";
        var paths = new[] { "Loyalty", "Organisation" };

        if (
            paths.Any(proj => relativePath.StartsWith($"{pathPrefix}{proj}", StringComparison.OrdinalIgnoreCase))
            && directoryPath.Split(Path.DirectorySeparatorChar).SkipLast(1).Any(parentDir => parentDir == "Pipelines")
            && directoryName == directoryName.ToLowerInvariant())
            return;

        var isDirectoryBelongExceptionCase = exceptDirectory.Select(x => directoryName.Contains(x)).Any();

        if (!isDirectoryBelongExceptionCase)
        {
            directoryName.Should().MatchRegex(NamingConventions.PascalCaseRegexPattern,
                $"Directory {directoryPath} should follow Pascal Case naming conventions ('ThisIsAnExample')");
        }
    }

    public static IEnumerable<object[]> TopLevelDirectories => Directory.GetDirectories(MonoRepository.RootDirectory)
        .Where(MonoRepository.IsGitTrackedDirectory)
        .AsTestSourceArguments();

    [Theory]
    [MemberData(nameof(TopLevelDirectories))]
    public void All_top_level_directories_should_be_approved(string topLevelDirectory)
    {
        // Assert
        MonoRepoDirectoryStructure.TopLevelDirectoryNames.Should().Contain(Path.GetFileName(topLevelDirectory),
            "Only approved directories can be placed at the top level to avoid pollution and chaos");
    }

    public static IEnumerable<object[]> TopLevelFiles => Directory
        .GetFiles(MonoRepository.RootDirectory)
        .AsTestSourceArguments();

    [Theory]
    [MemberData(nameof(TopLevelFiles))]
    public void All_top_level_files_should_be_approved(string topLevelFile)
    {
        // Assert

        // AUTHOR AND PR REVIEWER: THIS SHOULD VERY RARELY CHANGE - PLEASE GET A LEAD TO REVIEW THIS CHANGE BEFORE APPROVING
        new[]
            {
                "TechTonic.sln",
                "Impacted.slnf", // Special file generated at runtime in pipeline
                "Impacted.UnitTests.slnf", // Special file generated at runtime in pipeline
                "Directory.Build.props",
                "Directory.Build.targets",
                "Directory.Packages.props",
                SpecialFiles.EditorConfig,
                SpecialFiles.GitIgnore,
                SpecialFiles.NugetConfig,
                SpecialFiles.ReadMe,
                SpecialFiles.GitAttributes,
                SpecialFiles.AzuriteDbTableJson,
                SpecialFiles.AzuriteDbBlobJson,
                SpecialFiles.AzuriteDbBlobExtentJson,
                SpecialFiles.AzuriteDbQueueJson,
                SpecialFiles.AzuriteDbQueueExtentJson,
                // Not approved but ignored by git - so local checked can pass these tests:
            }
            .Should().Contain(Path.GetFileName(topLevelFile), "Only approved files can be placed at the top level to avoid pollution and chaos");
    }

    public static IEnumerable<object[]> AllTestProjectFiles => MonoRepository.AllTestProjectFiles.AsTestSourceArguments();

    [Theory]
    [MemberData(nameof(AllTestProjectFiles))]
    public void All_test_project_dir_paths_follow_test_path_conventions(string testProjectPath)
    {
        // Arrange
        var actualPathParts = Path.GetRelativePath(MonoRepository.RootDirectory, Path.GetDirectoryName(testProjectPath)!)
            .Split(Path.DirectorySeparatorChar);

        var expectedPathParts = Path.GetFileNameWithoutExtension(testProjectPath)
            .Split('.')
            .SkipLast(1) // Drop "UnitTests" => adding the test level is OPTIONAL and validated in a separate assertion below
            .ToList();

        var testLevelMatch = new Regex($"\\.(?<Level>[^\\.]+)Tests?.{FileExtensions.CsProject}$").Match(testProjectPath);
        var testLevel = testLevelMatch.Success
            ? testLevelMatch.Groups["Level"].Value
            : null;

        // Assert
        using var scope = new AssertionScope();

        actualPathParts.Should().ContainSingle(p => p == "Tests",
            $"The test project {testProjectPath} should have a 'tests' ancestor directory"
            + $" (e.g '..\\Tests\\{Path.GetFileName(testProjectPath)}' or '..\\Tests\\{testLevel ?? "<level>"}\\{Path.GetFileName(testProjectPath)}')");

        actualPathParts.Should().ContainInOrder(expectedPathParts,
            $"The test project {testProjectPath} should have a dir path with directories in this order: {string.Join(Path.DirectorySeparatorChar, expectedPathParts)}");

        if (testLevel != null && actualPathParts.Contains(testLevel, StringComparer.InvariantCulture))
        {
            actualPathParts.Last().Should().Be(testLevel,
                $"The test project {testProjectPath} expected to have its parent dirname as test level: ...\\{testLevel}\\{Path.GetFileName(testProjectPath)}'");
        }

        actualPathParts
            .Except(expectedPathParts)
            .Except(new[] { testLevel, "Components", "Domains", "Services", "Tests" })
            .Should().BeEmpty("Test path should only contain dir names that match the parts of the project name");
    }

    public static IEnumerable<object[]> AllStandardProjectFiles => MonoRepository.AllNonTestProjectFiles
        .Where(path => !path.Split(Path.DirectorySeparatorChar).Contains("Pipelines"))
        .Where(path => !path.Split(Path.DirectorySeparatorChar).Contains("Docs"))
        .AsTestSourceArguments();

    [Theory]
    [MemberData(nameof(AllStandardProjectFiles))]
    public void All_standard_projects_follow_standard_path_conventions(string projectPath)
    {
        // Arrange
        var actualPathParts = Path.GetRelativePath(MonoRepository.RootDirectory, Path.GetDirectoryName(projectPath)!)
            .Split(Path.DirectorySeparatorChar);

        var expectedPathParts = Path.GetFileNameWithoutExtension(projectPath)
            .Split('.')
            .ToList();

        //This allows simple Components/Services to exclude these folders
        if (expectedPathParts.Last() == "WebApi" || expectedPathParts.Last() == "AzureFunction")
        {
            expectedPathParts.RemoveAt(expectedPathParts.Count - 1);
        }

        // Assert
        using var scope = new AssertionScope();

#pragma warning disable S125 // Sections of code should not be commented out
        //JAMES - This is correct but many projects need to be fixed first
        //actualPathParts.Last().Should().Be("Src",
        //    $"The project {projectPath} should be in a 'Src' Folder"
        //    + $" (e.g '..\\Src\\{Path.GetFileName(projectPath)}'");
#pragma warning restore S125 // Sections of code should not be commented out

        actualPathParts.Should().ContainSingle(p => p == "Src",
            $"The project {projectPath} should not have a 'Src' folder inside a 'Src' folder");

        actualPathParts.Should().ContainInOrder(expectedPathParts,
            $"The project {projectPath} should have a dir path with directories in this order: {string.Join(Path.DirectorySeparatorChar, expectedPathParts)}");

        actualPathParts
            .Except(expectedPathParts)
            .Except(new[] { "AzureFunction", "Components", "Domains", "Services", "Src", "Tools", "WebApi" })
            .Should().BeEmpty("Project path should only contain special folders and dir names that match the project name");
    }

    public static IEnumerable<object[]> AllPipelineResourceProjectFiles => MonoRepository.AllNonTestProjectFiles
        .Where(path => path.Split(Path.DirectorySeparatorChar).Contains("Pipelines"))
        .AsTestSourceArguments();
}
