using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NLog;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.MediaObjects.Extensions;
using Oleg_ivo.MeloManager.Prism;
using Oleg_ivo.Tools.Utils;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    public class PlaylistImporter<TPlaylistFileAdapter> where TPlaylistFileAdapter : PlaylistFileAdapter
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        public PlaylistImporter(IComponentContext context, MediaDbContext dbContext, MeloManagerOptions options, IMediaCache mediaCache)
        {
            Adapter = context.ResolveUnregistered<TPlaylistFileAdapter>();
            DbContext = Enforce.ArgumentNotNull(dbContext, "dataContext");
            Options = Enforce.ArgumentNotNull(options, "options");
            MediaCache = Enforce.ArgumentNotNull(mediaCache, "mediaCache");
        }

        public TPlaylistFileAdapter Adapter { get; private set; }
        
        public MediaDbContext DbContext { get; private set; }

        public MeloManagerOptions Options { get; private set; }

        public IMediaCache MediaCache { get; private set; }

        public IEnumerable<Playlist> RunImportAll(IEnumerable<string> playlistFilenames, Category winampCategory)
        {
            var playlists = playlistFilenames.Select(filename => Import(filename, winampCategory)).ToList();
            if (winampCategory.Id == 0)
                DbContext.MediaContainers.Add(winampCategory);

            /*var files =
                DataContext.GetChangeSet()
                    .Inserts.OfType<File>()
                    .GroupBy(file => file.FullFileName.ToLowerInvariant())
                    .Where(g=>g.Count()>1)
                    .ToList();
            var mediaFiles =
                DataContext.GetChangeSet()
                    .Inserts.OfType<MediaFile>()
                    .Where(file => file.Name == @"Sixpence None the Richer - Kiss me.mp3")
                    .SelectMany(file => file.MediaContainerFiles.Select(mcf => mcf.File))
                    .Distinct()
                    .ToList();
            var doubles = DataContext.GetChangeSet()
                .Inserts.OfType<MediaFile>()
                .Where(mf => mf.MediaContainerFiles.Count > 1)
                .ToList();*/
            DbContext.SubmitChangesWithLog(log.Debug);
            return playlists;
        }

        public Playlist Import(string filename, Category importCategory = null)
        {
            log.Debug("Импорт плейлиста из файла {0}{1}", filename, importCategory!=null ? string.Format(@" в категорию ""{0}""", importCategory.Name) : null);
            var environmentVariableUsage = Utils.FileUtils.GetEnvironmentVariableUsage(filename);
            List<Playlist> playlists;
            Playlist playlist;
            
            if (environmentVariableUsage != null)
            {
                //плейлист попытается добавиться, а не обновлён, если на разных компьютерах он хранится в одном и том же файле, но в разных папках (AppData)
                var currentUser = Environment.UserName.ToLower();
                var otherUsers = Options.Users.OfType<string>().Where(user => user!=currentUser).ToList();

                var wrappedFilename = environmentVariableUsage.WrapPathWithVariable(filename);
                playlists = DbContext.Playlists
                    .AsEnumerable()
                    .Where(p => p.Files.Any(file =>
                    {
                        var fullFileName = file.FullFileName;
                        return new Func<string, string>(environmentVariableUsage.WrapPathWithVariable)(fullFileName) == wrappedFilename ||
                               otherUsers.Any(
                                   otherUser =>
                                       environmentVariableUsage.WrapPathWithVariable(fullFileName.Replace(otherUser, currentUser)) == wrappedFilename);
                    }))
                    .ToList();
                playlist = playlists.SingleOrDefault();
                if (playlist != null
                    && (playlist.OriginalFileName == null || environmentVariableUsage.WrapPathWithVariable(playlist.OriginalFileName) != wrappedFilename))
                {
                    playlist.Files.Add(MediaCache.GetOrAddCachedFile(filename));
                }
            }
            else
            {
                playlists = DbContext.MediaContainers.OfType<Playlist>()
                    .Where(p => p != null)
                    .AsEnumerable()
                    .Where(p => p.OriginalFileName == filename)
                    .ToList();
                playlist = playlists.SingleOrDefault();
            }

            var playlistFromFile = Adapter.FileToPlaylist(filename);
            if (playlist!=null)
            {
                GetUpdatePlaylistDiffAction(playlist, playlistFromFile).Apply();
            }
            else
            {
                playlist = GetAddPlaylistDiffAction(playlistFromFile, importCategory).ApplyAndReturnItem();
            }
            return playlist;
        }

        private ContainerDiffAction GetUpdatePlaylistDiffAction(Playlist playlist, PrePlaylist playlistFromFile)
        {
            Func<IEnumerable<MediaFile>, string, MediaFile> getMediaFile =
                (mediaFiles, filename) =>
                    mediaFiles.First(
                        mf =>
                            mf.Files.Any(
                                file =>
                                    string.Compare(file.FullFileName, filename,
                                        StringComparison.InvariantCultureIgnoreCase) == 0));

            Func<string, IDiffAction> onAdd = filename => new DiffAction<Playlist, MediaFile>(() => playlist,
                () => getMediaFile(playlistFromFile.MediaFiles, filename),
                (pl, mediaFile) =>
                {
                    pl.AddChildMediaFile(mediaFile);
                    log.Debug("Добавлен: {0}", mediaFile);
                }, DiffType.Added);

            Func<string, IDiffAction> onDelete = filename => new DiffAction<Playlist, MediaFile>(() => playlist,
                () => getMediaFile(playlist.MediaFiles, filename),
                (pl, mediaFile) =>
                {
                    DbContext.RemoveRelation(playlist, mediaFile);
                    log.Debug("Удалён: {0}", mediaFile);
                }, DiffType.Deleted);
            Func<string, string, IDiffAction> onEquals = (filename1, filename2) =>
                new DiffAction<Playlist, string>(() => playlist,
                    () => filename1,
                    (pl, filename) =>
                    {
                        log.Trace("Без изменения: {0}", filename);
                    }, DiffType.None);

            //compare old and new version
            var filesWas = GetFilenames(playlist.MediaFiles);
            var filesNow = GetFilenames(playlistFromFile.MediaFiles);
            var collectionDiff = DiffCreator.CreateCollectionDiff(filesWas, filesNow, onAdd, onDelete, onEquals);
            return new ContainerDiffAction(collectionDiff, DiffType.None)
            {
                PreAction = () => log.Debug("Обновление плейлиста {0}", playlist),
                PostAction = () => log.Debug("Обновление плейлиста {0} завершено", playlist),
            };
        }

        private DiffAction<Category, Playlist> GetAddPlaylistDiffAction(PrePlaylist playlistFromFile, Category importCategory)
        {
            return new DiffAction<Category, Playlist>(() =>
            {
                var rootCategory = importCategory ??
                   DbContext.MediaContainers.OfType<Category>().FirstOrDefault(c => c.IsRoot);
                if (rootCategory == null)
                {
                    log.Debug("Добавление корневой категории");

                    rootCategory = new Category { Name = "Плейлисты Winamp" };
                    DbContext.MediaContainers.Add(rootCategory);
                }
                return rootCategory;

            }, playlistFromFile.CreatePlaylist, (c, p) =>
            {
                c.AddChild(p);
                DbContext.MediaContainers.Add(p);
            }, DiffType.Added) { PreAction = () => log.Debug("Добавление плейлиста {0}", playlistFromFile) };
        }

        private IEnumerable<string> GetFilenames(IEnumerable<MediaFile> mediaFiles)
        {
            return
                mediaFiles.SelectMany(mediaFile => mediaFile.Files)
                    .Select(f => f.FileInfo)
                    //.Where(f => f.Exists)
                    .Select(f => f.FullName.ToLower());
        }

        private class FileComparer : IEqualityComparer<File>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            /// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
            public bool Equals(File x, File y)
            {
                if (x == y) return true;
                return GetHashCode(x) == GetHashCode(y);
            }

            /// <summary>
            /// Returns a hash code for the specified object.
            /// </summary>
            /// <returns>
            /// A hash code for the specified object.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
            public int GetHashCode(File obj)
            {
                var hash = 17;
                if (obj != null)
                {
                    if (obj.FullFileName != null) hash = hash * 23 + obj.FullFileName.GetHashCode();
                }
                return hash;
            }
        }

        private class MediaFileComparer : IEqualityComparer<MediaFile>
        {
            private readonly FileComparer fileComparer = new FileComparer();

            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            /// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
            public bool Equals(MediaFile x, MediaFile y)
            {
                if (x == y) return true;
                var join = GetFiles(x).Join(GetFiles(y), fileComparer);
                return @join.Any();
            }

            private IEnumerable<File> GetFiles(MediaFile mediaFile)
            {
                return mediaFile != null ? mediaFile.Files : Enumerable.Empty<File>();
            }

            /// <summary>
            /// Returns a hash code for the specified object.
            /// </summary>
            /// <returns>
            /// A hash code for the specified object.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
            public int GetHashCode(MediaFile obj)
            {
                var hash = 17;
                if (obj != null)
                {
                    hash = hash * 23 + obj.Id.GetHashCode();
                    if (obj.Name != null) hash = hash * 23 + obj.Name.GetHashCode();
                }
                return hash;
            }
        }
    }

}