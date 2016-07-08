using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using MeloManager.Api.Models;
using NHibernate;
using Oleg_ivo.Base.Autofac.DependencyInjection;

namespace MeloManager.Api.Controllers
{
    public abstract class ControllerBase<TEntity, TSearchParameter> : ApiController
        where TEntity : class
        where TSearchParameter : SearchParameters<TEntity> 
    {
        [Dependency(Required = true)]
        public NHibernateHelper NHibernateHelper { get; set; }

        public IEnumerable<object> Get([FromUri] TSearchParameter parameters)
        {
            using (var sessionFactory = NHibernateHelper.CreateSessionFactory<MediaContainerParentChildsController>())
            {
                using (var session = sessionFactory.OpenSession())
                {
                    var query = Query(parameters, session);
                    var entities = parameters.ApplyLocalFilters(query.Future());
                    var list = entities.Select(Projection).ToList();
                    return list;
                }
            }
        }

        protected virtual IQueryOver<TEntity, TEntity> Query(TSearchParameter parameters, ISession session)
        {
            var query = session.QueryOver<TEntity>();
            parameters.ApplyDbFilters(query);
            return query;
        }

        protected abstract object Projection(TEntity entity);
    }
}