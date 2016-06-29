using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Autofac;
using MeloManager.Api.Models;
using Oleg_ivo.Base.Extensions;

namespace MeloManager.Api
{
    public class SqlDbConnectionModule : Module
    {
        public SqlDbConnectionModule(string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            this.ConnectionString = connectionString;
        }

        public static string SelectConnectionString(ConnectionStringSettingsCollection settings, string key)
        {
            if (!key.IsNullOrEmpty())
                return settings[key].ConnectionString;
            else
                return settings.OfType<ConnectionStringSettings>().First(setting => setting.ElementInformation.IsPresent).ConnectionString;
        }

        protected string ConnectionString { get; private set; }


        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            var connBuilder = new SqlConnectionStringBuilder(ConnectionString);
            builder.RegisterInstance(connBuilder);
            builder.RegisterAdapter((SqlConnectionStringBuilder cs) => new SqlConnection(cs.ToString())).As<SqlConnection, DbConnection, IDbConnection>().ExternallyOwned();
            builder.RegisterAdapter((SqlConnectionStringBuilder cs) => new NHibernateHelper(cs.ToString())).ExternallyOwned();
        }
    }
}