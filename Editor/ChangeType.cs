using System;

namespace Willykc.Templ.Editor
{
    [Flags]
    public enum ChangeType
    {
        None = 0,
        Import = 1,
        Move = 2,
        Delete = 4,
        All = 7
    }
}
