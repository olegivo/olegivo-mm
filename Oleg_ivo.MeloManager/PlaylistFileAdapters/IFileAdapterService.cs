using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    public interface IFileAdapterService
    {
        void RequestImport(string importFilename);
        void RequestExport(Playlist playlist, string exportFilename);
    }
}