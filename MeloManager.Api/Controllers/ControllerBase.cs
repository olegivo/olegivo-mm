using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using MeloManager.Api.Models;
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
                    var query = session.QueryOver<TEntity>();
                    parameters.ApplyDbFilters(query);
                    var entities = parameters.ApplyLocalFilters(query.Future());
                    return entities.Select(Projection).ToList();
                }
            }
        }

        protected abstract object Projection(TEntity mediaContainer);
    }
}