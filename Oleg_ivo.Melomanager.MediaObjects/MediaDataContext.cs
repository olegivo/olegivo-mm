using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NLog;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.Tools.Utils;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    partial class MediaDataContext : IMediaCache
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public Category CreateCategory()
        {
            var category = new Category();
            MediaContainers.InsertOnSubmit(category);
            return category;
        }

        public Playlist CreatePlaylist()
        {
            var playlist = new Playlist();
            MediaContainers.InsertOnSubmit(playlist);
            return playlist;
        }

        public MediaFile CreateMediaFile()
        {
            var mediaFile = new MediaFile();
            MediaContainers.InsertOnSubmit(mediaFile);
            return mediaFile;
        }

        /// <summary>
        /// ”дал€ет св€зь между родительским и дочерним контейнером
        /// </summary>
        public void RemoveRelation(MediaContainer parent, MediaContainer child)
        {
            var parentRelation = parent.GetChildRelation(child);
            var childRelation = child.GetParentRelation(parent);
            parent.RemoveChild(child);
            child.RemoveParent(parent);

            //удаление св€зи, т.к. она сама себ€ не удал€ет
            parent.ChildMediaContainers.Remove(parentRelation);
            child.ParentMediaContainers.Remove(childRelation);
            MediaContainersParentChilds.DeleteOnSubmit(parentRelation);
            MediaContainersParentChilds.DeleteOnSubmit(childRelation);
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

        private ConcurrentDictionary<string, FileExt> filesCache;
        private ConcurrentDictionary<string, MediaFile> mediaFilesCache;
        private ConcurrentDictionary<string, Playlist> playlistsCache;

        partial void OnCreated()
        {
            using (new ElapsedAction(elapsed => log.Debug("Caches filled in {0}", elapsed))) RefreshCache();
        }

        

        public void RefreshCache()
        {
            MediaContainers.Load();
            var d =
                from mediaContainer in MediaContainers.Where(mc => mc is MediaFile || mc is Playlist)
                join mcf in MediaContainerFiles on mediaContainer equals mcf.MediaContainer
                join file in Files on mcf.File equals file
                select
                    new
                    {
                        FullFileName = file.FullFileName.ToLower(),
                        MediaFile = mediaContainer as MediaFile,
                        Playlist = mediaContainer as Playlist,
                        file
                    };

            //var list = d.ToList();
            //mediaFilesCache = new Dictionary<string, MediaFile>();
            //foreach (var arg in list)
            //{
            //    mediaFilesCache.Add(arg.FullFileName, arg.mediaFile);
            //}

            mediaFilesCache =
                new ConcurrentDictionary<string, MediaFile>(
                    d.Where(arg => arg.MediaFile != null)
                        .AsParallel()
                        .ToDictionary(arg => arg.FullFileName, arg => arg.MediaFile));
            //PlaylistsCache = d.Where(arg => arg.Playlist!=null).ToDictionary(arg => arg.FullFileName, arg => arg.Playlist);
            //FilesCache = d.GroupBy(arg => arg.FullFileName.ToLower()).ToDictionary(g => g.Key, g => g.First().file);
            //FilesCache = d.GroupBy(arg => arg.FullFileName.ToLower()).ToDictionary(g => g.Key, g => g.Single().file);
            filesCache =
                new ConcurrentDictionary<string, FileExt>(d.AsParallel()
                    .ToDictionary(arg => arg.FullFileName, arg => new FileExt(arg.file)));
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
                    return new FileExt(new File
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
                    mediaFile = new MediaFile {Name = file.Filename};
                    mediaFile.MediaContainerFiles.Add(new MediaContainerFile {File = file});
                    return mediaFile;
                });
        }

        public Playlist GetOrAddCachedPlaylist(string filename, string playlistName = null)
        {
            return playlistsCache.GetOrAdd(filename.ToLower(), 
                key => new Playlist(filename, this) {Name = playlistName});
        }
    }
}
