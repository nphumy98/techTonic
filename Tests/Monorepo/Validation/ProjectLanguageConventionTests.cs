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

    [Theory]
    [MemberData(nameof(AllHttpClientProjectFiles))]
    public void All_http_client_projects_are_following_http_client_project_naming_standards(string clientProjectPath)
    {
        // Arrange
        var projectFileNameWithoutExtension = Path.GetFileNameWithoutExtension(clientProjectPath);
        var validProjectRegex = new Regex("^.+\\.HttpClient$");

        // Act & Assert
        projectFileNameWithoutExtension.Should().MatchRegex(validProjectRegex,
            $"{projectFileNameWithoutExtension} at {clientProjectPath} should follow consistent naming standard for http client projects: having a '.HttpClient' suffix");
    }

    [Theory]
    [MemberData(nameof(AllAzureFunctionProjectFiles))]
    public void All_azure_function_projects_are_following_azure_function_project_naming_standards(string azureFunctionProjectPath)
    {
        // Arrange
        var projectFileNameWithoutExtension = Path.GetFileNameWithoutExtension(azureFunctionProjectPath);
        var validProjectRegex = new Regex("^.+\\.AzureFunctions?$");

        // Act & Assert
        projectFileNameWithoutExtension.Should().MatchRegex(validProjectRegex,
            $"{projectFileNameWithoutExtension} at {azureFunctionProjectPath} should follow consistent naming standard for http client projects: having a '.AzureFunction' suffix");
    }

    [Theory]
    [MemberData(nameof(AllAzureFunctionLibraryProjectFiles))]
    public void All_azure_function_library_projects_are_following_azure_function_library_project_naming_standards(string projectPath)
    {
        // Arrange
        var projectFileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectPath);
        var validProjectRegex = new Regex("^.+\\.AzureFunctions$");

        // Act & Assert
        projectFileNameWithoutExtension.Should().MatchRegex(validProjectRegex,
            $"{projectFileNameWithoutExtension} at {projectPath} should follow consistent naming standard for azure function library projects: having a '.AzureFunctions' suffix");
    }

    [Theory]
    [MemberData(nameof(AllWebLibraryProjectFiles))]
    public void All_web_library_projects_are_following_web_library_project_naming_standards(string projectPath)
    {
        // Arrange
        var projectFileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectPath);
        var validProjectRegex = new Regex("^.+\\.Web");

        // Act & Assert
        projectFileNameWithoutExtension.Should().MatchRegex(validProjectRegex,
            $"{projectFileNameWithoutExtension} at {projectPath} should follow consistent naming standard for web library projects: having a '.Web' suffix");
    }
}
