using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Microsoft.Build.Construction;

namespace TechTonic.Monorepo.Validation.UnitTests.Helpers;

public static class MonoRepository
{
    private static readonly string TemporaryComponents = $"{Path.DirectorySeparatorChar}Components{Path.DirectorySeparatorChar}Temp";

    private static string? _rootDirectory;
    public static string RootDirectory => FindRootDirectory();

    private static readonly Lazy<Repository> GitRepository = new(() => new Repository(RootDirectory));

    public static IReadOnlyCollection<string> AllGitTrackedDirectories => Directory.GetDirectories(RootDirectory, "*.*", SearchOption.AllDirectories)
        .Where(path => !path.StartsWith($"{RootDirectory}{TemporaryComponents}", StringComparison.OrdinalIgnoreCase))
        .Where(IsGitTrackedDirectory)
        .ToArray();

    public static IEnumerable<string> AllCsProjectFiles => Directory.GetFiles(RootDirectory, $"*.{FileExtensions.CsProject}", SearchOption.AllDirectories)
        .Where(projectFile => !projectFile.StartsWith($"{RootDirectory}{TemporaryComponents}", StringComparison.OrdinalIgnoreCase))
        .Where(IsGitTrackedFile);

    public static IEnumerable<string> AllSolutionFilterFiles => Directory.GetFiles(RootDirectory, $"*.{FileExtensions.SolutionFilter}", SearchOption.AllDirectories)
        .Where(IsGitTrackedFile)
        .Where(projectFile => !projectFile.StartsWith($"{RootDirectory}{TemporaryComponents}", StringComparison.OrdinalIgnoreCase));

    private static readonly Regex TestProjectRegex = new("Tests?\\." + FileExtensions.CsProject);

    public static IEnumerable<string> AllNonTestProjectFiles => AllCsProjectFiles
        .Where(projectFile => !TestProjectRegex.IsMatch(projectFile));

    public static IEnumerable<string> AllTestProjectFiles => AllCsProjectFiles
        .Where(projectFile => TestProjectRegex.IsMatch(projectFile));

    public static IEnumerable<string> AllWebSdkProjectFiles => AllCsProjectFiles
        .Where(projectFile => ProjectRootElement.Open(projectFile)?.Sdk.Equals("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase) ?? false);

    public static IEnumerable<string> AllNetSdkProjectFiles => AllCsProjectFiles
        .Where(projectFile => ProjectRootElement.Open(projectFile)?.Sdk.Equals("Microsoft.NET.Sdk", StringComparison.OrdinalIgnoreCase) ?? false);

    public static IEnumerable<string> AllHttpClientProjectFiles => AllNetSdkProjectFiles
        .Where(projectFile => new Regex("(?:Api|Client)\\." + FileExtensions.CsProject).IsMatch(projectFile));
    
    public static IEnumerable<string> AllWebLibraryProjectFiles => AllNetSdkProjectFiles
        .Where(projectFile => ProjectRootElement.Open(projectFile)?.GetPackageReferences().Any(reference => reference.StartsWith("Microsoft.AspNetCore.Http", StringComparison.OrdinalIgnoreCase)) ?? false)
        .Where(projectFile => !TestProjectRegex.IsMatch(projectFile));

    public static IEnumerable<string> AllAzureFunctionProjectFiles => AllNetSdkProjectFiles
        .Where(projectFile => ProjectRootElement.Open(projectFile)?.ContainsPropertyValueRegEx("AzureFunctionsVersion", ".+") ?? false);

    public static IEnumerable<string> AllAzureFunctionLibraryProjectFiles => AllNetSdkProjectFiles
        .Except(AllAzureFunctionProjectFiles)
        .Where(projectFile =>
        {
            var project = ProjectRootElement.Open(projectFile);
            if(project == null)
                return false;

            return project.GetPackageReferences().Any(reference => reference.StartsWith("Microsoft.Azure.Functions", StringComparison.OrdinalIgnoreCase));
        });

    public static IEnumerable<string> AllMarkdownFiles => Directory
        .GetFiles(RootDirectory, $"*.{FileExtensions.MarkDown}", SearchOption.AllDirectories)
        .Where(t => !t.Contains(SpecialDirectories.AzureDevopsConfig));

    public static IEnumerable<string> AzureDevopsPipelineFiles => Directory
        .GetFiles(RootDirectory, $"*.{FileExtensions.Yml}", SearchOption.AllDirectories)
        // assumes checkout isn't within an ancestor dir named "pipeline"
        .Where(path => path.Split(Path.DirectorySeparatorChar).Contains("pipelines", StringComparer.OrdinalIgnoreCase))
        .Where(f => !Path.GetFileName(f).Equals(SpecialFiles.GitVersion, StringComparison.OrdinalIgnoreCase));

    public static IEnumerable<string> PowershellScripts => Directory
        .GetFiles(RootDirectory, $"*.{FileExtensions.PowershellScript}", SearchOption.AllDirectories)
        .Where(IsGitTrackedFile);

    public static IEnumerable<string> BicepScripts => Directory
    .GetFiles(RootDirectory, $"*.{FileExtensions.Bicep}", SearchOption.AllDirectories);

    private static SolutionFile? _mainRepoSolution;

    public static SolutionFile MainRepoSolution
    {
        get
        {
            if (_mainRepoSolution != null)
                return _mainRepoSolution;

            _mainRepoSolution = SolutionFile.Parse(Path.Join(RootDirectory, "Plexure.sln"));
            return _mainRepoSolution;
        }
    }

    private static string FindRootDirectory()
    {
        if (_rootDirectory != null)
            return _rootDirectory;

        _rootDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        
        while(!IsRootGitDirectory(_rootDirectory))
        {
            _rootDirectory = Directory.GetParent(_rootDirectory)?.FullName
                ?? throw new DirectoryNotFoundException($"Failed to locate root git directory. Searched from {Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!}"); 
        }

        return _rootDirectory;
    }

    private static bool IsRootGitDirectory(string path)
    {
        return Directory.Exists(Path.Join(path, SpecialDirectories.Git));
    }

    public static bool IsGitTrackedDirectory(string directoryPath)
    {
        if (directoryPath.StartsWith(Path.Join(RootDirectory, SpecialDirectories.Git), StringComparison.OrdinalIgnoreCase))
            return false;

        return Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories).Any(IsGitTrackedFile);
    }

    public static bool IsGitTrackedFile(string filePath)
    {
        var gitIndexPath = Path.GetRelativePath(RootDirectory, filePath)
            .Replace("\\", "/");

        return GitRepository.Value.Index[gitIndexPath] != null;
    }
}
