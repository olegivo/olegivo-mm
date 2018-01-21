using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NHibernate;
using NHibernate.Criterion;
using Oleg_ivo.MeloManager.MediaObjects;

namespace MeloManager.Api.Controllers
{
    [RoutePrefix("MediaContainers")]
    public class MediaContainersController : ControllerBase<MediaContainer, MediaContainersController.MediaContainerParameters>
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<string, string> map = new Dictionary<string, string>
        {
            {nameof(Category).ToLower(), "categories"},
            {nameof(Playlist).ToLower(), "playlists"},
            {nameof(MediaFile).ToLower(), "mediafiles"},
        };

        public class MediaContainerParameters : SearchParameters<MediaContainer>
        {

            public long? Id { get; set; }
            public bool? IsRoot { get; set; }
            public long? ParentId { get; set; }
            public MediaContainerTypes? Type { get; set; }

            private static readonly Dictionary<MediaContainerTypes, Type> types =
                new Dictionary<MediaContainerTypes, Type>
                {
                    {MediaContainerTypes.Category, typeof(Category)},
                    {MediaContainerTypes.Playlist, typeof(Playlist)},
                    {MediaContainerTypes.MediaFile, typeof(MediaFile)},
                };

            public override void ApplyDbFilters(IQueryOver<MediaContainer, MediaContainer> query)
            {
                if (Type != null)
                {
                    var type = types[Type.Value];
                    query.Where(mc => mc.GetType() == type);
                }

                if (Id != null)
                {
                    query.Where(container => container.Id == Id);
                }

                if (IsRoot != null)
                {
                    query.Where(container => container.IsRoot == IsRoot);
                }

                // ReSharper disable once InvertIf
                if (ParentId != null)
                {
                    var parentIdsQuery = QueryOver.Of<MediaContainer>().Where(p => p.Id == ParentId).Select(p => p.Id);

                    MediaContainer parentContainer = null;
                    query.JoinAlias(mc => mc.ParentContainers, () => parentContainer).WithSubquery
                        .WhereProperty(() => parentContainer.Id).In(parentIdsQuery);
                }
            }
        }

        [Route("ByType")]
        public Dictionary<string, List<MediaContainerProjection>> GetByType([FromUri] MediaContainerParameters parameters)
        {
            return GetEntities(parameters, Projection)
                .Cast<MediaContainerProjection>()
                .GroupBy(item => map[item.Type])
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        protected override object Projection(MediaContainer entity)
        {
            var mediaFile = entity as MediaFile;
            if (mediaFile != null)
            {
                return MediaFilesController.GetProjection(mediaFile);
            }

            return new MediaContainerProjection
            {
                Id = entity.Id,
                Type = entity.GetType().Name.ToLower(),
                RowGuid = entity.RowGuid,
                Name = entity.Name,
                IsRoot = entity.IsRoot,
                DateUpdate = entity.DateUpdate?.ToUniversalTime() /*преобразование из локального времени (+3) в UTC*/,
                ChildContainersCount = entity.ChildContainers.Count,
                ParentContainersCount = entity.ParentContainers.Count,
                FilesCount = entity.Files.Count,
            };
        }

        public enum MediaContainerTypes
        {
            Category,
            Playlist,
            MediaFile
        }
    }
}