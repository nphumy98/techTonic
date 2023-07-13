using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Build.Construction;
using TechTonic.Monorepo.Validation.UnitTests.Helpers;
using Xunit;

namespace TechTonic.Monorepo.Validation.UnitTests;

public class AzureFunctionTests 
{
    // Currently isolated SDK does not support durable functions 
    public static IEnumerable<object[]> AllNonDurableAzureFunctionProjectFiles => MonoRepository.AllAzureFunctionProjectFiles
        .Where(path => !(ProjectRootElement.Open(path)?.GetPackageReferences().Any(reference => reference.Contains("Microsoft.Azure.DurableTask")) ?? false))
        .AsTestSourceArguments();

    [Theory(Skip = "Waiting for loyalty ADO Ticket #62045 to convert existing migrated functions to isolated before enabling this new policy")]
    [MemberData(nameof(AllNonDurableAzureFunctionProjectFiles))]
    public void All_non_durable_functions_should_use_isolated_function_libraries(string azureFunctionProjectPath)
    {
        // Arrange
        var project = ProjectRootElement.Open(azureFunctionProjectPath);

        // Act
        var containsLegacySdk = project?.ContainsPackageReference("Microsoft.NET.Sdk.Functions");
            
        // Assert
        containsLegacySdk.Should()
            .BeFalse($"Non-durable function {azureFunctionProjectPath} should use the new isolated (out of process) framework (Microsoft.Azure.Functions.Worker) instead of the legacy function SDK (Microsoft.NET.Sdk.Functions)");
    }
}
