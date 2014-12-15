using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NLog;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Autofac.DependencyInjection;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.MeloManager.Extensions;
using Oleg_ivo.MeloManager.MediaObjects;
using Oleg_ivo.MeloManager.Prism;
using Oleg_ivo.Tools.Utils;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    public class PlaylistImporter<TPlaylistFileAdapter> where TPlaylistFileAdapter : PlaylistFileAdapter
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        public PlaylistImporter(IComponentContext context, MediaDataContext dataContext, MeloManagerOptions options, IMediaCache mediaCache)
        {
            Adapter = context.ResolveUnregistered<TPlaylistFileAdapter>();
            DataContext = Enforce.ArgumentNotNull(dataContext, "dataContext");
            Options = Enforce.ArgumentNotNull(options, "options");
            MediaCache = Enforce.ArgumentNotNull(mediaCache, "mediaCache");
        }

        public TPlaylistFileAdapter Adapter { get; private set; }
        
        public MediaDataContext DataContext { get; private set; }

        public MeloManagerOptions Options { get; private set; }

        public IMediaCache MediaCache { get; private set; }

        public IEnumerable<Playlist> RunImportAll(IEnumerable<string> playlistFilenames, Category winampCategory)
        {
            var playlists = playlistFilenames.Select(filename => Import(filename, winampCategory)).ToList();
            if (winampCategory.Id == 0)
                DataContext.MediaContainers.InsertOnSubmit(winampCategory);

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
            DataContext.SubmitChangesWithLog();
            return playlists;
        }

        public Playlist Import(string filename, Category importCategory = null)
        {
            log.Debug("Импорт плейлиста из файла {0}{1}", filename, importCategory!=null ? string.Format(@" в категорию ""{0}""", importCategory.Name) : null);
            var usage = Utils.FileUtils.GetEnvironmentVariableUsage(filename);
            List<Playlist> playlists;
            Playlist playlist;
            if (usage != null)
            {
                //плейлист попытается добавиться, а не обновлён, если на разных компьютерах он хранится в одном и том же файле, но в разных папках (AppData)
                var currentUser = Environment.UserName.ToLower();
                var otherUsers = Options.Users.OfType<string>().Where(user => user!=currentUser).ToList();

                var wrapFunction = new Func<string, string>(source => source.Replace(usage.VariableValue, string.Format("%{0}%", usage.VariableName)));
                var wrappedFilename = wrapFunction(filename);
                playlists = DataContext.MediaContainers.OfType<Playlist>()
                    .Where(p => p != null)
                    .AsEnumerable()
                    .Where(p => p.MediaContainerFiles.Any(mcf =>
                    {
                        var fullFileName = mcf.File.FullFileName;
                        return wrapFunction(fullFileName) == wrappedFilename ||
                               otherUsers.Any(
                                   otherUser =>
                                       wrapFunction(fullFileName.Replace(otherUser, currentUser)) == wrappedFilename);
                    }))
                    .ToList();
                playlist = playlists.FirstOrDefault();
                if (playlist != null 
                    && (playlist.OriginalFileName==null || wrapFunction(playlist.OriginalFileName) != wrappedFilename))
                {
                    playlist.MediaContainerFiles.Add(new MediaContainerFile{File = MediaCache.GetOrAddCachedFile(filename)});
                }
            }
            else
            {
                playlists = DataContext.MediaContainers.OfType<Playlist>()
                    .Where(p => p != null)
                    .AsEnumerable()
                    .Where(p => p.OriginalFileName == filename)
                    .ToList();
                playlist = playlists.FirstOrDefault();
            }

            var playlistFromFile = Adapter.FileToPlaylist(filename);
            if (playlist!=null)
            {
                UpdatePlaylist(playlist, playlistFromFile);
            }
            else
            {
                playlist = AddPlaylist(playlistFromFile, importCategory);
            }
            return playlist;
        }

        private void UpdatePlaylist(Playlist playlist, PrePlaylist playlistFromFile)
        {
            log.Debug("Обновление плейлиста {0}", playlist);

            //compare old and new version
            var filesWas = GetFilenames(playlist.MediaFiles);
            var filesNow = GetFilenames(playlistFromFile.MediaFiles);
            var foj = filesWas.FullOuterJoin(filesNow);
            Func<IEnumerable<MediaFile>, string, MediaFile> getMediaFile =
                (mediaFiles, filename) => mediaFiles.First(mf => mf.MediaContainerFiles.Any(mcf => String.Compare(mcf.File.FullFileName, filename, StringComparison.InvariantCultureIgnoreCase)==0));
            foreach (var item in foj)
            {
                if (item.Item1 == null)
                {
                    var mediaFile = getMediaFile(playlistFromFile.MediaFiles, item.Item2);
                    playlist.AddChildMediaFile(mediaFile);
                    log.Debug("Добавлен: {0}", mediaFile);
                }
                else if (item.Item2 == null)
                {
                    var mediaFile = getMediaFile(playlist.MediaFiles, item.Item1);
                    DataContext.RemoveRelation(playlist, mediaFile);
                    log.Debug("Удалён: {0}", mediaFile);
                }
                else
                {
                    log.Trace("Без изменения: {0}", item.Item2);
                }
            }
        }

        private Playlist AddPlaylist(PrePlaylist playlistFromFile, Category importCategory)
        {
            log.Debug("Добавление плейлиста {0}", playlistFromFile);

            var rootCategory = importCategory ??
                               DataContext.MediaContainers.OfType<Category>().FirstOrDefault(c => c.IsRoot);
            if (rootCategory == null)
            {
                log.Debug("Добавление корневой категории");

                rootCategory = new Category { Name = "Плейлисты Winamp" };
                DataContext.MediaContainers.InsertOnSubmit(rootCategory);
            }
            var playlist = playlistFromFile.CreatePlaylist();
            var file = DataContext.GetOrAddCachedFile(playlist.OriginalFileName);
            playlist.MediaContainerFiles.Add(new MediaContainerFile { File = file });
            //playlist.OriginalFileName = playlistFromFile.Filename;

            rootCategory.AddChild(playlist);
            DataContext.MediaContainers.InsertOnSubmit(playlist);
            return playlist;
        }

        private IEnumerable<string> GetFilenames(IEnumerable<MediaFile> mediaFiles)
        {
            return
                mediaFiles.SelectMany(mediaFile => mediaFile.MediaContainerFiles.Select(mcf => mcf.File))
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
                return mediaFile != null ? mediaFile.MediaContainerFiles.Select(mcf => mcf.File) : Enumerable.Empty<File>();
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