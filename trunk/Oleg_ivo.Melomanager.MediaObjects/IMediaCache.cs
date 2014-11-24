using System.Collections.Generic;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    public interface IMediaCache
    {
        Dictionary<string, MediaFile> MediaFilesCache { get; set; }
        void RefreshCache();
        File GetOrAddCachedFile(string fullFilename);
        MediaFile GetOrAddCachedMediaFile(string filename);
        Playlist GetOrAddCachedPlaylist(string filename, string playlistName = null);
    }
}