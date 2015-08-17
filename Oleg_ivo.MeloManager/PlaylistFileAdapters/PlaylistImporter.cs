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
using Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff;
using Oleg_ivo.MeloManager.Prism;
using Oleg_ivo.Tools.Utils;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    public class PlaylistImporter<TPlaylistFileAdapter> where TPlaylistFileAdapter : PlaylistFileAdapter
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        public PlaylistImporter(IComponentContext context, IMediaRepository mediaRepository, MeloManagerOptions options)
        {
            Adapter = context.ResolveUnregistered<TPlaylistFileAdapter>();
            MediaRepository = Enforce.ArgumentNotNull(mediaRepository, "mediaRepository");
            Options = Enforce.ArgumentNotNull(options, "options");
        }

        public TPlaylistFileAdapter Adapter { get; private set; }

        public IMediaRepository MediaRepository { get; private set; }

        public MeloManagerOptions Options { get; private set; }

        /// <summary>
        /// Импорт файлов плейлистов.
        /// Если при импорте над плейлистами были произведены действительные действия импорта, будет вызвано <see cref="MediaRepository"/>.SaveChanges
        /// </summary>
        /// <param name="playlistFilenames"></param>
        /// <param name="winampCategory"></param>
        /// <returns>Возвращаются только те плейлисты, над которыми были произведены действительные действия импорта</returns>
        public IEnumerable<Playlist> Import(IEnumerable<string> playlistFilenames, Category winampCategory)
        {
            var filenames = playlistFilenames as IList<string> ?? playlistFilenames.ToList();
            var importResult = filenames.Select(filename => GetImportDiffAction(filename, winampCategory))
                .Where(diffAction => diffAction.DiffType!=DiffType.None)
                .Select(diffAction => diffAction.ApplyAndReturnItem())
                .ToList();
            if (importResult.Any())
            {
                if (winampCategory.Id == 0)
                    MediaRepository.MediaContainers.Add(winampCategory);

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
                log.Debug("При импорте файлов плейлистов ({0} шт.) было импортировано {1}", filenames.Count, importResult.Count);
            }
            else
            {
                log.Debug("При попытке импорта файлов плейлистов ({0} шт.) не обнаружилось различий", filenames.Count);
            }
            return importResult;
        }

        public ItemDiffAction<Playlist> GetImportDiffAction(string filename, Category importCategory = null)
        {
            var environmentVariableUsage = Utils.FileUtils.GetEnvironmentVariableUsage(filename);
            List<Playlist> playlists;
            Playlist playlist;

            if (environmentVariableUsage != null)
            {
                //плейлист попытается добавиться, а не обновлён, если на разных компьютерах он хранится в одном и том же файле, но в разных папках (AppData)
                var currentUser = Environment.UserName.ToLower();
                var otherUsers = Options.Users.OfType<string>().Where(user => user != currentUser).ToList();

                var wrappedFilename = environmentVariableUsage.WrapPathWithVariable(filename);
                playlists = MediaRepository.Playlists
                    .AsEnumerable()
                    .Where(p => p.Files.Any(file =>
                    {
                        var fullFileName = file.FullFileName;
                        return new Func<string, string>(environmentVariableUsage.WrapPathWithVariable)(fullFileName) ==
                               wrappedFilename ||
                               otherUsers.Any(
                                   otherUser =>
                                       environmentVariableUsage.WrapPathWithVariable(fullFileName.Replace(otherUser, currentUser)) ==
                                       wrappedFilename);
                    }))
                    .ToList();
                playlist = playlists.SingleOrDefault();
                if (playlist != null)
                {
                    var originalFileName = playlist.GetOriginalFileName(MediaRepository);
                    if (originalFileName == null ||
                        environmentVariableUsage.WrapPathWithVariable(originalFileName) != wrappedFilename)
                    {
                        playlist.Files.Add(MediaRepository.GetOrAddCachedFile(filename));
                    }
                }
            }
            else
            {
                playlists = MediaRepository.Playlists
                    .Where(p => p != null)
                    .AsEnumerable()
                    .Where(p => p.GetOriginalFileName(MediaRepository) == filename)
                    .ToList();
                playlist = playlists.SingleOrDefault();
            }

            var playlistFromFile = Adapter.FileToPlaylist(filename);
            var diffAction = playlist != null
                ? GetUpdatePlaylistDiffAction(playlist, playlistFromFile)
                : GetAddPlaylistDiffAction(playlistFromFile, importCategory);

            return new ContainerDiffAction<Playlist>(() => playlist, new IDiffAction[] {diffAction}, diffAction.DiffType)
            {
                PreAction =
                    () =>
                        log.Debug("Импорт плейлиста из файла {0}{1}", filename,
                            importCategory != null ? string.Format(@" в категорию ""{0}""", importCategory.Name) : null)
            };
        }

        private ContainerDiffAction<Playlist> GetUpdatePlaylistDiffAction(Playlist playlist, PrePlaylist playlistFromFile)
        {
            Func<IEnumerable<MediaFile>, string, MediaFile> getMediaFile =
                (mediaFiles, filename) =>
                    mediaFiles.First(mf => mf.Files.Any(file => file.Equals(filename)));

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
                    MediaRepository.RemoveRelation(playlist, mediaFile);
                    MediaRepository.RemoveIfOrphan(mediaFile);
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
            var filesWas = GetFilenames(playlist);
            var filesNow = GetFilenames(playlistFromFile);
            var collectionDiff = DiffCreator.CreateCollectionDiff(filesWas, filesNow, onAdd, onDelete, onEquals);
            
            return new ContainerDiffAction<Playlist>(() => playlist, collectionDiff, DiffType.Modified)
            {
                PreAction = () => log.Debug("Обновление плейлиста {0}", playlist),
                PostAction = () => log.Debug("Обновление плейлиста {0} завершено", playlist),
            };
        }

        private ItemDiffAction<Playlist> GetAddPlaylistDiffAction(PrePlaylist playlistFromFile, Category importCategory)
        {
            return new DiffAction<Category, Playlist>(
                () =>
                {
                    var rootCategory = importCategory ??
                                       MediaRepository.Categories.FirstOrDefault(c => c.IsRoot);
                    if (rootCategory == null)
                    {
                        log.Debug("Добавление корневой категории");

                        rootCategory = new Category {Name = "Плейлисты Winamp"};
                        MediaRepository.Categories.Add(rootCategory);
                    }
                    return rootCategory;
                },
                playlistFromFile.CreatePlaylist,
                (category, playlist) =>
                {
                    category.AddChild(playlist);
                    MediaRepository.Playlists.Add(playlist);
                },
                DiffType.Added)
            {
                PreAction = () => log.Debug("Добавление плейлиста {0}", playlistFromFile)
            };
        }

        private IEnumerable<string> GetFilenames(IEnumerable<MediaFile> mediaFiles)
        {
            return
                mediaFiles.SelectMany(mediaFile => mediaFile.Files)
                    .Select(f => f.FileInfo)
                    //.Where(f => f.Exists)
                    .Select(f => f.FullName.ToLower());
        }


        public IDiffAction GetExportDiffAction(Playlist playlist, string exportFilename)
        {
            if (!System.IO.File.Exists(exportFilename))
            {
                return new SimpleDiffAction(
                    () =>
                    {
                        Adapter.PlaylistToFile(playlist, exportFilename);
                        if (!playlist.Files.Any(file => file.Equals(exportFilename)))
                            playlist.Files.Add(MediaRepository.GetOrAddCachedFile(exportFilename));
                    },
                    DiffType.Added);
            }

            var playlistFromFile = Adapter.FileToPlaylist(exportFilename);

            Func<IEnumerable<MediaFile>, string, MediaFile> getMediaFile =
                (mediaFiles, filename) =>
                    mediaFiles.First(mf => mf.Files.Any(file => file.Equals(filename)));

            Func<string, IDiffAction> onAdd =
                filename => new DiffAction<PrePlaylist, MediaFile>(() => playlistFromFile,
                    () => getMediaFile(playlistFromFile.MediaFiles, filename),
                    (pl, mediaFile) =>
                    {
                        pl.MediaFiles.Add(mediaFile);
                        log.Debug("Добавлен: {0}", mediaFile);
                    }, DiffType.Added);

            Func<string, IDiffAction> onDelete =
                filename => new DiffAction<PrePlaylist, MediaFile>(() => playlistFromFile,
                    () => getMediaFile(playlist.MediaFiles, filename),
                    (pl, mediaFile) =>
                    {
                        pl.MediaFiles.Remove(mediaFile);
                        log.Debug("Удалён: {0}", mediaFile);
                    }, DiffType.Deleted);
            Func<string, string, IDiffAction> onEquals = (filename1, filename2) =>
                new DiffAction<PrePlaylist, string>(() => playlistFromFile,
                    () => filename1,
                    (pl, filename) => { log.Trace("Без изменения: {0}", filename); }, DiffType.None);

            //compare old and new version
            var oldVersion = GetFilenames(playlistFromFile);
            var newVersion = GetFilenames(playlist);
            var collectionDiff = DiffCreator.CreateCollectionDiff(oldVersion, newVersion, onAdd, onDelete, onEquals);

            var diffAction = new ContainerDiffAction<PrePlaylist>(() => playlistFromFile, collectionDiff,
                DiffType.Modified)
            {
                PreAction = () => log.Debug("Обновление плейлиста {0}", collectionDiff),
                PostAction = () =>
                {
                    Adapter.PlaylistToFile(playlistFromFile, exportFilename);
                    log.Debug("Обновление плейлиста {0} завершено", playlist);
                }
            };

            return diffAction;
        }

        private class FileComparer : IEqualityComparer<File>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            /// <param name="x">The first object of type <see cref="File"/> to compare.</param>
            /// <param name="y">The second object of type <see cref="File"/> to compare.</param>
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