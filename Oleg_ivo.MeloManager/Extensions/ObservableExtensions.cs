using System;
using System.Reactive.Linq;

namespace Oleg_ivo.MeloManager.Extensions
{
    public static class ObservableExtensions
    {
        /// <summary>
        /// Импортировано для совместимости с библиотекой Reactive Properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<T> Changes<T>(this IObservable<T> source)
        {
            return source.Skip(1);
        }

    }
}
