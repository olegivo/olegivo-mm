using System;
using System.Collections.Generic;
using NHibernate;

namespace MeloManager.Api.Controllers
{
    public abstract class SearchParameters<TEntity> where TEntity : class
    {
        public abstract void ApplyDbFilters(IQueryOver<TEntity, TEntity> query);

        public virtual IEnumerable<TEntity> ApplyLocalFilters(IEnumerable<TEntity> enumerable)
        {
            return enumerable;
        }
    }


}