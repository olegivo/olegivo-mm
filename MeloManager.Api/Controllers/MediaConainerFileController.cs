using MeloManager.Api.Models;
using NHibernate;

namespace MeloManager.Api.Controllers
{
    public class MediaConainerFilesController : ControllerBase<MediaConainerFile, MediaConainerFilesController.MediaConainerFileParameters>
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public class MediaConainerFileParameters : SearchParameters<MediaConainerFile>
        {
            public long? MediaContainerId { get; set; }
            public long? FileId { get; set; }

            public override void ApplyDbFilters(IQueryOver<MediaConainerFile, MediaConainerFile> query)
            {
                if (MediaContainerId != null)
                {
                    query.Where(parentChild => parentChild.MediaContainerId == MediaContainerId);
                }
                if (FileId != null)
                {
                    query.Where(parentChild => parentChild.FileId == FileId);
                }
            }
        }

        protected override object Projection(MediaConainerFile entity)
        {
            return new
            {
                entity.MediaContainerId,
                entity.FileId,
            };
        }
    }
}