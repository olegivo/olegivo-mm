using System.Collections.Generic;
using System.Linq;
using Oleg_ivo.Base.Extensions;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    partial class MediaDataContext : IMediaCache
    {
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

        public Dictionary<string, File> FilesCache { get; set; }

        public Dictionary<string, MediaFile> MediaFilesCache { get; set; }

        public Dictionary<string, Playlist> PlaylistsCache { get; set; }

        partial void OnCreated()
        {
            RefreshCache();
        }

        public void RefreshCache()
        {
            var d =
                from mediaContainer in MediaContainers.Where(mc => mc is MediaFile || mc is Playlist)
                join mcf in MediaContainerFiles on mediaContainer equals mcf.MediaContainer
                join file in Files on mcf.File equals file
                select
                    new
                    {
                        file.FullFileName,
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

            MediaFilesCache = d.Where(arg => arg.MediaFile!=null).ToDictionary(arg => arg.FullFileName, arg => arg.MediaFile);
            //PlaylistsCache = d.Where(arg => arg.Playlist!=null).ToDictionary(arg => arg.FullFileName, arg => arg.Playlist);
            FilesCache = d.Where(arg => arg.MediaFile!=null).ToDictionary(arg => arg.FullFileName, arg => arg.file);
            /*

            mediaFilesCache =
                MediaContainers.OfType<MediaFile>()
                    .SelectMany(
                        mediaFile => mediaFile.MediaContainerFiles.Select(mcf => new {mcf.File.FullFileName, mediaFile}))
                    .ToDictionary(arg => arg.FullFileName, arg => arg.mediaFile);*/
        }

        public File GetOrAddCachedFile(string fullFilename)
        {
            var key = fullFilename.ToLower();
            var file = FilesCache.GetValueOrDefault(key);
            if (file != null) return file;

            var fileName = System.IO.Path.GetFileName(fullFilename);
            var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fullFilename);
            var extension = System.IO.Path.GetExtension(fullFilename);
            var drive = System.IO.Path.GetPathRoot(fullFilename);
            var path = System.IO.Path.GetDirectoryName(fullFilename);
            file = new File
            {
                FullFileName = fullFilename,
                Drive = drive,
                Path = path,
                Filename = fileName,
                FileNameWithoutExtension = fileNameWithoutExtension,
                Extention = extension
            };
            FilesCache.Add(key, file);
            return file;
        }

        public MediaFile GetOrAddCachedMediaFile(string filename)
        {
            var key = filename.ToLower();
            var mediaFile = MediaFilesCache.GetValueOrDefault(key);
            if (mediaFile != null) return mediaFile;

            var file = GetOrAddCachedFile(filename);
            mediaFile = new MediaFile {Name = file.Filename};
            mediaFile.MediaContainerFiles.Add(new MediaContainerFile{File = file});
            MediaFilesCache.Add(key, mediaFile);
            return mediaFile;
        }

        public Playlist GetOrAddCachedPlaylist(string filename, string playlistName = null)
        {
            filename = filename.ToLower();
            var playlist = PlaylistsCache.GetValueOrDefault(filename);
            if (playlist != null) return playlist;

            var file = GetOrAddCachedFile(filename);
            playlist = new Playlist();
            playlist.MediaCache = this;
            playlist.Name = playlistName;
            //playlist.OriginalFileName = file.Filename;
            playlist.MediaContainerFiles.Add(new MediaContainerFile{File = file});
            PlaylistsCache.Add(filename, playlist);
            return playlist;
        }
    }
}
