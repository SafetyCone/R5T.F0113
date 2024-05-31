using System;


namespace R5T.F0113
{
    public static class Instances
    {
        public static F0000.IEnumerableOperator EnumerableOperator => F0000.EnumerableOperator.Instance;
        public static L0066.IFileExtensions FileExtensions => L0066.FileExtensions.Instance;
        public static F0000.IFileSystemOperator FileSystemOperator => F0000.FileSystemOperator.Instance;
        public static F0020.IFrameworkNames FrameworkNames => F0020.FrameworkNames.Instance;
        public static F0002.IPathOperator PathOperator => F0002.PathOperator.Instance;
        public static F0020.IProjectFileOperator ProjectFileOperator => F0020.ProjectFileOperator.Instance;
        public static IProjectReferencesOperator ProjectReferencesOperator => F0113.ProjectReferencesOperator.Instance;
        public static F0020.IProjectSdkStringOperations ProjectSdkStringOperations => F0020.ProjectSdkStringOperations.Instance;
        public static F0020.IProjectXmlOperator ProjectXmlOperator => F0020.ProjectXmlOperator.Instance;
        public static F0000.IStringOperator StringOperator => F0000.StringOperator.Instance;
        public static F0114.IRuntimeDirectoryPathOperator RuntimeDirectoryPathOperator => F0114.RuntimeDirectoryPathOperator.Instance;
        public static IRuntimesOperator RuntimesOperator => F0113.RuntimesOperator.Instance;
    }
}