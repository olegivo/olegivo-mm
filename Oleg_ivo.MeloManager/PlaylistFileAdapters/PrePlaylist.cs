using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    /// <summary>
    /// Содержит информацию о плейлисте, достаточную для создания сущности <see cref="Playlist"/>
    /// </summary>
    public class PrePlaylist : IRepairable, IMediaFilesContainer
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

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Name ?? Filename;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<MediaFile> GetEnumerator()
        {
            return MediaFiles.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return MediaFiles.GetEnumerator();
        }

        /// <summary>
        /// Починка медиа-контейнера (рекурсивно)
        /// </summary>
        /// <param name="foundFiles"></param>
        /// <param name="optionRepairOnlyBadFiles"></param>
        /// <param name="mediaCache"></param>
        public void BatchRepair(IEnumerable<string> foundFiles, bool optionRepairOnlyBadFiles, IMediaCache mediaCache)
        {
            IsRepaired = false;
            var foundFilesList = foundFiles as IList<string> ?? foundFiles.ToList();
            foreach (var mediaContainer in MediaFiles)
            {
                mediaContainer.BatchRepair(foundFilesList, optionRepairOnlyBadFiles, mediaCache);
                if (!IsRepaired)
                    IsRepaired = mediaContainer.IsRepaired;
            }
            /*
            var result = Parallel.ForEach(MediaFiles, mediaContainer =>
            {
                mediaContainer.BatchRepair(foundFilesList, optionRepairOnlyBadFiles, mediaCache);
                if (!IsRepaired)
                    IsRepaired = mediaContainer.IsRepaired;
            });
            if (!result.IsCompleted)
            {
                
            }
            */
        }

        public bool IsRepaired { get; set; }
    }
}