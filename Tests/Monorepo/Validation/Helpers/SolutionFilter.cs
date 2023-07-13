using System;

namespace TechTonic.Monorepo.Validation.UnitTests.Helpers
{
    internal class SolutionFilterFile
    {
        public SolutionFilter? Solution { get; set; }
    }

    internal class SolutionFilter
    {
        public string? Path { get; set; }
        public string[] Projects { get; set; } = Array.Empty<string>();
    }
}
