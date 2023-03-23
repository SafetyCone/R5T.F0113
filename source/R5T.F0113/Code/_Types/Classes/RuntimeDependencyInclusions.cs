using System;

using R5T.T0142;


namespace R5T.F0113
{
    [DataTypeMarker]
    public class RuntimeDependencyInclusions
    {
        public bool NetCore { get; set; }
        public bool AspNetCore { get; set; }
        public bool WindowsFormsCore { get; set; }
    }
}
