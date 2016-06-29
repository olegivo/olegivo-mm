using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MeloManager.Api.Models;
using NHibernate;
using NHibernate.Criterion;
using Oleg_ivo.MeloManager.MediaObjects;

namespace MeloManager.Api.Controllers
{
    public class MediaContainersController : ControllerBase<MediaContainer, MediaContainersController.MediaContainerParameters>
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public class MediaContainerParameters : SearchParameters<MediaContainer>
        {
            public long? Id { get; set; }
            public bool? IsRoot { get; set; }
            public long? ParentId { get; set; }

            public override void ApplyDbFilters(IQueryOver<MediaContainer, MediaContainer> query)
            {
                if (Id != null)
                {
                    query.Where(container => container.Id == Id);
                }
                if (IsRoot != null)
                {
                    query.Where(container => container.IsRoot == IsRoot);
                }
                if (ParentId != null)
                {
                    var parentIdsQuery = QueryOver.Of<MediaContainer>().Where(p => p.Id == ParentId).Select(p => p.Id);

                    MediaContainer parentContainer = null;
                    query.JoinAlias(mc => mc.ParentContainers, () => parentContainer)
                        .WithSubquery.WhereProperty(() => parentContainer.Id).In(parentIdsQuery);
                }
            }
        }

        protected override object Projection(MediaContainer mediaContainer)
        {
            return new
            {
                mediaContainer.Id,
                mediaContainer.Name,
                mediaContainer.IsRoot,
                mediaContainer.DateUpdate,
                ChildContainersCount = mediaContainer.ChildContainers.Count,
                ParentContainersCount = mediaContainer.ParentContainers.Count,
                FilesCount = mediaContainer.Files.Count,
            };
        }
    }
}