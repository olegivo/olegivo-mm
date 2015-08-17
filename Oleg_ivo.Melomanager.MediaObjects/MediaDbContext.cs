using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.IO;
using System.Linq;
using NLog;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.Tools.Utils;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    public class MediaDbContext : DbContext, IMediaRepository
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public MediaDbContext() : base("MeloManagerEF")
        {
            //Disable initializer
            Database.SetInitializer<MediaDbContext>(null);
        }

        /// <summary>
        /// This method is called when the model for a derived context has been initialized, but
        ///             before the model has been locked down and used to initialize the context.  The default
        ///             implementation of this method does nothing, but it can be overridden in a derived class
        ///             such that the model can be further configured before it is locked down.
        /// </summary>
        /// <remarks>
        /// Typically, this method is called only once when the first instance of a derived context
        ///             is created.  The model for that context is then cached and is for all further instances of
        ///             the context in the app domain.  This caching can be disabled by setting the ModelCaching
        ///             property on the given ModelBuidler, but note that this can seriously degrade performance.
        ///             More control over caching is provided through use of the DbModelBuilder and DbContextFactory
        ///             classes directly.
        /// </remarks>
        /// <param name="modelBuilder">The builder that defines the model for the context being created. </param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.AddFromAssembly(GetType().Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<MediaContainer> MediaContainers { get; set; }
        public virtual DbSet<File> Files { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Playlist> Playlists { get; set; }
        public virtual DbSet<MediaFile> MediaFiles { get; set; }
        //public DbSet<MediaContainersParentChild> MediaContainersParentChilds { get; set; }

        public Category CreateCategory()
        {
            var category = new Category();
            MediaContainers.Add(category);
            return category;
        }

        public Playlist CreatePlaylist(string originalFileName, IMediaCache mediaCache)
        {
            var playlist = new Playlist(originalFileName, mediaCache);
            MediaContainers.Add(playlist);
            return playlist;
        }

        public MediaFile CreateMediaFile()
        {
            var mediaFile = new MediaFile();
            MediaContainers.Add(mediaFile);
            return mediaFile;
        }

        /// <summary>
        /// Удаляет связь между родительским и дочерним контейнером
        /// </summary>
        public void RemoveRelation(MediaContainer parent, MediaContainer child)
        {
            parent.RemoveChild(child);
            child.RemoveParent(parent);

            //удаление связи, т.к. она сама себя не удаляет
            //parent.ChildMediaContainers.Remove(parentRelation);
            //child.ParentMediaContainers.Remove(childRelation);
            //MediaContainersParentChilds.DeleteOnSubmit(parentRelation);
            //MediaContainersParentChilds.DeleteOnSubmit(childRelation);
        }

        public bool RemoveIfOrphan(MediaContainer mediaContainer)
        {
            //в случае сиротства удаляем сам элемент контейнера из базы
            if (mediaContainer.ParentContainers != null && mediaContainer.ParentContainers.Any()) return false;

            //if (UnderlyingItem.Id > 0) 

            //удаление связи с файлами и самих файлов, если это единственая связь
            if (mediaContainer.Files.Any())
            {
                Files.RemoveRange(mediaContainer.Files.Where(file => file.MediaContainers.Count == 1));
            }

            MediaContainers.Remove(mediaContainer);
            return true;
        }

        private class FileExt
        {
            public FileExt(File file)
            {
                File = file;
                RefreshExists();
            }

            public readonly File File;
            public bool Exists;

            public void RefreshExists()
            {
                Exists = System.IO.File.Exists(File.FullFileName);
            }
        }

        private ConcurrentDictionary<string, FileExt> filesCache = new ConcurrentDictionary<string,FileExt>();
        private ConcurrentDictionary<string, MediaFile> mediaFilesCache = new ConcurrentDictionary<string, MediaFile>();
        private ConcurrentDictionary<string, Playlist> playlistsCache = new ConcurrentDictionary<string, Playlist>();

        public void RefreshCache()
        {
            using (new ElapsedAction(elapsed => log.Debug("RefreshCache completed in {0}", elapsed)))
            {
                MediaContainers.Include(mc => mc.Files)
                    .Include(mc => mc.ParentContainers)
                    .Include(mc => mc.ChildContainers)
                    .Load();

                //this.ActionWithLog(() => {}, Console.WriteLine);
                //var mediaContainers = Playlists.ToList();
                var d = Files//.ToList()
                    .Select(file =>
                        new
                        {
                            file.FullFileName,
                            MediaFiles = file.MediaContainers.OfType<MediaFile>().ToList(),
                            //BUG: пока Playlist реализует интерфейс, IEnumerable<MediaFile>, запросы на фильтрацию по типу Playlist не работают
                            //Playlists = file.MediaContainers.OfType<Playlist>().ToList(), 
                            file
                        })
                    .ToList();
                var list1 = Playlists.Where(playlist => !playlist.Files.Any()).ToList();
                var list = MediaContainers.Where(mc => (/*mc is Playlist ||*/ mc is MediaFile) && !mc.Files.Any()).ToList();
                //if (list.Any())
                //    throw new InvalidOperationException();
                //var list = d.ToList();
                //mediaFilesCache = new Dictionary<string, MediaFile>();
                //foreach (var arg in list)
                //{
                //    mediaFilesCache.Add(arg.FullFileName, arg.mediaFile);
                //}

                filesCache =
                new ConcurrentDictionary<string, FileExt>(d.ToDictionary(arg => arg.FullFileName.ToLower(), arg => new FileExt(arg.file)));
                mediaFilesCache =
                    new ConcurrentDictionary<string, MediaFile>(d
                        //.Where(arg => arg.MediaFiles.Any() || arg.Playlists.Any())
                        .Where(item => item.MediaFiles.Any())
                        .ToDictionary(item => item.FullFileName.ToLower(), item => item.MediaFiles.Single()));
                //PlaylistsCache = d.Where(arg => arg.Playlist!=null).ToDictionary(arg => arg.FullFileName, arg => arg.Playlist);
            }
            /*
            mediaFilesCache =
                MediaContainers.OfType<MediaFile>()
                    .SelectMany(
                        mediaFile => mediaFile.MediaContainerFiles.Select(mcf => new {mcf.File.FullFileName, mediaFile}))
                    .ToDictionary(arg => arg.FullFileName, arg => arg.mediaFile);*/
        }

        public File GetOrAddCachedFile(string fullFilename)
        {
            return GetOrAddFile(fullFilename).File;
        }

        public bool GetOrAddFileExists(string fullFilename, bool refreshExists = false)
        {
            var fileExt = GetOrAddFile(fullFilename);
            if (refreshExists) fileExt.RefreshExists();
            return fileExt.Exists;
        }

        private FileExt GetOrAddFile(string fullFilename)
        {
            return filesCache.GetOrAdd(fullFilename.ToLower(),
                key =>
                {
                    var fileName = Path.GetFileName(fullFilename);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFilename);
                    var extension = Path.GetExtension(fullFilename);
                    var drive = Path.GetPathRoot(fullFilename);
                    var path = Path.GetDirectoryName(fullFilename);
                    return new FileExt(
                        new File
                        {
                            FullFileName = fullFilename,
                            Drive = drive,
                            Path = path,
                            Filename = fileName,
                            FileNameWithoutExtension = fileNameWithoutExtension,
                            Extention = extension
                        });
                });
        }

        public MediaFile GetOrAddCachedMediaFile(string filename)
        {
            return mediaFilesCache.GetOrAdd(filename.ToLower(),
                key =>
                {
                    var mediaFile = mediaFilesCache.GetValueOrDefault(key);
                    if (mediaFile != null) return mediaFile;

                    var file = GetOrAddCachedFile(filename);
                    mediaFile = new MediaFile { Name = file.Filename };
                    mediaFile.Files.Add(file);
                    return mediaFile;
                });
        }

        public Playlist GetOrAddCachedPlaylist(string filename, string playlistName = null)
        {
            return playlistsCache.GetOrAdd(filename.ToLower(),
                key => new Playlist(filename, this) { Name = playlistName });
        }
    }

    namespace EntityMapping
    {
        internal class MediaContainerConfiguration : EntityTypeConfiguration<MediaContainer>
        {
            /// <summary>
            /// Initializes a new instance of EntityTypeConfiguration
            /// </summary>
            public MediaContainerConfiguration()
            {

                ToTable("MediaContainers")
                    .HasKey(item => item.Id)
                    .Ignore(item => item.IsRepaired);

                Property(item => item.DateInsert).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
                Property(item => item.DateUpdate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
                
                HasMany(mc => mc.Files)
                    .WithMany(file => file.MediaContainers)
                    .Map(c =>
                    {
                        c.MapLeftKey("MediaContainerId");
                        c.MapRightKey("FileId");
                        c.ToTable("MediaContainerFiles");
                    });
                HasMany(childMediaContainer => childMediaContainer.ParentContainers)
                    .WithMany(parentMediaContainer => parentMediaContainer.ChildContainers)
                    .Map(c =>
                    {
                        c.MapLeftKey("ChildId");
                        c.MapRightKey("ParentId");
                        c.ToTable("MediaContainersParentChilds");
                    });
            }
        }

        internal class FileConfiguration : EntityTypeConfiguration<File>
        {
            /// <summary>
            /// Initializes a new instance of EntityTypeConfiguration
            /// </summary>
            public FileConfiguration()
            {
                ToTable("Files").HasKey(item => item.Id);
                Property(item => item.FullFileName).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
                Property(item => item.DateInsert).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
                Property(item => item.DateUpdate).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            }
        }

        internal class CategoryConfiguration : EntityTypeConfiguration<Category>
        {
            /// <summary>
            /// Initializes a new instance of EntityTypeConfiguration
            /// </summary>
            public CategoryConfiguration()
            {
                ToTable("Categories");
                Ignore(item => item.ParentCategory);
            }
        }

        internal class PlaylistConfiguration : EntityTypeConfiguration<Playlist>
        {
            /// <summary>
            /// Initializes a new instance of EntityTypeConfiguration
            /// </summary>
            public PlaylistConfiguration()
            {
                ToTable("Playlists");
            }
        }

        internal class MediaFileConfiguration : EntityTypeConfiguration<MediaFile>
        {
            /// <summary>
            /// Initializes a new instance of EntityTypeConfiguration
            /// </summary>
            public MediaFileConfiguration()
            {
                ToTable("MediaFiles");
            }
        }

    }

}
