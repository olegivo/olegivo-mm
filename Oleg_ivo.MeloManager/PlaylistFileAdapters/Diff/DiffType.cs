using System;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff
{
    [Flags]
    public enum DiffType
    {
        None = 0,
        Added = 1,
        Modified = 2,
        Deleted = 4
    }
}