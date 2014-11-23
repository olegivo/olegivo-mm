using System;
using System.Data.Linq;
using System.IO;
using Oleg_ivo.Base.Autofac;

namespace Oleg_ivo.MeloManager.Extensions
{
    public static class DataContextExtensions
    {
        public class DataContextLogHelper : IDisposable
        {
            private readonly TextWriter textWriter;
            private readonly DataContext dataContext;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public DataContextLogHelper(DataContext dataContext, TextWriter textWriter)
            {
                this.textWriter = textWriter;
                this.dataContext = Enforce.ArgumentNotNull(dataContext, "dataContext");
                this.dataContext.Log = textWriter;
            }

            public void LogChangesInfo()
            {
                var changeSet = dataContext.GetChangeSet();
                textWriter.WriteLine("Изменения для фиксации:");
                textWriter.WriteLine("Вставок: {0}", changeSet.Inserts.Count);
                textWriter.WriteLine("Обновлений: {0}", changeSet.Updates.Count);
                textWriter.WriteLine("Удалений: {0}", changeSet.Deletes.Count);
            }

            public void SubmitChanges()
            {
                LogChangesInfo();
                dataContext.SubmitChanges();
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                dataContext.Log = null;
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
