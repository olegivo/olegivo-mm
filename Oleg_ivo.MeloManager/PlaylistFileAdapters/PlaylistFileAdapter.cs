using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NLog;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    public abstract class PlaylistFileAdapter
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public abstract PrePlaylist FileToPlaylist(string filename, string playlistName = null);

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public void PlaylistToFile(IEnumerable<MediaFile> mediaFilesContainer, string filename)
        {
            log.Info("Запись плейлиста [{0}] в файл [{1}]", mediaFilesContainer, filename);
            try
            {
                MediaFilesToFile(filename, mediaFilesContainer);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public readonly static string[] PlaylistFilesSearchPatterns = { "*.m3u8", "*.m3u" };//TODO: на своём ли оно месте?
        public abstract void MediaFilesToFile(string filename, IEnumerable<MediaFile> mediaFiles);
    }
}