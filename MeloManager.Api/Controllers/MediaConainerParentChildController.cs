using MeloManager.Api.Models;
using NHibernate;

namespace MeloManager.Api.Controllers
{
    public class MediaContainerParentChildsController : ControllerBase<MediaConainerParentChild, MediaContainerParentChildsController.MediaConainerParentChildParameters>
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public class MediaConainerParentChildParameters : SearchParameters<MediaConainerParentChild>
        {
            public long? ParentId { get; set; }
            public long? ChildId { get; set; }

            public override void ApplyDbFilters(IQueryOver<MediaConainerParentChild, MediaConainerParentChild> query)
            {
                if (ParentId != null)
                {
                    query.Where(parentChild => parentChild.ParentId == ParentId);
                }
                if (ChildId != null)
                {
                    query.Where(parentChild => parentChild.ChildId == ChildId);
                }
            }
        }

        protected override object Projection(MediaConainerParentChild entity)
        {
            return new
            {
                entity.ParentId,
                entity.ChildId,
            };
        }
    }
}