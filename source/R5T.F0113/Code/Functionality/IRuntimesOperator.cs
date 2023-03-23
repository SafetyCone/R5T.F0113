using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using R5T.F0114;
using R5T.T0132;
using R5T.T0172;
using R5T.T0172.Extensions;
using R5T.T0174;
using R5T.T0175;
using R5T.T0175.Extensions;
using R5T.T0177;


namespace R5T.F0113
{
    [FunctionalityMarker]
    public partial interface IRuntimesOperator : IFunctionalityMarker
    {
        public void Determine_RuntimeDependencyInclusions(
            IProjectFilePath projectFilePath,
            XElement projectElement,
            ProjectDependenciesSet projectDependenciesSet,
            RuntimeDependencyInclusions runtimeDependencyInclusions)
        {
            runtimeDependencyInclusions.NetCore = this.ShouldIncludeCoreRuntimeDirectory(projectElement);

            runtimeDependencyInclusions.AspNetCore = this.ShouldIncludeAspNetRuntimeDirectory(
                projectFilePath,
                projectElement,
                projectDependenciesSet);

            runtimeDependencyInclusions.WindowsFormsCore = this.ShouldIncludeWindowsRuntimeDirectory(projectElement);
        }

        public RuntimeDependencyInclusions Determine_RuntimeDependencyInclusions(
            IProjectFilePath projectFilePath,
            ProjectDependenciesSet projectDependenciesSet)
        {
            var inclusions = new RuntimeDependencyInclusions();

            Instances.ProjectFileOperator.InReadonlyProjectFileContext_Synchronous(
                projectFilePath.Value,
                projectElement =>
                {
                    this.Determine_RuntimeDependencyInclusions(
                        projectFilePath,
                        projectElement,
                        projectDependenciesSet,
                        inclusions);
                });

            return inclusions;
        }

        public bool ShouldIncludeAspNetRuntimeDirectory(
            IProjectFilePath projectFilePath,
            XElement projectElement,
            ProjectDependenciesSet projectDependenciesSet)
        {
            var sdk = Instances.ProjectXmlOperator.GetSdk(projectElement);

            var isWebSdk = Instances.ProjectSdkStringOperations.Is_WebSdk(sdk);
            if (isWebSdk)
            {
                return true;
            }

            var hasAspNetDependency = projectDependenciesSet.HasAspNetDependencyByProjectFilePath[projectFilePath];
            if (hasAspNetDependency)
            {
                return true;
            }

            return false;
        }

        public bool ShouldIncludeCoreRuntimeDirectory(
            XElement projectElement)
        {
            // Is the project a web project?
            var sdk = Instances.ProjectXmlOperator.GetSdk(projectElement);

            var shouldInclude = sdk switch
            {
                // Publishing Blazor WebAssembly results in ALL required assemblies.
                F0020.IProjectSdkStrings.BlazorWebAssembly_Constant => false,
                // True for all others.
                _ => true,
            };

            return shouldInclude;
        }

        public bool ShouldIncludeWindowsRuntimeDirectory(
            XElement projectElement)
        {
            var targetFramework = Instances.ProjectXmlOperator.GetTargetFramework(projectElement);

            // Is the project a windows forms project?
            var isWindowsProject = Instances.StringOperator.Contains(
                targetFramework,
                "windows");

            return isWindowsProject;
        }

        public IEnumerable<DirectoryPath> Get_RuntimeDirectories(
            RuntimeDependencyInclusions inclusions,
            DotnetRuntimeDirectoryPaths dotnetRuntimeDirectoryPaths)
        {
            var output = Instances.EnumerableOperator.Empty<DirectoryPath>()
                .AppendIf(
                    inclusions.AspNetCore,
                    dotnetRuntimeDirectoryPaths.AspNetCoreApp.ToDirectoryPath())
                .AppendIf(
                    inclusions.NetCore,
                    dotnetRuntimeDirectoryPaths.NetCoreApp.ToDirectoryPath())
                .AppendIf(
                    inclusions.WindowsFormsCore,
                    dotnetRuntimeDirectoryPaths.WindowsDesktopApp.ToDirectoryPath())
                ;

            return output;
        }

        public IEnumerable<DirectoryPath> Get_RuntimeDirectories(RuntimeDependencyInclusions inclusions)
        {
            var dotnetRuntimeDirectoryPaths = Instances.RuntimeDirectoryPathOperator.GetDotnetRuntimeDirectoryPaths();

            var output = this.Get_RuntimeDirectories(
                inclusions,
                dotnetRuntimeDirectoryPaths);

            return output;
        }

        public async Task<RuntimeDependencyInclusions> Get_RuntimeDependencyInclusions(
            IProjectFilePath projectFilePath,
            ProjectDependenciesSet projectDependenciesSet)
        {
            // Use a wrapping dictionary for conversion to/from strong type.
            var wrappingDictionary = new TypeConversionDictionaryWrapper<string, string[], IProjectFilePath, IProjectFilePath[]>(
                projectDependenciesSet.RecursiveProjectDependenciesByProjectFilePath_Exclusive,
                new FunctionBasedConverter<IProjectFilePath, string>(
                    x => x.Value,
                    x => x.ToProjectFilePath()),
                new FunctionBasedConverter<IProjectFilePath[], string[]>(
                    x => x.Select(y => y.Value).Now(),
                    x => x.Select(y => y.ToProjectFilePath()).Now()));

            // First get the recursive dependencies for a project.
            await Instances.ProjectReferencesOperator.AddRecursiveProjectReferences_Exclusive_Idempotent(
                wrappingDictionary,
                projectFilePath.Value);

            // Then compute which projects in the recursive set do not have information about whether they have an ASP.NET dependency.
            Instances.ProjectReferencesOperator.ComputeAspNetCoreReferences(projectDependenciesSet);

            var inclusions = this.Determine_RuntimeDependencyInclusions(
                projectFilePath,
                projectDependenciesSet);

            return inclusions;
        }
    }
}
