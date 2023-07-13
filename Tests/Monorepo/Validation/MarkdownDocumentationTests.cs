using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Execution;
using TechTonic.SharedProject;
using TechTonic.Monorepo.Validation.UnitTests.Helpers;
using Xunit;

namespace TechTonic.Monorepo.Validation.UnitTests;

public class MarkdownDocumentationTests
{
    public static IEnumerable<object[]> AllMarkdownFiles => MonoRepository.AllMarkdownFiles.AsTestSourceArguments();

    [Theory]
    [MemberData(nameof(AllMarkdownFiles))]
    public void All_markdown_files_should_be_hyphenated_case_except_readmes(string markdownFilePath)
    {
        // Arrange
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(markdownFilePath);
        var isReadmeFile = SpecialFiles.ReadMe.Equals(Path.GetFileName(markdownFilePath), StringComparison.OrdinalIgnoreCase);

        // Assert
        if (isReadmeFile)
        {
            fileNameWithoutExtension.Should().Be("README", $"Readme files should follow standard casing convention of all caps - {markdownFilePath}");
            return;
        }
            
        fileNameWithoutExtension.Should().MatchRegex(NamingConventions.HyphenSeparatedLowercaseRegexPattern,
            $"Mark down file {fileNameWithoutExtension} in {markdownFilePath} should follow hyphenated lower case naming conventions");
    }

    [Theory]
    [MemberData(nameof(AllMarkdownFiles))]
    public void All_markdown_relative_links_are_not_broken(string markdownFilePath)
    {
        // Arrange
        var markdownContents = File.ReadAllLines(markdownFilePath);
        var relativePathLinkRegex = new Regex(@"\[[^][]+]\((?<RelativePath>[^:\)]+)\)"); // i.e. not anything with a protocol - so assume relative path

        using var assertionScope = new AssertionScope();

        bool isInFencedCodeBlock = false;

        foreach (var line in markdownContents)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var lineStartsWithCodeBlock = line.Trim().StartsWith("```", StringComparison.InvariantCultureIgnoreCase);

            if (lineStartsWithCodeBlock)
            {
                isInFencedCodeBlock = !isInFencedCodeBlock;
            }

            if(isInFencedCodeBlock) continue;

            foreach (Match match in relativePathLinkRegex.Matches(line))
            {
                var relativePath = match.Groups["RelativePath"].Value;

                var dir = Path.GetDirectoryName(markdownFilePath);

                if (string.IsNullOrEmpty(dir)) throw new ValidationException("Unable to get GetDirectoryName for {markdownFilePath}");

                var referencedFile = PathUtils.CombineRelative(dir, relativePath);

                (File.Exists(referencedFile) || Directory.Exists(referencedFile))
                    .Should().BeTrue($"The relative link {relativePath} in {markdownFilePath} should reference an existing file ({referencedFile}) but it does not exist");

                //If the casing is wrong then the above will fail on ubuntu but pass in windows (this is anoying)
                //The below will fail on windows if the casing is wrong
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    PathUtils.CheckPathCasing(referencedFile).Should().BeTrue($"The relative link {relativePath} in {markdownFilePath} should have the same casing as the actual file on disk");
                }
            }
        }
    }

}