using System;
using System.Collections.Generic;
using System.Linq;
using Oleg_ivo.Base.Autofac;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff
{
    public class ContainerDiffAction<TItem> : ItemDiffAction<TItem> where TItem : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ContainerDiffAction(Func<TItem> itemProvider, IList<IDiffAction> childDiffActions, DiffType defaultDiffType)
            : base(itemProvider, childDiffActions)
        {
            Enforce.ArgumentNotNull(childDiffActions, "childDiffActions");
            DiffType = childDiffActions.Any(action => action.DiffType != DiffType.None)
                ? defaultDiffType
                : DiffType.None;
        }
    }
}