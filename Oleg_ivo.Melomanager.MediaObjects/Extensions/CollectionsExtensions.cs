using System.Collections;
using System.Collections.Specialized;
using System.Linq;

namespace Oleg_ivo.MeloManager.MediaObjects.Extensions
{
    public static class CollectionsExtensions
    {
        private static readonly ArrayList empty = new ArrayList();
        public static T GetChange<T>(this NotifyCollectionChangedEventArgs args)
        {
            
            if (args.Action == NotifyCollectionChangedAction.Add || args.Action == NotifyCollectionChangedAction.Remove)
            {
                var source = (args.Action == NotifyCollectionChangedAction.Add ? args.OldItems : args.NewItems) ?? empty;
                var target = (args.Action == NotifyCollectionChangedAction.Remove ? args.OldItems : args.NewItems) ?? empty;
                return target.Cast<T>().Except(source.Cast<T>()).SingleOrDefault();
            }
            return default(T);
        }
    }
}
