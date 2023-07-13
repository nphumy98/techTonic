using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.Build.Construction;
using TechTonic.Monorepo.Validation.UnitTests.Helpers;
using Xunit;

namespace TechTonic.Monorepo.Validation.UnitTests;

public class CsProjectConventionTests
{
    public static IEnumerable<object[]> AllProjectFiles => MonoRepository.AllCsProjectFiles.AsTestSourceArguments();

    [Theory]
    [MemberData(nameof(AllProjectFiles))]
    public void All_project_files_should_be_dot_separated_with_title_case(string csProjectFile)
    {
        // Arrange
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(csProjectFile);

        // Assert
        fileNameWithoutExtension.Should().MatchRegex(NamingConventions.DotSeparatedPascalCaseRegexPattern,
            $"Cs Project File {fileNameWithoutExtension} in {csProjectFile} should follow dot separated title case naming conventions");
    }

    [Theory]
    [MemberData(nameof(AllProjectFiles))]
    public void All_project_files_should_exclude_plexure_prefix(string csProjectFile)
    {
        // Arrange
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(csProjectFile);

        // Assert
        fileNameWithoutExtension.Should().NotStartWith("Plexure",
            $"Cs Project File {fileNameWithoutExtension} in {csProjectFile} should not start with the redundant 'Plexure' prefix in the monorepo to keep paths small");
    }

    [Theory]
    [MemberData(nameof(AllProjectFiles))]
    public void All_project_files_should_be_sdk_format(string csProjectFile)
    {
        // Act
        Action act = () => ProjectRootElement.Open(csProjectFile);

        // Assert
        act.Should().NotThrow($"The project {csProjectFile} should be a valid project file in SDK format");
    }

    [Theory]
    [MemberData(nameof(AllProjectFiles))]
    public void All_project_namespaces_match_filename_with_plexure_prefix(string csProjectFile)
    {
        // Arrange
        var project = ProjectRootElement.Open(csProjectFile)!;
        project.Should().NotBeNull();

        // Act
        var rootNamespaceProperty = project.Properties.SingleOrDefault(property => property.Name == "RootNamespace")?.Value;
        if (rootNamespaceProperty == null)
            return; // Default namespaces already guaranteed to have the plexure prefix via common Directory.Build.props

        // Assert
        var expected = "Plexure." + Path.GetFileNameWithoutExtension(csProjectFile);

        rootNamespaceProperty.Should()
            .Be(expected,
            $"The project {csProjectFile} should follow namespace conventions prefixed with Plexure. in the monorepo");
    }

    [Theory]
    [MemberData(nameof(AllProjectFiles))]
    public void All_project_assemblies_match_filename_with_plexure_prefix(string csProjectFile)
    {
        // Arrange
        var project = ProjectRootElement.Open(csProjectFile)!;
        project.Should().NotBeNull();

        // Act
        var assemblyProperty = project.Properties.SingleOrDefault(property => property.Name == "AssemblyName")?.Value;
        if (assemblyProperty == null)
            return; // Assembly already guaranteed to have the plexure prefix via common Directory.Build.props

        // Assert
        var expected = "Plexure." + Path.GetFileNameWithoutExtension(csProjectFile);

        assemblyProperty.Should().Be(expected,
            $"The project {csProjectFile} should follow assembly name conventions prefixed with Plexure. in the monorepo");
    }

    [Theory]
    [MemberData(nameof(AllProjectFiles))]
    public void All_projects_referenced_by_main_solution_file(string csProjectFile)
    {
        // Arrange
        var maybeFound = MonoRepository.MainRepoSolution.ProjectsByGuid.Values.FirstOrDefault(project => project.AbsolutePath.Equals(csProjectFile, StringComparison.OrdinalIgnoreCase));

        // Assert
        maybeFound.Should().NotBeNull(
            $"The project {csProjectFile} should be referenced in the main Plexure.sln file");
    }

    [Theory]
    [MemberData(nameof(AllProjectFiles))]
    public void No_projects_have_implicit_usings(string csProjectFile)
    {
        // Arrange
        var project = ProjectRootElement.Open(csProjectFile)!;
        project.Should().NotBeNull();

        // Act
        var implicitUsingsProperty = project.Properties.SingleOrDefault(property => property.Name == "ImplicitUsings")?.Value;
        if (implicitUsingsProperty == null) return;

        if (csProjectFile.Contains($"Components{Path.DirectorySeparatorChar}AzureDevops", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Components{Path.DirectorySeparatorChar}BackgroundJobs", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Components{Path.DirectorySeparatorChar}BlobStorage", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Components{Path.DirectorySeparatorChar}Cosmos", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Components{Path.DirectorySeparatorChar}DistributedLock", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Components{Path.DirectorySeparatorChar}Identifiers", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Components{Path.DirectorySeparatorChar}Logging", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Components{Path.DirectorySeparatorChar}Performance", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Components{Path.DirectorySeparatorChar}PlatformSettings", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Components{Path.DirectorySeparatorChar}QuartzScheduler", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Components{Path.DirectorySeparatorChar}TableStorage", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Components{Path.DirectorySeparatorChar}QueueStorage", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Domains{Path.DirectorySeparatorChar}Core", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Domains{Path.DirectorySeparatorChar}Loyalty", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Domains{Path.DirectorySeparatorChar}Messaging", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Domains{Path.DirectorySeparatorChar}Organisation", StringComparison.OrdinalIgnoreCase) ||
            csProjectFile.Contains($"Domains{Path.DirectorySeparatorChar}Targeting", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Assert
        implicitUsingsProperty.Should().NotBe("enable",
            $"The project {csProjectFile} should follow conventions and not use Implicit Usings");
    }
}
