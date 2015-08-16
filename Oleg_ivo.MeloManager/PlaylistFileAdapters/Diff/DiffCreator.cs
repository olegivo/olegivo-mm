using System;
using System.Collections.Generic;
using Oleg_ivo.Base.Extensions;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff
{
    public class DiffCreator
    {
        public static IList<IDiffAction> CreateCollectionDiff<T>(IEnumerable<T> oldVersion, IEnumerable<T> newVersion, Func<T, IDiffAction> onAdd, Func<T, IDiffAction> onDelete, Func<T, T, IDiffAction> onEquals, IEqualityComparer<T> comparer = null)
        {
            var foj = oldVersion.FullOuterJoin(newVersion, comparer);
            IList<IDiffAction> diffActions = new List<IDiffAction>();
            foreach (var item in foj)
            {
                if (item.Item1 == null)
                {
                    diffActions.Add(onAdd(item.Item2));
                }
                else if (item.Item2 == null)
                {
                    diffActions.Add(onDelete(item.Item1));
                }
                else
                {
                    diffActions.Add(onEquals(item.Item1, item.Item2));
                }
            }

            return diffActions;
        }
    }
}