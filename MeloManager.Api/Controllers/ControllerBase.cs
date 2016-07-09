using System;
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

        [Route("")]
        public IEnumerable<object> Get([FromUri] TSearchParameter parameters)
        {
            var entities = GetEntities(parameters, Projection).ToList();
            return entities;
        }

        protected IEnumerable<TResult> GetEntities<TResult>(TSearchParameter parameters, Func<TEntity, TResult> selector)
        {
            return DbRequest(session =>
            {
                var query = Query(parameters, session);
                return parameters.ApplyLocalFilters(query.Future()).Select(selector).ToList();
            });
        }

        protected IEnumerable<TEntity> GetEntities(TSearchParameter parameters)
        {
            return GetEntities(parameters, entity => entity);
        }

        protected TResult DbRequest<TResult>(Func<ISession, TResult> request)
        {
            using (var sessionFactory = NHibernateHelper.CreateSessionFactory<MediaContainerParentChildsController>())
            {
                using (var session = sessionFactory.OpenSession())
                {
                    return request(session);
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