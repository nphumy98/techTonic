using System.Globalization;
using System.Text;
using static System.Char;

namespace TechTonic.Monorepo.Validation.UnitTests.Helpers;

public static class NamingConventions
{
    public const string AdrFileRegexPattern = "^[0-9]{4}\\-[a-z][a-z0-9\\-]*(?<!\\-).md$"; // e.g. 0001-this_is_an_example_of_a_valid_text.md
    public const string BicepFileRegexPattern = "^[a-z]+((\\d)|([A-Z0-9][a-z0-9]+))*([A-Z])?.bicep"; // e.g. thisExample.bicep (Camel Case)
    public const string PowershellCommandletFileRegexPattern = "^[A-Z][a-z]+\\-[A-Z]([A-Za-z0-9])+$"; // e.g. This-IsAnExampleOfAValidText (VerbNounHyphenatedTitleCase)

    public const string PascalCaseRegexPattern = "^[A-Z0-9][A-Za-z]*([0-9]*[A-Z][A-Za-z]*)*[0-9]*$"; // e.g. ThisIsAnExampleOfAValidText, ThisIs1AnotherExample
    public const string DotSeparatedPascalCaseRegexPattern = "^[A-Z][A-Za-z0-9]*(?:\\.[A-Z][A-Za-z0-9]*)*$"; // e.g. This.Is.An.Example.Of.A.Valid.Text This.Is1.MoreExample (DotSeparatedTitleCase)
    public const string HyphenSeparatedLowercaseRegexPattern = "^[a-z0-9][a-z0-9\\-]*(?<!\\-)$"; // e.g. this_is_an_example_of_a_valid_text
    public const string HyphenSeparatedPascalCaseRegexPattern = "^[A-Z][A-Za-z0-9]*(?:\\-[A-Z][A-Za-z0-9]*)*$"; // e.g. This-IsAnExample-Of-A-Valid-Text


    public static string PascalToHyphenSeparatedLowercase(this string pascalCase)
    {
        var builder = new StringBuilder();
        foreach (var c in pascalCase)
        {
            if (IsUpper(c) && builder.Length > 0)
                builder.Append('-');
            builder.Append(ToLower(c, CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}
