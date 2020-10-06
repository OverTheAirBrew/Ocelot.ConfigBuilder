#module nuget:?package=Cake.DotNetTool.Module&version=0.4.0
#addin "nuget:?package=Cake.Git&version=0.21.0"
#addin nuget:?package=Newtonsoft.Json&version=12.0.3
#addin nuget:?package=System.Net.Http&version=4.3.4
#tool "nuget:?package=ReportGenerator&version=4.5.1"

using System;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var buildNumber = Argument("buildNumber", "0");

string committedVersion = "0.0.0-dev";
string version = "";
int releaseId = 0;

var artifactsDirectory = Directory("artifacts");
var packageDirectory = artifactsDirectory + Directory("Packages");
var releaseNotesFile = packageDirectory + File("releasenotes.md");
var artifactsFile = packageDirectory + File("artifacts.txt");
var artifactsForUnitTestsDir = artifactsDirectory + Directory("UnitTests");

var srcDirectory = "./src/Ocelot.ConfigBuilder.csproj";


Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .IsDependentOn("Build")
    .IsDependentOn("Package")
    .IsDependentOn("Push");

Task("Clean")
    .Does(() => {
        CleanDirectory(packageDirectory);
        CleanDirectory(artifactsDirectory);
    });

Task("Version")
    .Does(() => { 
        if (IsRunningOnCircleCI())
        {
          Information("Persisting version number...");
          PersistVersion(committedVersion, buildNumber);
        }
        else
        {
          Information("We are not running on build server, so we won't persist the version number.");
        }
    });

Task("Build")
    .Does(() => {
        var buildSettings = new DotNetCoreBuildSettings{
            Configuration = configuration,
            OutputDirectory = $"{artifactsDirectory}/Api",
            Framework = "netcoreapp3.1",
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .SetVersion(buildNumber)
                .WithProperty("FileVersion", buildNumber)
                .WithProperty("InformationalVersion", buildNumber)
                .WithProperty("nowarn", "7035")
        };

        DotNetCoreBuild(srcDirectory, buildSettings);
    });

Task("Package")
    .Does(() => {
         var settings = new DotNetCorePackSettings
        {
            Configuration = "Release",
            OutputDirectory = $"{artifactsDirectory}"
        };

        DotNetCorePack(srcDirectory, settings);
    });

Task("Push")
    .WithCriteria(IsRunningOnCircleCI())
    .Does(() => {
        var settings = new DotNetCoreNuGetPushSettings
        {
            Source = "github",
        };

        DotNetCoreNuGetPush($"{artifactsDirectory}/Ocelot.ConfigBuilder*.nupkg", settings);
    });

RunTarget(target);


private bool IsRunningOnCircleCI()
{
    return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CIRCLECI"));
}

private bool IsMaster()
{
    return Environment.GetEnvironmentVariable("CIRCLE_BRANCH")?.ToLower() == "master";
}

private void PersistVersion(string committedVersion, string newVersion)
{
	Information(string.Format("We'll search all csproj files for {0} and replace with {1}...", committedVersion, newVersion));

	var projectFiles = GetFiles("./**/*.csproj");

	foreach(var projectFile in projectFiles)
	{
		var file = projectFile.ToString();
 
		Information(string.Format("Updating {0}...", file));

		var updatedProjectFile = System.IO.File.ReadAllText(file)
			.Replace(committedVersion, newVersion);

		System.IO.File.WriteAllText(file, updatedProjectFile);
	}
}
