using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using FluentAssertions;
using TechTonic.Monorepo.Validation.UnitTests.Helpers;
using Xunit;

namespace TechTonic.Monorepo.Validation.UnitTests;

/// <remarks>
/// See 0007-monorepo-language-conventions.md in repo ADR docs for agreed rule set
/// </remarks>
public class ProjectLanguageConventionTests
{
    public static IEnumerable<object[]> AllTestProjectFiles => MonoRepository.AllTestProjectFiles.AsTestSourceArguments();
    public static IEnumerable<object[]> AllWebSdkProjectFiles => MonoRepository.AllWebSdkProjectFiles.AsTestSourceArguments();
    public static IEnumerable<object[]> AllHttpClientProjectFiles => MonoRepository.AllHttpClientProjectFiles.AsTestSourceArguments();
    public static IEnumerable<object[]> AllAzureFunctionProjectFiles => MonoRepository.AllAzureFunctionProjectFiles
        .AsTestSourceArguments();
    public static IEnumerable<object[]> AllAzureFunctionLibraryProjectFiles => MonoRepository.AllAzureFunctionLibraryProjectFiles.AsTestSourceArguments();
    public static IEnumerable<object[]> AllWebLibraryProjectFiles => MonoRepository.AllWebLibraryProjectFiles.AsTestSourceArguments();

    [Theory]
    [MemberData(nameof(AllTestProjectFiles))]
    public void All_test_projects_are_following_test_project_naming_standards(string testProjectPath)
    {
        // Arrange
        var projectFileNameWithoutExtension = Path.GetFileNameWithoutExtension(testProjectPath);
        var validTestProjectRegex = new Regex("^.+?\\.(?<Level>[^\\.]+)Tests$");
        var approvedTestLevelNames = new HashSet<string>(new[]
        {
            "Unit",
            "Integration",
            "Regression",
            "Acceptance",
            "Performance",
            "Smoke",
            "Stress",
            "Bdd"
        });

        // Act
        var match = validTestProjectRegex.Match(projectFileNameWithoutExtension);

        // Assert
        match.Success.Should().BeTrue( 
            $"{projectFileNameWithoutExtension} at {testProjectPath} should follow consistent naming standard for test projects: <project/deployable name>.<level/type>.Tests");

        approvedTestLevelNames.Should().Contain(match.Groups["Level"].Value,
            $"{projectFileNameWithoutExtension} at {testProjectPath} should follow consistent naming standard for the test level '{match.Groups["Level"].Value}', should not be a slight variant of other test levels but rather the same");
    }

    [Theory]
    [MemberData(nameof(AllWebSdkProjectFiles))]
    public void All_web_api_projects_are_following_web_api_project_naming_standards(string webProjectPath)
    {
        // Arrange
        var projectFileNameWithoutExtension = Path.GetFileNameWithoutExtension(webProjectPath);
        var validProjectRegex = new Regex("^.+\\.WebApi$");

        // Act & Assert
        if (projectFileNameWithoutExtension != "PlatformSettings.Portal") // Platform setting portal is not webapi project
        {
            projectFileNameWithoutExtension.Should().MatchRegex(validProjectRegex,
                $"{projectFileNameWithoutExtension} at {webProjectPath} should follow consistent naming standard for web api projects: having a '.WebApi' suffix");
        }
    }
}
