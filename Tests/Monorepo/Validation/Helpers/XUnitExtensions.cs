using System.Collections.Generic;
using System.Linq;

namespace TechTonic.Monorepo.Validation.UnitTests.Helpers;

public static class XUnitExtensions
{
    public static IEnumerable<object[]> AsTestSourceArguments<T>(this IEnumerable<T> items) => items.Select(item => new object[] { item! });
}