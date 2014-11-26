using System.Collections.Generic;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    /// <summary>
    /// Содержит информацию о плейлисте, достаточную для создания сущности <see cref="Playlist"/>
    /// </summary>
    public class PrePlaylist
    {
        public string Name { get; set; }
        public string Filename { get; set; }
        public List<MediaFile> MediaFiles { get; set; }

        public Playlist CreatePlaylist()
        {
            Playlist playlist = null;
            if (MediaFiles != null)
            {
                playlist = new Playlist { OriginalFileName = Filename, Name = Name };
                foreach (var mediaFile in MediaFiles)
                {
                    playlist.AddChildMediaFile(mediaFile);
                }
            }
            return playlist;
        }
    }
}