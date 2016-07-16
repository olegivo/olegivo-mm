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

        internal static object GetProjection(MediaFile entity)
        {
            var files = entity.Files
                .AsEnumerable()
                .Where(f => File.Exists(f.FullFileName))
                .ToList();

            return new
            {
                entity.Id,
                Type = entity.GetType().Name.ToLower(),
                entity.RowGuid,
                entity.Name,
                entity.Album,
                entity.Artist,
                entity.Length,
                entity.Title,
                entity.Track,
                entity.TrackCount,
                entity.Year,
                entity.IsRoot,
                entity.DateUpdate,
                ParentContainersCount = entity.ParentContainers.Count,
                FilesCount = entity.Files.Count,
                File = files.Count == 1 ? FilesController.GetProjection(files.Single()) : null
            };
        }
    }
}