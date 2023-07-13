using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using TechTonic.Monorepo.Validation.UnitTests.Helpers;
using Xunit;

namespace TechTonic.Monorepo.Validation.UnitTests;

public class BicepConventionTests
{
    public static IEnumerable<object[]> BicepScripts => MonoRepository.BicepScripts.AsTestSourceArguments();

    [Theory]
    [MemberData(nameof(BicepScripts))]
    public void All_bicep_scripts_should_follow_recommended_naming_standards(string path)
    {
        // Arrange
        var fileName = Path.GetFileName(path);

        // Assert
        fileName.Should().MatchRegex(NamingConventions.BicepFileRegexPattern, $"Bicep script {path} should follow camel case standards");
    }

}
