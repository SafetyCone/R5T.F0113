using System;
using System.Collections.Generic;

using R5T.T0142;
using R5T.T0172;


namespace R5T.F0113
{
    /// <summary>
    /// Contains a set of dependencies (where each dependency is unique).
    /// </summary>
    [DataTypeMarker]
    public class AssembliesSet
    {
        public HashSet<IAssemblyFilePath> AssemblyFilePaths { get; }


        public AssembliesSet()
            : this(new HashSet<IAssemblyFilePath>())
        {
        }

        public AssembliesSet(IEnumerable<IAssemblyFilePath> assemblyFilePaths)
            : this()
        {
            this.AssemblyFilePaths.AddRange(assemblyFilePaths);
        }

        public AssembliesSet(HashSet<IAssemblyFilePath> assemblyFilePaths)
        {
            this.AssemblyFilePaths = assemblyFilePaths;
        }
    }
}
