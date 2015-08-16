using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    public abstract class PlaylistFileAdapter
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public abstract PrePlaylist FileToPlaylist(string filename, string playlistName = null);

        public void PlaylistToFile(Playlist playlist, string filename)
        {
            log.Info("Запись плейлиста [{0}] в файл [{1}]", playlist, filename);
            try
            {
                var mediaFiles = playlist.ChildContainers.Cast<MediaFile>();
                MediaFilesToFile(filename, mediaFiles);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public readonly static string[] PlaylistFilesSearchPatterns = { "*.m3u8", "*.m3u" };
        public abstract void MediaFilesToFile(string filename, IEnumerable<MediaFile> mediaFiles);
    }
}