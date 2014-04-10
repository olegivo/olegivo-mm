using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    public abstract class PlaylistFileAdapter
    {
        public abstract Playlist FileToPlaylist(string filename);
        public abstract void PlaylistToFile(Playlist playlist, string filename);

        public readonly static string[] PlaylistFilesSearchPatterns = { "*.m3u8", "*.m3u" };
    }
}