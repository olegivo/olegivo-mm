using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Oleg_ivo.MeloManager.MediaObjects;
using File = System.IO.File;

namespace MeloManager.Api.Controllers
{
    public class MediaFilesController : ControllerBase<MediaFile, MediaFilesController.MediaFileParameters>
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public class MediaFileParameters : SearchParameters<MediaFile>
        {

            public long? Id { get; set; }
            public bool? IsRoot { get; set; }
            public long? ParentId { get; set; }

            public override void ApplyDbFilters(IQueryOver<MediaFile, MediaFile> query)
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
                    query.JoinAlias(mediaFile => mediaFile.ParentContainers, () => parentContainer)
                        .WithSubquery.WhereProperty(() => parentContainer.Id)
                        .In(parentIdsQuery);
                }
            }
        }

        protected override object Projection(MediaFile entity)
        {
            return GetProjection(entity);
        }

        internal static MediaFileProjection GetProjection(MediaFile entity)
        {
            var files = entity.Files
                .AsEnumerable()
                .Where(f => File.Exists(f.FullFileName))
                .ToList();

            return new MediaFileProjection
            {
                Id = entity.Id,
                Type = entity.GetType().Name.ToLower(),
                RowGuid = entity.RowGuid,
                Name = entity.Name,
                Album = entity.Album,
                Artist = entity.Artist,
                Length = entity.Length,
                Title = entity.Title,
                Track = entity.Track,
                TrackCount = entity.TrackCount,
                Year = entity.Year,
                IsRoot = entity.IsRoot,
                DateUpdate = entity.DateUpdate?.ToUniversalTime() /*преобразование из локального времени (+3) в UTC*/,
                ParentContainersCount = entity.ParentContainers.Count,
                FilesCount = entity.Files.Count,
                File = files.Count == 1 ? FilesController.GetProjection(files.Single()) : null
            };
        }
    }
}