using System;
using System.Data.Entity;
using System.Linq;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.Base.Utils;

namespace Oleg_ivo.MeloManager.MediaObjects.Extensions
{
    public static class DbContextExtensions
    {
        public class DbContextLogHelper : StateHolder<Action<string>>
        {
            private readonly DbContext dbContext;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public DbContextLogHelper(DbContext dbContext, Action<string> logger = null)
                : base(() =>
                {
                    var old = dbContext.Database.Log;
                    dbContext.Database.Log = logger ?? Console.WriteLine;
                    return old;
                }, 
                oldState => dbContext.Database.Log = oldState)
            {
                this.dbContext = Enforce.ArgumentNotNull(dbContext, "dbContext");
            }

            public void LogChangesInfo()
            {
                dbContext.ChangeTracker.DetectChanges();
                var changeSet =
                    dbContext.ChangeTracker.Entries()
                        .Where(entry => entry.State != EntityState.Unchanged)
                        .GroupBy(entry => entry.State)
                        .Select(g => string.Format("{0} :{1}", g.Key, g.Count()))
                        .ToList();
                dbContext.Database.Log("Изменения для фиксации:");
                dbContext.Database.Log(changeSet.Any() ? changeSet.JoinToString("\n") : "Нет");
            }

            public void SubmitChanges()
            {
                LogChangesInfo();
                dbContext.SaveChanges();
            }

        }

        public static void ActionWithLog<TDataContext>(this TDataContext dataContext, Action<TDataContext> action, Action<string> logger = null) where TDataContext : DbContext
        {
            using (new DbContextLogHelper(dataContext, logger))
            {
                action(dataContext);
            }
        }

        public static void ActionWithLog(this DbContext dataContext, Action action, Action<string> logger = null)
        {
            using (new DbContextLogHelper(dataContext, logger))
            {
                action();
            }
        }

        public static TResult FuncWithLog<TResult>(this DbContext dataContext, Func<TResult> func, Action<string> logger = null)
        {
            using (new DbContextLogHelper(dataContext, logger))
            {
                return func();
            }
        }

        public static void SubmitChangesWithLog(this DbContext dataContext, Action<string> logger = null)
        {
            using (var helper = new DbContextLogHelper(dataContext, logger))
            {
                helper.SubmitChanges();
            }
        }

        public static bool HasChanges(this DbContext dataContext)
        {
            return dataContext.ChangeTracker.HasChanges();
        }
    }
}