using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using FluentAssertions;
using TechTonic.Monorepo.Validation.UnitTests.Helpers;
using Xunit;

namespace TechTonic.Monorepo.Validation.UnitTests;

public class PowershellConventionTests
{
    public static IEnumerable<object[]> PowershellScripts => MonoRepository.PowershellScripts.AsTestSourceArguments();

    [Theory]
    [MemberData(nameof(PowershellScripts))]
    public void All_powershell_scripts_should_follow_recommended_naming_standards(string path)
    {
        // Arrange
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);

        // Assert
        if (IsCommandlet(path))
        {
            fileNameWithoutExtension.Should().MatchRegex(NamingConventions.PowershellCommandletFileRegexPattern,
                $"Powershell commandlet {path} should follow \"Verb-Noun\" standards. " +
                "Where \"Verb\" should be an approved powershell verb (see the \"Get-Verb\" command)");
        }
        else
        {
            fileNameWithoutExtension.Should().MatchRegex(NamingConventions.HyphenSeparatedPascalCaseRegexPattern,
                $"Powershell script {path} should follow hyphenated standards. " +
                "Where \"Verb\" should be an approved powershell verb (see the \"Get-Verb\" command)");
        }
    }

    [Theory]
    [MemberData(nameof(PowershellScripts))]
    public void All_powershell_scripts_should_set_strict_mode_to_at_least_version_one(string path)
    {
        // Act
        var doesSetStructMode = DoesFileContainText(path, "Set-StrictMode", StringComparison.OrdinalIgnoreCase);
        // Exploiting that you cannot call Set-StrictMode with zero. So it its present it is at least version 1.0

        // Assert
        doesSetStructMode.Should().BeTrue($"The powershell script {path} should set strict mode to at level level 1, ideally 3.");
    }

    private static bool DoesFileContainText(string filePath, string text, StringComparison comparison)
    {
        using var fileStream = File.OpenRead(filePath);
        using var reader = new StreamReader(fileStream);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.Contains(text, comparison))
                return true;
        }

        return false;
    }

    private static bool IsCommandlet(string powershellScriptPath)
    {
        var firstLine = ReadFirstLineExcludingComments(powershellScriptPath);
        return new Regex("^\\s*\\[\\s*Cmdletbinding", RegexOptions.IgnoreCase).IsMatch(firstLine ?? string.Empty);
    }

    private static string? ReadFirstLineExcludingComments(string powershellScriptPath)
    {
        using var stream = File.OpenRead(powershellScriptPath);
        using var reader = new StreamReader(stream);
        bool isOpenComment = false;
        string? line;
        var isSpaceOrCommentsRegEx = new Regex("^(?:\\s*|\\s*#.+)$");
        var isOpenCommentRegex = new Regex("^\\s*<#");
        var isClosedCommentRegex = new Regex("#>\\s*$");
        while ((line = reader.ReadLine()) != null)
        {
            if (isSpaceOrCommentsRegEx.IsMatch(line))
                continue;

            if (!isOpenComment)
                isOpenComment = isOpenCommentRegex.IsMatch(line);

            var isCommentLine = isOpenComment;

            if (isClosedCommentRegex.IsMatch(line))
                isOpenComment = false;

            if (isCommentLine)
                continue;

            return line;
        }

        return null;
    }
}
