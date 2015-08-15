using System;
using System.Data.Linq;
using System.IO;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Utils;

namespace Oleg_ivo.MeloManager.MediaObjects.Extensions
{
    public static class DataContextExtensions
    {
        public class DataContextLogHelper : StateHolder<TextWriter>
        {
            private readonly DataContext dataContext;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public DataContextLogHelper(DataContext dataContext, TextWriter textWriter)
                : base(() =>
                {
                    var old = dataContext.Log;
                    dataContext.Log = textWriter;
                    return old;
                },
                oldState => dataContext.Log = oldState)
            {
                this.dataContext = Enforce.ArgumentNotNull(dataContext, "dataContext");
                this.dataContext.Log = textWriter;
            }

            public void LogChangesInfo()
            {
                var changeSet = dataContext.GetChangeSet();
                dataContext.Log.WriteLine("Изменения для фиксации:");
                dataContext.Log.WriteLine("Вставок: {0}", changeSet.Inserts.Count);
                dataContext.Log.WriteLine("Обновлений: {0}", changeSet.Updates.Count);
                dataContext.Log.WriteLine("Удалений: {0}", changeSet.Deletes.Count);
            }

            public void SubmitChanges()
            {
                LogChangesInfo();
                dataContext.SubmitChanges();
            }
        }

        public static void ActionWithLog<TDataContext>(this TDataContext dataContext, Action<TDataContext> action) where TDataContext : DataContext
        {
            using (new DataContextLogHelper(dataContext, Console.Out))
            {
                action(dataContext);
            }
        }

        public static void ActionWithLog(this DataContext dataContext, Action action)
        {
            using (new DataContextLogHelper(dataContext, Console.Out))
            {
                action();
            }
        }

        public static void SubmitChangesWithLog(this DataContext dataContext)
        {
            using (var helper = new DataContextLogHelper(dataContext, Console.Out))
            {
                helper.SubmitChanges();
            }
        }
    }
}
