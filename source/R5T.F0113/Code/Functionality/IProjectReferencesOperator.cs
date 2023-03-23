using System;
using System.Collections.Generic;

using R5T.T0132;
using R5T.T0172;

namespace R5T.F0113
{
    [FunctionalityMarker]
    public partial interface IProjectReferencesOperator : IFunctionalityMarker,
        F0016.F001.IProjectReferencesOperator
    {
        public void ComputeAspNetCoreReferences(ProjectDependenciesSet projectDependenciesSet)
        {
            // Keep a list of projects for file data analysis.
            var projectsToEvaluate = new HashSet<IProjectFilePath>();

            foreach (var projectFilePath in projectDependenciesSet.RecursiveProjectDependenciesByProjectFilePath_Exclusive.Keys)
            {
                var alreadyEvaluated = projectDependenciesSet.HasAspNetDependencyByProjectFilePath.ContainsKey(projectFilePath);
                if (!alreadyEvaluated)
                {
                    var hasAspNetDependency = false;

                    // First compute which projects have ASP.NET core references just by evaluating dependency references.
                    // (Do this first since it is quicker than going to the file system, and if a project has both a reference to a project with an ASP.NET Core reference, and an ASP.NET Core reference, it's quicker to mark it based on dependency reference data in memory than by checking the file system.)
                    var recursiveDependencies = projectDependenciesSet.RecursiveProjectDependenciesByProjectFilePath_Exclusive[projectFilePath];
                    foreach (var dependencyProject in recursiveDependencies)
                    {
                        var dependencyAlreadyEvaluated = projectDependenciesSet.HasAspNetDependencyByProjectFilePath.ContainsKey(dependencyProject);
                        if (dependencyAlreadyEvaluated)
                        {
                            var dependencyHasAspNetDependency = projectDependenciesSet.HasAspNetDependencyByProjectFilePath[dependencyProject];
                            if (dependencyHasAspNetDependency)
                            {
                                // If the project has a dependency that is known to have an ASP.NET Core dependency, then the project is known to have an ASP.NET Core dependency.
                                hasAspNetDependency = true;
                                break;
                            }
                        }
                        else
                        {
                            // Add the dependency to the list of those needing evaluation.
                            projectsToEvaluate.Add(dependencyProject);
                        }
                    }

                    // Only if we know the project has an ASP.NET dependency based on dependency analysis do we add it, and are done.
                    if (hasAspNetDependency)
                    {
                        projectDependenciesSet.HasAspNetDependencyByProjectFilePath.Add(
                            projectFilePath,
                            hasAspNetDependency);

                        continue;
                    }

                    // Now we have to query the file.
                    var hasAspNetCoreAppFrameworkReference = Instances.ProjectFileOperator.InQueryProjectFileContext_Synchronous(
                        projectFilePath.Value,
                        projectElement =>
                        {
                            var hasAspNetCoreAppFrameworkReference = Instances.ProjectXmlOperator.HasFrameworkReference(
                                projectElement,
                                Instances.FrameworkNames.Microsoft_AspNetCore_App);

                            return hasAspNetCoreAppFrameworkReference;
                        });

                    // Because we have previously tested for having a recursive ASP.NET reference, we now know definitively whether the project has a ASP.NET Core reference.
                    projectDependenciesSet.HasAspNetDependencyByProjectFilePath.Add(
                        projectFilePath,
                        hasAspNetCoreAppFrameworkReference);
                }
            }
        }
    }
}
