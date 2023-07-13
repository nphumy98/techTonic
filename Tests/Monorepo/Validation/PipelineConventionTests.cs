using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using TechTonic.Monorepo.Validation.UnitTests.Helpers;
using Xunit;

namespace TechTonic.Monorepo.Validation.UnitTests;

public class PipelineConventionTests
{
    public static IEnumerable<object[]> PipelineFiles => MonoRepository.AzureDevopsPipelineFiles.AsTestSourceArguments();

    [Theory]
    [MemberData(nameof(PipelineFiles))]
    public void All_pipeline_yml_files_should_be_hyphenated_lower_case(string pipelinePath)
    {
        // Arrange
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pipelinePath);

        // Assert
        fileNameWithoutExtension.Should().MatchRegex(NamingConventions.HyphenSeparatedLowercaseRegexPattern,
            $"Pipeline path {pipelinePath} should follow hyphenated lower case naming conventions");
    }

    [Fact]
    public void All_pipeline_yml_files_should_use_yml_extension_rather_than_yaml()
    {
        // Arrange
        var pipelineFilesWithIncorrectExtension = Directory
            .GetFiles(MonoRepository.RootDirectory, $"*.{FileExtensions.Yaml}", SearchOption.AllDirectories)
            .Where(path => path.Split(Path.DirectorySeparatorChar).Contains("pipelines", StringComparer.OrdinalIgnoreCase));

        // Assert
        pipelineFilesWithIncorrectExtension.Should().BeEmpty("Pipeline files should use 'yml' not 'yaml' to be consistent");
    }
}
