using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Microsoft.DotNet.Cli.Build;
using Microsoft.DotNet.Cli.Build.Framework;
using Microsoft.DotNet.InternalAbstractions;

using static Microsoft.DotNet.Cli.Build.Framework.BuildHelpers;

namespace Microsoft.DotNet.Host.Build
{
    public class DebTargets
    {
        [Target(nameof(GenerateSharedHostDeb),
                nameof(GenerateSharedFrameworkDeb))]
        [BuildPlatforms(BuildPlatform.Ubuntu)]
        public static BuildTargetResult GenerateDebs(BuildTargetContext c)
        {
            return c.Success();
        }

        [Target]
        [BuildPlatforms(BuildPlatform.Ubuntu)]
        public static BuildTargetResult GenerateSharedHostDeb(BuildTargetContext c)
        {
            var packageName = Monikers.GetDebianSharedHostPackageName(c);
            var version = c.BuildContext.Get<HostVersion>("HostVersion").LockedHostVersion;
            var inputRoot = c.BuildContext.Get<string>("SharedHostPublishRoot");
            var debFile = c.BuildContext.Get<string>("SharedHostInstallerFile");
            var objRoot = Path.Combine(Dirs.Output, "obj", "debian", "sharedhost");
            var manPagesDir = Path.Combine(Dirs.RepoRoot, "Documentation", "manpages");
            var debianConfigFile = Path.Combine(Dirs.RepoRoot, 
                "packaging", "deb-package", "host", "dotnet-sharedhost-debian_config.json");

            var debianConfigVariables = new Dictionary<string, string>()
            {
                { "SHARED_HOST_BRAND_NAME", Monikers.SharedHostBrandName }
            };

            if (Directory.Exists(objRoot))
            {
                Directory.Delete(objRoot, true);
            }

            Directory.CreateDirectory(objRoot);

            var debCreator = new DebPackageCreator(
                DotNetCli.Stage0,
                objRoot,
                dotnetDebToolPackageSource: Dirs.Packages);

            debCreator.CreateDeb(
                debianConfigFile, 
                packageName, 
                version, 
                inputRoot, 
                debianConfigVariables, 
                debFile, 
                manPagesDir);

            return c.Success();
        }

        [Target(nameof(InstallSharedHost))]
        [BuildPlatforms(BuildPlatform.Ubuntu)]
        public static BuildTargetResult GenerateSharedFrameworkDeb(BuildTargetContext c)
        {
            var packageName = Monikers.GetDebianSharedFrameworkPackageName(c);
            var sharedHostVersion = c.BuildContext.Get<HostVersion>("HostVersion").LockedHostVersion;
            var version = c.BuildContext.Get<string>("SharedFrameworkNugetVersion");
            var inputRoot = c.BuildContext.Get<string>("SharedFrameworkPublishRoot");
            var debFile = c.BuildContext.Get<string>("SharedFrameworkInstallerFile");
            var objRoot = Path.Combine(Dirs.Output, "obj", "debian", "sharedframework");
            var debianConfigFile = Path.Combine(Dirs.RepoRoot,
                "packaging", "deb-package", "sharedframework", "dotnet-sharedframework-debian_config.json");

            var debianConfigVariables = new Dictionary<string, string>()
            {
                { "SHARED_HOST_DEBIAN_VERSION", sharedHostVersion },
                { "SHARED_FRAMEWORK_DEBIAN_PACKAGE_NAME", packageName },
                { "SHARED_FRAMEWORK_NUGET_NAME", Monikers.SharedFrameworkName },
                { "SHARED_FRAMEWORK_NUGET_VERSION",  c.BuildContext.Get<string>("SharedFrameworkNugetVersion")},
                { "SHARED_FRAMEWORK_BRAND_NAME", Monikers.SharedFxBrandName }
            };

            if (Directory.Exists(objRoot))
            {
                Directory.Delete(objRoot, true);
            }

            Directory.CreateDirectory(objRoot);

            var debCreator = new DebPackageCreator(
                DotNetCli.Stage0,
                objRoot,
                dotnetDebToolPackageSource: Dirs.Packages);

            debCreator.CreateDeb(
                debianConfigFile,
                packageName,
                version,
                inputRoot,
                debianConfigVariables,
                debFile);

            return c.Success();
        }

        [Target(nameof(InstallSharedFramework),
                nameof(RemovePackages))]
        [BuildPlatforms(BuildPlatform.Ubuntu)]
        public static BuildTargetResult TestDebInstaller(BuildTargetContext c)
        {
            return c.Success();
        }
        
        [Target]
        public static BuildTargetResult InstallSharedHost(BuildTargetContext c)
        {
            InstallPackage(c.BuildContext.Get<string>("SharedHostInstallerFile"));
            
            return c.Success();
        }
        
        [Target(nameof(InstallSharedHost))]
        public static BuildTargetResult InstallSharedFramework(BuildTargetContext c)
        {
            InstallPackage(c.BuildContext.Get<string>("SharedFrameworkInstallerFile"));
            
            return c.Success();
        }
        
        [Target]
        [BuildPlatforms(BuildPlatform.Ubuntu)]
        public static BuildTargetResult RemovePackages(BuildTargetContext c)
        {
            IEnumerable<string> orderedPackageNames = new List<string>()
            {
                Monikers.GetDebianSharedFrameworkPackageName(c),
                Monikers.GetDebianSharedHostPackageName(c)
            };
            
            foreach(var packageName in orderedPackageNames)
            {
                RemovePackage(packageName);
            }
            
            return c.Success();
        }
        
        private static void InstallPackage(string packagePath)
        {
            Cmd("sudo", "dpkg", "-i", packagePath)
                .Execute()
                .EnsureSuccessful();
        }
        
        private static void RemovePackage(string packageName)
        {
            Cmd("sudo", "dpkg", "-r", packageName)
                .Execute()
                .EnsureSuccessful();
        }
    }
}
