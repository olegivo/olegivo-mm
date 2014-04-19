using System.Collections.Generic;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    internal class ExtInfo
    {
        private readonly string title;
        private readonly string filename;

        public ExtInfo(string title, string filename)
        {
            this.title = title;
            this.filename = filename;
        }

        private static readonly Dictionary<string, MediaFile>  mediaFilesCache = new Dictionary<string, MediaFile>();//TODO: возможно, кешировать где-то в другом классе?

        public MediaFile GetMediaFile()
        {
            if (mediaFilesCache.ContainsKey(filename))
                return mediaFilesCache[filename];
            var mediaFile = new MediaFile();
            var file = File.GetFile(filename);
            mediaFile.Name = file.Filename;
            mediaFile.MediaContainerFiles.Add(new MediaContainerFile {File = file});
            mediaFilesCache.Add(filename, mediaFile);
            return mediaFile;
        }
    }
}