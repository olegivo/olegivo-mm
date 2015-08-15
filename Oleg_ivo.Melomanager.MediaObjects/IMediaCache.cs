namespace Oleg_ivo.MeloManager.MediaObjects
{
    public interface IMediaCache
    {
        void RefreshCache();
        File GetOrAddCachedFile(string fullFilename);
        MediaFile GetOrAddCachedMediaFile(string filename);
        Playlist GetOrAddCachedPlaylist(string filename, string playlistName = null);
    }
}