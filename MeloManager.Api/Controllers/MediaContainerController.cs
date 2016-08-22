﻿using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Oleg_ivo.MeloManager.MediaObjects;

namespace MeloManager.Api.Controllers
{
    public class MediaContainersController : ControllerBase<MediaContainer, MediaContainersController.MediaContainerParameters>
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public enum MediaContainerTypes
        {
            Category,
            Playlist,
            MediaFile
        }

        public class MediaContainerParameters : SearchParameters<MediaContainer>
        {

            public long? Id { get; set; }
            public bool? IsRoot { get; set; }
            public long? ParentId { get; set; }
            public MediaContainerTypes? Type { get; set; }

            private static readonly Dictionary<MediaContainerTypes, Type> types =
                new Dictionary<MediaContainerTypes, Type>
                {
                    {MediaContainerTypes.Category, typeof (Category)},
                    {MediaContainerTypes.Playlist, typeof (Playlist)},
                    {MediaContainerTypes.MediaFile, typeof (MediaFile)},
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
                if (ParentId != null)
                {
                    var parentIdsQuery = QueryOver.Of<MediaContainer>().Where(p => p.Id == ParentId).Select(p => p.Id);

                    MediaContainer parentContainer = null;
                    query.JoinAlias(mc => mc.ParentContainers, () => parentContainer).WithSubquery.WhereProperty(() => parentContainer.Id).In(parentIdsQuery);
                }
            }
        }

        protected override object Projection(MediaContainer entity)
        {
            var mediaFile = entity as MediaFile;
            if (mediaFile != null)
            {
                return MediaFilesController.GetProjection(mediaFile);
            }

            return new
            {
                entity.Id,
                Type = entity.GetType().Name.ToLower(),
                entity.RowGuid,
                entity.Name,
                entity.IsRoot,
                entity.DateUpdate,
                ChildContainersCount = entity.ChildContainers.Count,
                ParentContainersCount = entity.ParentContainers.Count,
                FilesCount = entity.Files.Count,
            };
        }
    }
}