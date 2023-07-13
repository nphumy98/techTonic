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

public class ArchitectureDesignRecordTests
{
    public static IEnumerable<object[]> AdrDirectories
    {
        get
        {
            foreach (var directory in MonoRepository.AllGitTrackedDirectories.OrderBy(d => d))
            {
                if(directory.EndsWith(MonoRepoDirectoryStructure.ArchitectureDesignRecordDirectoryPathSuffix, StringComparison.OrdinalIgnoreCase))
                    yield return new object[] { directory };
            }
        }
    }

    [Theory]
    [MemberData(nameof(AdrDirectories))]
    public void adr_directory_files_are_following_naming_conventions(string adrDirectoryPath)
    {
        // Arrange
        var allFiles = Directory.GetFiles(adrDirectoryPath);

        // AUTHOR AND PR REVIEWER: THIS SHOULD VERY RARELY CHANGE - PLEASE GET A LEAD TO REVIEW THIS CHANGE BEFORE APPROVING
        var approvedFilesOtherThanAdrs = new HashSet<string>(new[]
        {
            "index.md",
            "template.md",
            "update-index.bat"
        }, StringComparer.OrdinalIgnoreCase);

        // Assert
        using var assertionScope = new AssertionScope();
        foreach (var filePath in allFiles)
        {
            var fileName = Path.GetFileName(filePath);

            if(approvedFilesOtherThanAdrs.Contains(fileName))
                continue;

            fileName.Should().MatchRegex(NamingConventions.AdrFileRegexPattern,
                $"The ADR record {fileName} in {adrDirectoryPath} should be an ADR record following the standard ADR naming and casing convention");
        }
    }

    private static IEnumerable<string> GetAdrRecordFiles(string adrDirectory)
    {
        return Directory.GetFiles(adrDirectory)
            .Where(path => Regex.IsMatch(Path.GetFileName(path), NamingConventions.AdrFileRegexPattern));
    }

    [Theory]
    [MemberData(nameof(AdrDirectories))]
    public void There_are_no_adr_index_number_clashes(string adrDirectoryPath)
    {
        // Arrange
        var allAdrRecords = GetAdrRecordFiles(adrDirectoryPath);
            
        // Act
        var allIndexNumbers = allAdrRecords.Select(path => int.Parse(Path.GetFileName(path).Substring(0, 4)));

        // Assert
        allIndexNumbers.Should().OnlyHaveUniqueItems($"Adrs in the same directory {adrDirectoryPath} should not have clashing index numbers");
    }

    [Theory]
    [MemberData(nameof(AdrDirectories))]
    public void There_are_subdirs_in_adr_dirs(string adrDirectoryPath)
    {
        // Arrange
        var subDirs = Directory.GetDirectories(adrDirectoryPath);

        // Assert
        subDirs.Should().BeEmpty($"The ADR dir {adrDirectoryPath} should not have any sub directories");
    }

    public static IEnumerable<object[]> AdrRecordFiles
    {
        get
        {
            foreach (var adrDirectory in AdrDirectories.Select(args => (string)args[0]))
            {
                foreach (var path in GetAdrRecordFiles(adrDirectory))
                {
                    yield return new object[] { path };
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(AdrRecordFiles))]
    public void Adr_is_indexed(string adrPath)
    {
        // Arrange
        var indexFile = Path.Join(Path.GetDirectoryName(adrPath), "index.md");
        File.Exists(indexFile).Should().BeTrue($"Missing index file {indexFile}");

        var indexContents = File.ReadAllText(indexFile);

        // Assert
        indexContents.Should().Contain(Path.GetFileName(adrPath), $"Adr record {adrPath} should be indexed in {indexFile}");
    }
}
