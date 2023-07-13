using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Json;
using Microsoft.Build.Construction;
using Newtonsoft.Json;
using TechTonic.SharedProject;
using TechTonic.Monorepo.Validation.UnitTests.Helpers;
using Xunit;

namespace TechTonic.Monorepo.Validation.UnitTests;

public static class SolutionFilterConventionTests
{
    public static IEnumerable<object[]> AllSolutionFilterFiles => MonoRepository.AllSolutionFilterFiles.AsTestSourceArguments();

    [Theory]
    [MemberData(nameof(AllSolutionFilterFiles))]
    public static void Solution_filter_has_valid_paths(string solutionFilterPath)
    {
        // Arrange
        var filterContent = File.ReadAllText(solutionFilterPath);

        // Act
        var solutionFilter = JsonConvert.DeserializeObject<SolutionFilterFile>(filterContent);

        // Assert
        solutionFilter.Should().NotBeNull();
        solutionFilter!.Solution.Should().NotBeNull();
        solutionFilter.Solution!.Path.Should().NotBeNullOrWhiteSpace();

        using var scope = new AssertionScope();

        var solutionPath = Path.Join(Path.GetDirectoryName(solutionFilterPath)!, solutionFilter.Solution!.Path!).CleanDirectorySeparators();

        File.Exists(solutionPath).Should().BeTrue($"Referenced solution path '{solutionPath}' should not be broken in {solutionFilterPath}");

        foreach (var projectPath in solutionFilter.Solution!.Projects)
        {
            var projectFullPath = Path.Join(MonoRepository.RootDirectory, projectPath).CleanDirectorySeparators();
            File.Exists(projectFullPath).Should().BeTrue($"Project path '{projectFullPath}' reference should not be broken in {solutionFilterPath}");
        }
    }

    [Theory]
    [MemberData(nameof(AllSolutionFilterFiles))]
    public static void Solution_filter_is_valid_json(string solutionFilterPath)
    {
        // Arrange
        var filterContent = File.ReadAllText(solutionFilterPath);

        // Act & Assert
        filterContent.Should().BeValidJson($"Solution filter' {solutionFilterPath}' should have valid json");
    }

    [Theory]
    [MemberData(nameof(AllSolutionFilterFiles))]
    public static void Solution_filter_has_all_dependencies(string solutionFilterPath)
    {
        // Arrange
        var filterContent = File.ReadAllText(solutionFilterPath);
        var solutionFilter = JsonConvert.DeserializeObject<SolutionFilterFile>(filterContent);
        solutionFilter.Should().NotBeNull();
        var referencedProjects = solutionFilter!.Solution!.Projects
            .Select(path => Path.Join(MonoRepository.RootDirectory, path).CleanDirectorySeparators())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Act
        var missingDependencies = GetMissingDependentProjectPaths(referencedProjects)
            .Select(path => Path.GetRelativePath(MonoRepository.RootDirectory, path));

        // Assert
        missingDependencies.Should().BeEmpty($"Solution filter {solutionFilterPath} should contain all dependencies in order to compile properly");
    }

    private static IEnumerable<string> GetMissingDependentProjectPaths(IReadOnlySet<string> projectPaths)
    {
        var indirectlyImpactedProjectPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var projectsToWalkDependencies = new Queue<string>(projectPaths);

        while (projectsToWalkDependencies.Any())
        {
            var projectPath = projectsToWalkDependencies.Dequeue();
            foreach (var directDependencyPath in GetDirectProjectDependencies(projectPath))
            {
                if (indirectlyImpactedProjectPaths.Contains(directDependencyPath)
                    || projectPaths.Contains(directDependencyPath))
                    continue;

                indirectlyImpactedProjectPaths.Add(directDependencyPath);
                projectsToWalkDependencies.Enqueue(directDependencyPath);
            }
        }

        return indirectlyImpactedProjectPaths;
    }
    
    private static IEnumerable<string> GetDirectProjectDependencies(string projectPath)
    {
        var project = ProjectRootElement.Open(projectPath);
        if (project == null)
            throw new NotSupportedException($"Cannot read {projectPath} - is it invalid or in the old unsupported format?");

        return project.ItemGroups
            .SelectMany(itemGroup => itemGroup.Items)
            .Where(itemDefinition => itemDefinition.ElementName == "ProjectReference")
            .Select(itemDefinition => PathUtils.CombineReference(projectPath, itemDefinition.Include));
    }
}
