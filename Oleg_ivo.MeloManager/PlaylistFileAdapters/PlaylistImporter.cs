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
using File = Oleg_ivo.MeloManager.MediaObjects.File;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    public class PlaylistImporter<TPlaylistFileAdapter> where TPlaylistFileAdapter : PlaylistFileAdapter
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        public PlaylistImporter(IComponentContext context, MediaDataContext dataContext)
        {
            Adapter = context.ResolveUnregistered<TPlaylistFileAdapter>();
            DataContext = Enforce.ArgumentNotNull(dataContext, "dataContext");
        }

        public MediaDataContext DataContext { get; private set; }

        public TPlaylistFileAdapter Adapter { get; private set; }

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
            var playlists =
                DataContext.MediaContainers.OfType<Playlist>()
                    .Where(playlist => playlist != null)
                    .AsEnumerable()
                    .Where(playlist => playlist.OriginalFileName == filename)
                    .ToList();

            var playlistFromFile = Adapter.FileToPlaylist(filename);
            var retPlaylist = playlists.FirstOrDefault();
            if (retPlaylist!=null)
            {
                UpdatePlaylist(retPlaylist, playlistFromFile);
            }
            else
            {
                AddPlaylist(playlistFromFile, importCategory);
                retPlaylist = playlistFromFile;
            }
            return retPlaylist;
        }

        private void UpdatePlaylist(Playlist playlist, Playlist playlistFromFile)
        {
            log.Debug("Обновление плейлиста {0}", playlist);

            //compare old and new version
            var filesWas = GetFiles(playlist);
            var filesNow = GetFiles(playlistFromFile);
            var foj = filesWas.FullOuterJoin(filesNow);
            Func<Playlist, string, MediaFile> getMediaFile =
                (p, f) => p.MediaFiles.First(mf => mf.MediaContainerFiles.Any(mcf => String.Compare(mcf.File.FullFileName, f, StringComparison.InvariantCultureIgnoreCase)==0));
            foreach (var item in foj)
            {
                if (item.Item1 == null)
                {
                    var mediaFile = getMediaFile(playlistFromFile, item.Item2);
                    ((MediaContainer)mediaFile).ParentMediaContainers.Single().ParentMediaContainer = playlist;
                    log.Debug("Добавлен: {0}", mediaFile);
                }
                else if (item.Item2 == null)
                {
                    var mediaFile = getMediaFile(playlist, item.Item1);
                    DataContext.RemoveRelation(playlist, mediaFile);
                    log.Debug("Удалён: {0}", mediaFile);
                }
                else
                {
                    log.Trace("Без изменения: {0}", item.Item2);
                }
            }
        }

        private void AddPlaylist(Playlist playlistFromFile, Category importCategory)
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
            rootCategory.AddChild(playlistFromFile);
            DataContext.MediaContainers.InsertOnSubmit(playlistFromFile);
        }

        private IEnumerable<string> GetFiles(Playlist playlist)
        {
            return
                playlist.MediaFiles.SelectMany(mediaFile => mediaFile.MediaContainerFiles.Select(mcf => mcf.File))
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