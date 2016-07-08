using NHibernate;
using Oleg_ivo.MeloManager.MediaObjects;

namespace MeloManager.Api.Controllers
{
    public class FilesController : ControllerBase<File, FilesController.MediaContainerParameters>
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public class MediaContainerParameters : SearchParameters<File>
        {
            public long? Id { get; set; }

            public override void ApplyDbFilters(IQueryOver<File, File> query)
            {
                if (Id != null)
                {
                    query.Where(container => container.Id == Id);
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