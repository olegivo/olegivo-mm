using System;
using FluentNHibernate.Mapping;
using NHibernate.Mapping;
using Oleg_ivo.MeloManager.MediaObjects;

namespace MeloManager.Api.Models
{
    namespace EntityMappings
    {
        public class MediaContainerMap : ClassMap<MediaContainer>
        {
            /// <summary>
            /// Initializes a new instance of EntityTypeConfiguration
            /// </summary>
            public MediaContainerMap()
            {
                Table("MediaContainers");
                Id(x => x.Id).GeneratedBy.Identity();

                Map(x => x.Name);
                Map(x => x.IsRoot);
                Map(x => x.DateInsert).Generated.Insert();
                Map(x => x.DateUpdate).ReadOnly();

                HasManyToMany(x => x.ChildContainers)
                    .ParentKeyColumns.Add("ParentId")
                    .ChildKeyColumns.Add("ChildId")
                    .Cascade.All()
                    .Not.LazyLoad()
                    .Table("MediaContainersParentChilds");

                HasManyToMany(x => x.ParentContainers)
                    .ParentKeyColumns.Add("ChildId")
                    .ChildKeyColumns.Add("ParentId")
                    .Cascade.All()
                    .Table("MediaContainersParentChilds");

                HasManyToMany(x => x.Files)
                    .ParentKeyColumns.Add("MediaContainerId")
                    .ChildKeyColumns.Add("FileId")
                    .Cascade.All()
                    .Table("MediaContainerFiles");
            }
        }

        public class CategoryMap : SubclassMap<Category>
        {
            public CategoryMap()
            {
                Table("Categories");
                KeyColumn("Id");
            }
        }

        public class PlaylistMap : SubclassMap<Playlist>
        {
            public PlaylistMap()
            {
                Table("Playlists");
                KeyColumn("Id");
            }
        }

        public class MediaFileMap : SubclassMap<MediaFile>
        {
            public MediaFileMap()
            {
                Table("MediaFiles");
                KeyColumn("Id");
            }
        }

        public class FileMap : ClassMap<File>
        {
            /// <summary>
            /// Initializes a new instance of EntityTypeConfiguration
            /// </summary>
            public FileMap()
            {
                Table("Files");
                Id(x => x.Id).GeneratedBy.Identity();

                Map(x => x.FullFileName);
                Map(x => x.DateInsert).Generated.Insert();
                Map(x => x.DateUpdate).ReadOnly();

                HasManyToMany(x => x.MediaContainers)
                    .ParentKeyColumns.Add("FileId")
                    .ChildKeyColumns.Add("MediaContainerId")
                    .Cascade.All()
                    .Table("MediaContainerFiles");
            }
        }

        public class MediaContainersParentChildMap : ClassMap<MediaConainerParentChild>
        {
            public MediaContainersParentChildMap()
            {
                Table("MediaContainersParentChilds");

                Id(x => x.Id).GeneratedBy.Identity();
                Map(x => x.ParentId);
                Map(x => x.ChildId);
                Map(x => x.DateInsert).Generated.Insert();
            }
        }

        public class MediaConainerFileMap : ClassMap<MediaConainerFile>
        {
            public MediaConainerFileMap()
            {
                Table("MediaContainerFiles");

                Id(x => x.Id).GeneratedBy.Identity();
                Map(x => x.MediaContainerId);
                Map(x => x.FileId);
                Map(x => x.DateInsert).Generated.Insert();
            }
        }
    }

    public class MediaConainerParentChild
    {
        public virtual long Id { get; protected set; }
        public virtual long ParentId { get; set; }
        public virtual long ChildId { get; set; }
        public virtual DateTime DateInsert { get; protected set; }
    }

    public class MediaConainerFile
    {
        public virtual long Id { get; protected set; }
        public virtual long MediaContainerId { get; set; }
        public virtual long FileId { get; set; }
        public virtual DateTime DateInsert { get; protected set; }
    }
}