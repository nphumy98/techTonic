using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Construction;

namespace TechTonic.Monorepo.Validation.UnitTests.Helpers;

internal static class DotNetProjectExtensions
{
    public static bool ContainsPackageReference(this ProjectRootElement project, string packageName)
    {
        return project.GetPackageReferences().Contains(packageName, StringComparer.OrdinalIgnoreCase);
    }

    public static IEnumerable<string> GetPackageReferences(this ProjectRootElement project)
    {
        return project.Items
            .Where(item => item.ItemType == "PackageReference")
            .Select(item => item.Include);
    }

    public static bool ContainsPropertyValueRegEx(this ProjectRootElement project, string propertyName, string regexPattern)
    {
        return project.Properties
            .Any(property => property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)
                             && Regex.IsMatch(property.Value, regexPattern));
    }
}
