using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using R5T.F0113;
using R5T.F0114;
using R5T.F0115;
using R5T.T0132;
using R5T.T0172;
using R5T.T0172.Extensions;
using R5T.T0176;
using R5T.T0176.Extensions;
using R5T.T0180;
using R5T.T0180.Extensions;


namespace R5T.F0113
{
    [FunctionalityMarker]
    public partial interface IReflectionOperator : IFunctionalityMarker
    {
        /// <summary>
        /// Chooses <see cref="DistinctFilter_ByFileName(IEnumerable{IAssemblyFilePath})"/> as the default.
        /// </summary>
        public IDistinctEnumerable<IAssemblyFilePath> DistinctFilter(
            IEnumerable<IAssemblyFilePath> assemblyFilePaths)
        {
            return this.DistinctFilter_ByFileName(assemblyFilePaths);
        }

        /// <summary>
        /// Filter assembly file paths by file name.
        /// <para>
        /// Because the same assembly might exist in two different file locations (an assembly is just a file, and can be copied anywhere!),
        /// distinct file paths are not good enough.
        /// This looks at the file name to determine if an assembly is unique.
        /// </para>
        /// <para>
        /// Really, the contents of the file should be examined, specifically to determine the actual version of the assembly.
        /// But that looks to be very hard?
        /// And includes an actual trip to the file-system (which is slow).
        /// So instead assume the file name is good enough and see how far that gets us.
        /// </para>
        /// </summary>
        public IDistinctEnumerable<IAssemblyFilePath> DistinctFilter_ByFileName(
            IEnumerable<IAssemblyFilePath> assemblyFilePaths)
        {
            var duplicates = assemblyFilePaths
                .GroupBy(filePath => Instances.PathOperator.GetFileName(filePath.Value))
                .Where(group => group.Count() > 1)
                .Select(group => group.First())
                .Now();

            var nonDuplicateAssemblyFilePaths = assemblyFilePaths
                .Except(duplicates)
                .AsDistinctEnumerable()
                ;

            return nonDuplicateAssemblyFilePaths;
        }

        public AssembliesSet Get_AssembliesSet(
            IDistinctEnumerable<IAssemblyFilePath> assemblyFilePaths)
        {
            var output = new AssembliesSet(assemblyFilePaths);
            return output;
        }

        public AssembliesSet Get_AssembliesSet(
            IEnumerable<IAssemblyFilePath> assemblyFilePaths,
            DistinctFilter<IAssemblyFilePath> distinctFilter)
        {
            var distinctAssemblyFilePaths = distinctFilter(assemblyFilePaths);

            var output = this.Get_AssembliesSet(distinctAssemblyFilePaths);
            return output;
        }

        public AssembliesSet Get_AssembliesSet(
            IEnumerable<AssemblyFilePath> assemblyFilePaths)
        {
            var output = this.Get_AssembliesSet(
                assemblyFilePaths,
                this.DistinctFilter);

            return output;
        }

        public Assembly Get_Assembly(
            IAssemblyFilePath assemblyFilePath,
            MetadataLoadContext metadataLoadContext)
        {
            var assembly = metadataLoadContext.LoadFromAssemblyPath(assemblyFilePath.Value);
            return assembly;
        }

        public IAssemblyFilePath[] Get_AssemblyFilePaths(
            IDirectoryPath directoryPath)
        {
            var output = Instances.FileSystemOperator.FindChildFilesInDirectoryByFileExtension(
                directoryPath.Value,
                Instances.FileExtensions.Dll)
                .Select(x => x.ToAssemblyFilePath())
                .Now();

            return output;
        }

        public IAssemblyFilePath[] Get_AssemblyFilePaths(IEnumerable<IDirectoryPath> directoryPaths)
        {
            var distinctDirectoryPaths = directoryPaths
                .Distinct()
                ;

            var output = distinctDirectoryPaths
                .SelectMany(x => this.Get_AssemblyFilePaths(x))
                .Now();

            return output;
        }

        public IAssemblyFilePath[] Get_AssemblyFilePaths(
            IAssemblyFilePath assemblyFilePath,
            IEnumerable<IDirectoryPath> runtimeDirectoryPaths)
        {
            var assemblyFileDirectoryPath = Instances.PathOperator.GetFileParentDirectoryPath(assemblyFilePath.Value)
                .ToDirectoryPath();

            var output = this.Get_AssemblyFilePaths(
                runtimeDirectoryPaths
                    .Append(assemblyFileDirectoryPath));

            return output;
        }

        public MetadataLoadContext Get_MetadataLoadContext(
            AssembliesSet assembliesSet)
        {
            var resolver = this.Get_PathAssemblyResolver(assembliesSet);

            var metadataLoadContext = new MetadataLoadContext(resolver);
            return metadataLoadContext;
        }

        public MetadataLoadContext Get_MetadataLoadContext(
            IAssemblyFilePath assemblyFilePath,
            IEnumerable<IAssemblyFilePath> assemblyFilePaths)
        {
            var distinctAssemblyFilePaths = this.DistinctFilter(assemblyFilePaths);

            var assembliesSet = this.Get_AssembliesSet(distinctAssemblyFilePaths);

            var metadataLoadContext = this.Get_MetadataLoadContext(assembliesSet);
            return metadataLoadContext;
        }

