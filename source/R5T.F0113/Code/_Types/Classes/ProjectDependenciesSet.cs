using System;
using System.Collections.Generic;

using R5T.T0142;
using R5T.T0172;


namespace R5T.F0113
{
    [DataTypeMarker]
    public class ProjectDependenciesSet
    {
        /// <summary>
        /// Keep track of all recursive dependencies of a project, exclusive of the project itself (which is available as the key).
        /// </summary>
        public Dictionary<IProjectFilePath, IProjectFilePath[]> RecursiveProjectDependenciesByProjectFilePath_Exclusive { get; } = new Dictionary<IProjectFilePath, IProjectFilePath[]>();

        /// <summary>
        /// Keep track of which projects have an ASP.NET Core framework dependency.
        /// This is used for determining which dependency assemblies to add during reflection on assembly metadata.
        /// </summary>
        public Dictionary<IProjectFilePath, bool> HasAspNetDependencyByProjectFilePath { get; } = new Dictionary<IProjectFilePath, bool>();
    }
}
