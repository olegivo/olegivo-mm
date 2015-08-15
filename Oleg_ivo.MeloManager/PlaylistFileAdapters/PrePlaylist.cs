using System;
using System.Collections.Generic;
using System.Linq;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    /// <summary>
    /// Содержит информацию о плейлисте, достаточную для создания сущности <see cref="Playlist"/>
    /// </summary>
    public class PrePlaylist
    {
        private readonly IMediaCache mediaCache;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public PrePlaylist(IMediaCache mediaCache)
        {
            this.mediaCache = mediaCache;
        }

        public string Name { get; set; }
        public string Filename { get; set; }
        public List<MediaFile> MediaFiles { get; set; }

        public Playlist CreatePlaylist()
        {
            Playlist playlist = null;
            if (MediaFiles != null)
            {
                playlist = new Playlist(Filename, mediaCache) {Name = Name};
                foreach (var mediaFile in MediaFiles)
                {
                    playlist.AddChildMediaFile(mediaFile);
                }
            }
            return playlist;
        }
        
        public Func<Playlist> CreatePlaylistFunc()
        {
            Playlist playlist = null;
            if (MediaFiles != null)
            {
                playlist = new Playlist(Filename, mediaCache) {Name = Name};
                var diffAction =
                    new ContainerDiffAction(
                        MediaFiles.Select(
                            mediaFile =>
                                new DiffAction<Playlist, MediaFile>(
                                    () => new Playlist(Filename, mediaCache) {Name = Name}, () => mediaFile,
                                    (pl, mf) => pl.AddChildMediaFile(mf), DiffType.Added)).Cast<IDiffAction>().ToList(),
                        DiffType.None);
            }
            return null;
        }
    }
}