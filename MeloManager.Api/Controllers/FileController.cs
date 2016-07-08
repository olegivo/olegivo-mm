using NHibernate;
using NHibernate.Criterion;
using Oleg_ivo.MeloManager.MediaObjects;

namespace MeloManager.Api.Controllers
{
    public class FilesController : ControllerBase<File, FilesController.MediaContainerParameters>
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public class MediaContainerParameters : SearchParameters<File>
        {
            public long? Id { get; set; }
            public long? MediaContainerId { get; set; }

            public override void ApplyDbFilters(IQueryOver<File, File> query)
            {
                if (Id != null)
                {
                    query.Where(container => container.Id == Id);
                }
                if (MediaContainerId != null)
                {
                    var parentIdsQuery = QueryOver.Of<MediaContainer>().Where(p => p.Id == MediaContainerId).Select(p => p.Id);

                    MediaContainer parentContainer = null;
                    query.JoinAlias(file => file.MediaContainers, () => parentContainer)
                        .WithSubquery.WhereProperty(() => parentContainer.Id)
                        .In(parentIdsQuery);
                }
            }
        }

        protected override object Projection(File entity)
        {
            return new
            {
                entity.Id,
                entity.FullFileName
            };
        }
    }
}