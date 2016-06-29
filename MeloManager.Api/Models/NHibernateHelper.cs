using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;

namespace MeloManager.Api.Models
{
    public class NHibernateHelper
    {
        public NHibernateHelper(string connectionString)
        {
            ConnectionString = connectionString;
            DefaultSchema = "dbo";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TAncorType">Тип для к сборке, в которой хранятся маппинги сущностей</typeparam>
        /// <returns></returns>
        public ISessionFactory CreateSessionFactory<TAncorType>()
        {
            var sessionFactory = Fluently.Configure()
                .Database(
                    MsSqlConfiguration.MsSql2008
                        .DefaultSchema(DefaultSchema)
                        .Driver<LoggerSqlClientDriver>()
                        .ConnectionString(ConnectionString))
                .Mappings(m =>
                {
                    m.FluentMappings.AddFromAssemblyOf<TAncorType>();
                })
                .Cache(c =>
                {
                    c.UseSecondLevelCache()
                        .UseQueryCache()
                        .ProviderClass<NHibernate.Cache.HashtableCacheProvider>();
                })
                .BuildSessionFactory();
            return sessionFactory;
        }

        public string DefaultSchema { get; set; }

        public string ConnectionString { get; set; }

        protected void SaveOrUpdate(object obj, ISession session, bool flushIfSessionDirty = true)
        {
            session.SaveOrUpdate(obj);
            if (flushIfSessionDirty && session.IsDirty()) session.Flush();
        }
    }
}