        public MetadataLoadContext Get_MetadataLoadContext(
            IAssemblyFilePath assemblyFilePath,
            IEnumerable<IDirectoryPath> runtimeDirectoryPaths)
        {
            var assemblyFilePaths = this.Get_AssemblyFilePaths(
                assemblyFilePath,
                runtimeDirectoryPaths);

            return this.Get_MetadataLoadContext(
                assemblyFilePath,
                assemblyFilePaths);
        }

        public MetadataLoadContext Get_MetadataLoadContext(
            IAssemblyFilePath assemblyFilePath,
            RuntimeDependencyInclusions inclusions)
        {
            var runtimeDirectoryPaths = Instances.RuntimesOperator.Get_RuntimeDirectories(inclusions);

            return this.Get_MetadataLoadContext(
                assemblyFilePath,
                runtimeDirectoryPaths);
        }

        public async Task<MetadataLoadContext> Get_MetadataLoadContext(
            IAssemblyFilePath assemblyFilePath,
            IProjectFilePath projectFilePath,
            ProjectDependenciesSet projectDependenciesSet)
        {
            var inclusions = await Instances.RuntimesOperator.Get_RuntimeDependencyInclusions(
                projectFilePath,
                projectDependenciesSet);

            return this.Get_MetadataLoadContext(
                assemblyFilePath,
                inclusions);
        }

        public Task<MetadataLoadContext> Get_MetadataLoadContext(
            ProjectFileTuple projectFileTuple,
            ProjectDependenciesSet projectDependenciesSet)
        {
            return this.Get_MetadataLoadContext(
                projectFileTuple.AssemblyFilePath,
                projectFileTuple.ProjectFilePath,
                projectDependenciesSet);
        }

        public PathAssemblyResolver Get_PathAssemblyResolver(AssembliesSet assembliesSet)
        {
            var assemblyFilePaths = assembliesSet.AssemblyFilePaths
                .Select(x => x.Value)
                ;

            var resolver = new PathAssemblyResolver(assemblyFilePaths);
            return resolver;
        }

        public void InAssemblyContext_Synchronous(
            IAssemblyFilePath assemblyFilePath,
            AssembliesSet dependencyAssembliesSet,
            Action<Assembly> action)
        {
            using var metadataLoadContext = this.Get_MetadataLoadContext(
                assemblyFilePath,
                dependencyAssembliesSet.AssemblyFilePaths);

            var assembly = metadataLoadContext.LoadFromAssemblyPath(assemblyFilePath.Value);

            action(assembly);
        }

        public void InAssemblyContext_Synchronous(
            IAssemblyFilePath assemblyFilePath,
            IEnumerable<AssemblyFilePath> assemblyFilePaths,
            Action<Assembly> action)
        {
            var metadataLoadContext = this.Get_MetadataLoadContext(
                assemblyFilePath,
                assemblyFilePaths);

            var assembly = metadataLoadContext.LoadFromAssemblyPath(assemblyFilePath.Value);

            action(assembly);
        }

        public void InAssemblyContext_Synchronous(
            IAssemblyFilePath assemblyFilePath,
            IEnumerable<DirectoryPath> runtimeDirectoryPaths,
            Action<Assembly> action)
        {
            var metadataLoadContext = this.Get_MetadataLoadContext(
                assemblyFilePath,
                runtimeDirectoryPaths);

            var assembly = metadataLoadContext.LoadFromAssemblyPath(assemblyFilePath.Value);

            action(assembly);
        }

        public void InAssemblyContext_Synchronous(
            IAssemblyFilePath assemblyFilePath,
            RuntimeDependencyInclusions runtimeDependencyInclusions,
            Action<Assembly> action)
        {
            var metadataLoadContext = this.Get_MetadataLoadContext(
                assemblyFilePath,
                runtimeDependencyInclusions);

            var assembly = metadataLoadContext.LoadFromAssemblyPath(assemblyFilePath.Value);

            action(assembly);
        }

        public async Task InAssemblyContext_Synchronous(
            IAssemblyFilePath assemblyFilePath,
            ProjectFilePath projectFilePath,
            ProjectDependenciesSet projectDependenciesSet,
            Action<Assembly> action)
        {
            var metadataLoadContext = await this.Get_MetadataLoadContext(
                assemblyFilePath,
                projectFilePath,
                projectDependenciesSet);

            var assembly = metadataLoadContext.LoadFromAssemblyPath(assemblyFilePath.Value);

            action(assembly);
        }

        public async Task InAssemblyContext_Synchronous(
            ProjectFileTuple projectFileTuple,
            ProjectDependenciesSet projectDependenciesSet,
            Action<Assembly> action)
        {
            var metadataLoadContext = await this.Get_MetadataLoadContext(
                projectFileTuple,
                projectDependenciesSet);

            var assembly = metadataLoadContext.LoadFromAssemblyPath(projectFileTuple.AssemblyFilePath.Value);

            action(assembly);
        }
    }
}
