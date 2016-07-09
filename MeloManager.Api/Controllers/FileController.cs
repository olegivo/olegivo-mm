using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using NHibernate;
using NHibernate.Criterion;
using Oleg_ivo.MeloManager.MediaObjects;
using File = Oleg_ivo.MeloManager.MediaObjects.File;

namespace MeloManager.Api.Controllers
{
    [RoutePrefix("files")]
    public class FilesController : ControllerBase<File, FilesController.FileParameters>
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public class FileParameters : SearchParameters<File>
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
            var fileInfo = new FileInfo(entity.FullFileName);
            return new
            {
                entity.Id,
                entity.Filename,
                fileInfo.CreationTimeUtc,
                fileInfo.LastWriteTimeUtc,
            };
        }

        [HttpGet]
        [Route("download")]
        public HttpResponseMessage Download([FromUri] FileParameters parameters)
        {
            var files = GetEntities(parameters).ToList();
            var file = files.Single();

            var stream = new FileStream(file.FullFileName, FileMode.Open);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content =
                    new StreamContent(stream)
                    {
                        Headers =
                        {
                            ContentType = new MediaTypeHeaderValue("application/octet-stream"),
                            ContentDisposition = new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = file.Filename
                            }
                        }
                    }
            };
            return result;

        }
    }
}