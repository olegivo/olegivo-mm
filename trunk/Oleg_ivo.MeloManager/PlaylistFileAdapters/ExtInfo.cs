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

        public MediaFile GetMediaFile()
        {
            var mediaFile = new MediaFile();
            var file = File.GetFile(filename);
            mediaFile.Name = file.Filename;
            mediaFile.MediaContainerFiles.Add(new MediaContainerFile {File = file});
            return mediaFile;
        }
    }
}