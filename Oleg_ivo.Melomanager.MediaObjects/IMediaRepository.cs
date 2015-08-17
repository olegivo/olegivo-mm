using System.Data.Entity;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    public interface IMediaRepository : IMediaCache
    {
        DbSet<MediaContainer> MediaContainers { get; set; }
        DbSet<File> Files { get; set; }
        DbSet<Category> Categories { get; set; }
        DbSet<Playlist> Playlists { get; set; }
        DbSet<MediaFile> MediaFiles { get; set; }
        Category CreateCategory();
        Playlist CreatePlaylist(string originalFileName, IMediaCache mediaCache);
        MediaFile CreateMediaFile();

        /// <summary>
        /// Удаляет связь между родительским и дочерним контейнером
        /// </summary>
        void RemoveRelation(MediaContainer parent, MediaContainer child);

        bool RemoveIfOrphan(MediaContainer mediaContainer);
    }
}