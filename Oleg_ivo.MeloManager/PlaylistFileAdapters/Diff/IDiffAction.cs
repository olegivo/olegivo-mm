using System.Collections.Generic;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff
{
    public interface IDiffAction
    {
        IList<IDiffAction> ChildDiffActions { get; }
        DiffType DiffType { get; }
        void Apply();
    }
}